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
using NUnit.Framework;
using Unity.Burst;
using UnityEditor;
using UnityEngine;
namespace NeroWeNeed.UIDots {
    public class UIGlobalSettings : ScriptableObject, IEnumerable<UIGlobalSettings.SchemaElement> {
        public const string ASSET_DIRECTORY = "Assets/Settings";
        public const string ASSET_LOCATION = ASSET_DIRECTORY + "/UISettings.asset";
        public List<SchemaElement> elements;
        public ReadOnlyDictionary<string, SchemaElement> entryView = null;
        public ReadOnlyDictionary<string, SchemaElement> Elements
        {
            get
            {
                return entryView ??= new ReadOnlyDictionary<string, SchemaElement>(elements.ToDictionary(k => k.identifier));
            }
        }
        public string outputPath;
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
        public void Refresh() {
#if UNITY_EDITOR
            var newEntries = Collect(AppDomain.CurrentDomain.GetAssemblies());
            this.elements.Clear();
            this.elements.AddRange(newEntries.Values);
            //this.entryView = new ReadOnlyDictionary<string, Element>(entries.ToDictionary(k => k.identifier));
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        //TODO: Ensure proper delegates can be created from methods.
        private static Dictionary<string, SchemaElement> Collect(params Assembly[] assemblies) {
            var entries = new Dictionary<string, SchemaElement>();
            foreach (var assembly in assemblies) {
                foreach (var type in assembly.GetLoadableTypes()) {
                    if (type.IsSealed && type.IsAbstract && type.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        foreach (var attribute in type.GetCustomAttributes<UIDotsElementAttribute>().AsEnumerable()) {
                            if (entries.ContainsKey(attribute.Identifier)) {
                                Debug.LogError($"Duplicate identifier '{attribute.Identifier}' found. Skipping.");
                                continue;
                            }
                            if (CreateElement(attribute, type, out SchemaElement element)) {
                                entries[attribute.Identifier] = element;
                            }
                        }
                    }
                }
            }
            return entries;
        }
        private static bool CreateElement(UIDotsElementAttribute attribute, Type type, out SchemaElement element) {
            element = default;
            if (attribute.LayoutPass == null || attribute.RenderPass == null) {
                return false;
            }
            element = new SchemaElement
            {
                identifier = attribute.Identifier,
                mask = attribute.Mask,
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
        public IEnumerator<SchemaElement> GetEnumerator() {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return elements.GetEnumerator();
        }
        [Serializable]
        public struct SchemaElement {
            public string identifier;
            public ulong mask;
            public SerializableMethod layoutPass;
            public SerializableMethod renderPass;
            public SerializableMethod renderBoxCounter;
        }
    }

}