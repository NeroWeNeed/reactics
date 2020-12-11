using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using NeroWeNeed.Commons.Editor;
using Newtonsoft.Json;
using Unity.Burst;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public class UISchema {
        [JsonProperty]
        private readonly Dictionary<string, Element> elements;
        [JsonIgnore]
        public ReadOnlyDictionary<string, Element> Elements;
        public UISchema() {
            elements = new Dictionary<string, Element>();
            Elements = new ReadOnlyDictionary<string, Element>(elements);
        }
        public static UISchema Collect(params Assembly[] assemblies) {
            var schema = new UISchema();
            foreach (var assembly in assemblies) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsSealed && type.IsAbstract && type.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
                            var attributes = method.GetCustomAttributes<UIDotsElementAttribute>();
                            if (method.GetCustomAttribute<BurstCompileAttribute>() != null && attributes != null) {
                                foreach (var attribute in attributes) {
                                    if (schema.elements.ContainsKey(attribute.Identifier)) {
                                        Debug.LogError($"Duplicate identifier '{attribute.Identifier}' found. Skipping.");
                                        continue;
                                    }
                                    schema.elements[attribute.Identifier] = new Element
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
            return schema;
        }
        public static UISchema Collect() => Collect(AppDomain.CurrentDomain.GetAssemblies());

        public static UISchema FromJson(string json) => JsonConvert.DeserializeObject<UISchema>(json, new SerializableTypeConverter());
        public string ToJson(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting, new SerializableTypeConverter());
        [Serializable]
        public struct Element {
            public string identifier;
            public string displayName;
            public SerializableType config;
            public SerializableMethod pass;
        }

    }

    public class UISchemaAsset : TextAsset, IEnumerable<UISchema.Element> {
        public const string ASSET_LOCATION = "Assets/UIDots/UISchema.json";
        public const string ASSET_FOLDER = "UIDots";

        [MenuItem("DOTS/UIDots/Refresh Schema")]
        public static void RefreshData() {
            if (!Directory.Exists("Assets/" + ASSET_FOLDER))
                AssetDatabase.CreateFolder("Assets", ASSET_FOLDER);
            var schema = new UISchemaAsset(UISchema.Collect().ToJson());
            AssetDatabase.CreateAsset(schema, ASSET_LOCATION);
            AssetDatabase.SaveAssets();

        }
        private UISchema schema;
        public ReadOnlyDictionary<string, UISchema.Element> Elements { get => schema.Elements; }

        public UISchemaAsset(string text) : base(text) {
            this.schema = UISchema.FromJson(text);
        }
        public UISchemaAsset(UISchema schema) : base(schema.ToJson()) {
            this.schema = schema;
        }

        public static UISchemaAsset Schema
        {
            get
            {
                var schema = AssetDatabase.LoadAssetAtPath<UISchemaAsset>(ASSET_LOCATION);
                if (schema == null) {
                    if (!Directory.Exists("Assets/" + ASSET_FOLDER))
                        AssetDatabase.CreateFolder("Assets", ASSET_FOLDER);
                    schema = new UISchemaAsset(UISchema.Collect().ToJson());
                    AssetDatabase.CreateAsset(schema, ASSET_LOCATION);
                    AssetDatabase.SaveAssets();
                }
                return schema;
            }
        }

        public IEnumerator<UISchema.Element> GetEnumerator() {
            return schema.Elements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return schema.Elements.Values.GetEnumerator();
        }
    }
}