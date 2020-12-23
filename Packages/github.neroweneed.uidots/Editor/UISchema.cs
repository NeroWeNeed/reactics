using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using NeroWeNeed.Commons;
using Unity.Burst;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class UISchema : ScriptableObject, IEnumerable<UISchema.Element> {
        public const string ASSET_LOCATION = "Assets/UIDots/UISchema.asset";
        public const string ASSET_FOLDER = "UIDots";
        [MenuItem("DOTS/UIDots/Refresh Schema")]
        public static void RefreshData() {
            Default.Refresh();
        }
        private static UISchema schema = null;
        public static UISchema Default
        {
            get
            {
                if (schema == null) {
                    var newSchema = AssetDatabase.LoadAssetAtPath<UISchema>(ASSET_LOCATION);
/*                     if (newSchema == null) {
                        if (!Directory.Exists("Assets/" + ASSET_FOLDER))
                            AssetDatabase.CreateFolder("Assets", ASSET_FOLDER);
                        newSchema = ScriptableObject.CreateInstance<UISchema>();
                        newSchema.name = "Default UI Schema";
                        newSchema.Refresh();
                        AssetDatabase.CreateAsset(newSchema, ASSET_LOCATION);
                        AssetDatabase.SaveAssets();
                    } */
                    schema = newSchema;

                }
                return schema;


            }
        }
        [SerializeField]
        public List<Element> entries = new List<Element>();
        private bool entryViewUpToDate;

        public ReadOnlyDictionary<string, Element> entryView = null;
        public ReadOnlyDictionary<string, Element> Entries
        {
            get
            {
                if (entryView == null || !entryViewUpToDate) {
                    entryView = new ReadOnlyDictionary<string, Element>(entries.ToDictionary(k => k.identifier));
                    entryViewUpToDate = true;
                }
                return entryView;
            }
        }
        public void Refresh() {
            var newEntries = Collect(AppDomain.CurrentDomain.GetAssemblies());
            this.entries.Clear();
            this.entries.AddRange(newEntries.Values);
            this.entryView = new ReadOnlyDictionary<string, Element>(entries.ToDictionary(k => k.identifier));
            EditorUtility.SetDirty(this);
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
                                        mask = attribute.Mask,
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
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return entries.GetEnumerator();
        }
        [Serializable]
        public struct Element {
            public string identifier;
            public string displayName;
            public ulong mask;
            public SerializableMethod pass;
        }
    }

}