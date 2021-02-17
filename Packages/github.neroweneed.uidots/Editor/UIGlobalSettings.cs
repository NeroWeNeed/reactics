using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using AOT;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using NeroWeNeed.UIDots.Editor;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
namespace NeroWeNeed.UIDots {
    public class UIGlobalSettings : ScriptableObject {
        public const string ASSET_DIRECTORY = "Assets/Settings";
        public const string DEFAULT_SCHEMA_LOCATION = "Assets/Resources/UI/UISchema.asset";
        public const string DEFAULT_SCHEMA_DIRECTORY = "Assets/Resources/UI";
        public const string ASSET_LOCATION = ASSET_DIRECTORY + "/UISettings.asset";
        public ReadOnlyDictionary<string, UISchema.Element> entryView = null;
        public ReadOnlyDictionary<string, UISchema.Element> Elements
        {
            get
            {
                return entryView ??= new ReadOnlyDictionary<string, UISchema.Element>(schema?.elements.ToDictionary(k => k.identifier));
            }
        }
        public string outputPath;

        public UISchema schema;
        public string assetGroupCachePath;
        public static SerializedObject GetOrCreateSerializedSettings() => new SerializedObject(GetOrCreateSettings());
        public static UIGlobalSettings GetOrCreateSettings() {
            var value = AssetDatabase.LoadAssetAtPath<UIGlobalSettings>(ASSET_LOCATION);
            if (value == null) {
                value = ScriptableObject.CreateInstance<UIGlobalSettings>();

                value.outputPath = "Assets/Resources/UI";
                value.assetGroupCachePath = "Assets/Resources/UI/AssetGroups";
                Directory.CreateDirectory(ASSET_DIRECTORY);
                AssetDatabase.CreateAsset(value, ASSET_LOCATION);
                AssetDatabase.SaveAssets();
            }
            return value;
        }

        private void Awake() {
            /*             if (schema == null) {
                            schema = ScriptableObject.CreateInstance<UISchema>();
                            schema.name = "UISchema";
                            var di = new FileInfo(DEFAULT_SCHEMA_LOCATION);
                            di.Directory.Create();
                            AssetDatabase.CreateAsset(schema, DEFAULT_SCHEMA_LOCATION);
                            AssetDatabase.SaveAssets();
                        } */
        }
        public void Refresh() {
            var newEntries = CollectElements(AppDomain.CurrentDomain.GetAssemblies());
            schema.elements.Clear();
            schema.elements.AddRange(newEntries.Values);
            var newInheritableFields = CollectInheritableFields();
            schema.inheritableFields.Clear();
            schema.inheritableFields.AddRange(newInheritableFields);
            this.entryView = new ReadOnlyDictionary<string, UISchema.Element>(schema.elements.ToDictionary(k => k.identifier));
            UnityEditor.EditorUtility.SetDirty(schema);
            foreach (var modelGuid in AssetDatabase.FindAssets($"t:{nameof(UIModel)}")) {
                var model = AssetDatabase.LoadAssetAtPath<UIModel>(AssetDatabase.GUIDToAssetPath(modelGuid));
                if (model != null)
                    model.Write();
            }
            AssetDatabase.SaveAssets();
        }
        //TODO: Ensure proper delegates can be created from methods.
        private static Dictionary<string, UISchema.Element> CollectElements(params Assembly[] assemblies) {
            var entries = new Dictionary<string, UISchema.Element>();
            foreach (var assembly in assemblies) {
                foreach (var type in assembly.GetLoadableTypes()) {
                    if (type.IsSealed && type.IsAbstract && type.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        foreach (var attribute in type.GetCustomAttributes<UIDotsElementAttribute>().AsEnumerable()) {
                            if (entries.ContainsKey(attribute.Identifier)) {
                                Debug.LogError($"Duplicate identifier '{attribute.Identifier}' found. Skipping.");
                                continue;
                            }
                            if (CreateElement(attribute, type, out UISchema.Element element)) {
                                entries[attribute.Identifier] = element;
                            }
                        }
                    }
                }
            }
            return entries;
        }

        private static List<UISchema.InheritableField> CollectInheritableFields() {
            var fields = new List<UISchema.InheritableField>();
            for (byte i = 0; i < UIConfigTypeTable.Types.Length; i++) {
                
                CollectInheritableFields(UIConfigTypeTable.Types[i], i, 0, fields);
            }
            return fields;
        }
        private static void CollectInheritableFields(Type type, byte config, int offset, List<UISchema.InheritableField> fields) {
            
            if (type == typeof(UILength)) {
                
                fields.Add(new UISchema.InheritableField
                {
                    config = config, offset = offset, length = UnsafeUtility.SizeOf<UILength>()
                });
            }
            else {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                    CollectInheritableFields(field.FieldType, config, offset + UnsafeUtility.GetFieldOffset(field), fields);
                }
            }
        }
        private static bool CreateElement(UIDotsElementAttribute attribute, Type type, out UISchema.Element element) {
            element = default;
            if (attribute.LayoutPass == null || attribute.RenderPass == null) {
                return false;
            }
            element = new UISchema.Element
            {
                identifier = attribute.Identifier,
                requiredBlockMask = (ulong)attribute.ConfigBlocks,
                optionalBlockMask = (ulong)attribute.OptionalConfigBlocks,
                layoutPass = GetBurstMethod<UILayoutPass>(type, attribute.LayoutPass),
                renderPass = GetBurstMethod<UIRenderPass>(type, attribute.RenderPass),
                renderBoxCounter = TryGetBurstMethod<UIRenderBoxCounter>(type, attribute.RenderBoxCounter)
            };
            return true;
        }
        private static MethodInfo TryGetBurstMethod<TDelegate>(Type type, string name) where TDelegate : Delegate {
            if (string.IsNullOrEmpty(name))
                return null;
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null || method.GetCustomAttribute<BurstCompileAttribute>() == null || method.GetCustomAttribute<MonoPInvokeCallbackAttribute>() == null)
                return null;
            try {
                var del = method.CreateDelegate(typeof(TDelegate));
                return method;
            }
            catch (Exception) {
                return null;
            }
        }
        private static MethodInfo GetBurstMethod<TDelegate>(Type type, string name) where TDelegate : Delegate {
            Contract.Requires(!string.IsNullOrEmpty(name));
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Contract.Ensures(method != null, $"Unable to find Method {name} in Type {type.FullName}.");
            Contract.Ensures(method.GetCustomAttribute<BurstCompileAttribute>() != null, $"Method {type.FullName}#{name} is missing {nameof(BurstCompileAttribute)}. Skipping.");
            Contract.Ensures(method.GetCustomAttribute<MonoPInvokeCallbackAttribute>() != null, $"Method {type.FullName}#{name} is missing {nameof(MonoPInvokeCallbackAttribute)}. Skipping.");
            Contract.Ensures(method.CreateDelegate(typeof(TDelegate)) != null, $"Unable to create Delegate {nameof(TDelegate)} from Method {type.FullName}#{name}. Skipping");
            return method;
        }

    }

}