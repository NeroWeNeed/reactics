using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using NeroWeNeed.Commons;
using Unity.Burst;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class UISchema : ScriptableObject, IEnumerable<UISchema.Element> {
        public const string ASSET_LOCATION = "Assets/UIDots/UISchema.asset";
        public const string ASSET_FOLDER = "UIDots";
        [MenuItem("DOTS/UIDots/Refresh Schema")]
        public static void RefreshData() {
            Default.Refresh();
        }
        public static UISchema Default
        {
            get
            {
                var schema = AssetDatabase.LoadAssetAtPath<UISchema>(ASSET_LOCATION);
                if (schema == null) {
                    if (!Directory.Exists("Assets/" + ASSET_FOLDER))
                        AssetDatabase.CreateFolder("Assets", ASSET_FOLDER);
                    schema = ScriptableObject.CreateInstance<UISchema>();
                    schema.name = "Default UI Schema";
                    schema.Refresh();
                    AssetDatabase.CreateAsset(schema, ASSET_LOCATION);
                    AssetDatabase.SaveAssets();
                }
                return schema;
            }
        }


        [SerializeField]
        private List<Element> entries = new List<Element>();

        public ReadOnlyDictionary<string, Element> entryView = null;
        public ReadOnlyDictionary<string, Element> Entries
        {
            get
            {
                return entryView ??= new ReadOnlyDictionary<string, Element>(entries.ToDictionary(k => k.identifier));
            }
        }
        public void Refresh() {
            var newEntries = Collect(AppDomain.CurrentDomain.GetAssemblies());
            this.entries.Clear();
            this.entries.AddRange(newEntries.Values);
            this.entryView = new ReadOnlyDictionary<string, Element>(newEntries);
        }
        public static Dictionary<string, Element> Collect(params Assembly[] assemblies) {
            var entries = new Dictionary<string, Element>();
            foreach (var assembly in assemblies) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsSealed && type.IsAbstract && type.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                            var attributes = method.GetCustomAttributes<UIDotsElementAttribute>();
                            if (method.GetCustomAttribute<BurstCompileAttribute>() != null && attributes != null) {
                                foreach (var attribute in attributes) {
                                    if (entries.ContainsKey(attribute.Identifier)) {
                                        Debug.LogError($"Duplicate identifier '{attribute.Identifier}' found. Skipping.");
                                        continue;
                                    }
                                    entries[attribute.Identifier] = new Element
                                    {
                                        identifier = attribute.Identifier,
                                        displayName = attribute.Identifier,
                                        config = attribute.ConfigurationType,
                                        pass = method
                                    };
                                }
                            }
                        }
                    }
                }
            }
            return entries;
        }
        public IEnumerator<UISchema.Element> GetEnumerator() {
            return Entries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Entries.Values.GetEnumerator();
        }
        [Serializable]
        public struct Element {
            public string identifier;
            public string displayName;
            public SerializableType config;
            public SerializableMethod pass;
        }
    }

}