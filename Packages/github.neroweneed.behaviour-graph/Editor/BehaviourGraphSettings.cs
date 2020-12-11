using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public class BehaviourGraphSettings : CompileableTableObject {

        public List<AssemblyDefinitionAsset> assemblies = new List<AssemblyDefinitionAsset>();
        public string behaviourName;
        public List<BehaviourEntry> behaviours;

        [SerializeField]
        private List<ModelEntry> models = new List<ModelEntry>();

        public List<ModelEntry> Models => models;
        [ConcreteTypeFilter]
        [UnmanagedFilter]
        [AttributeTypeFilter(typeof(VariableDefinitionAttribute))]
        public SerializableType variableDefinition;

        [ConcreteTypeFilter]
        [SuperTypeFilter(typeof(BehaviourGraphCompiler))]
        [ParameterlessConstructorFilter]
        public SerializableType compiler;
        [ConcreteTypeFilter]
        [SuperTypeFilter(typeof(IBehaviourProvider))]
        [ParameterlessConstructorFilter]
        public SerializableType provider;


        public bool automaticRefresh = true;

        [NonSerialized]
        private Type behaviourType;
        public Type BehaviourType => behaviourType ??= BehaviourGraphGlobalSettings.Settings.Entries.Values.FirstOrDefault(data => data.asset == this).type.Value;
        private ReadOnlyDictionary<string, BehaviourEntry> behaviourView;
        public ReadOnlyDictionary<string, BehaviourEntry> Behaviours => behaviourView ??= new ReadOnlyDictionary<string, BehaviourEntry>(behaviours.ToDictionary(k => k.identifier));

        internal Assembly[] CollectAssembiles() {
            var names = new List<string>();
            for (int i = 0; i < assemblies.Count; i++) {
                AssemblyDefinitionAsset asmDef = assemblies[i];
                if (asmDef == null)
                    continue;
                var asmDefData = JsonUtility.FromJson<AssemblyDefinitionData>(asmDef.text);
                names.Add(asmDefData.name);
            }
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => names.Contains(a.GetName().Name)).ToArray();
        }
        public void RefreshData() {
            if (this.provider.Value == null) {
                Debug.LogError($"Provider for Behaviour Type {this.behaviourType} is not set.");
                return;
            }
            var provider = Activator.CreateInstance(this.provider.Value) as IBehaviourProvider;
            if (provider == null) {
                Debug.LogError($"Unable to create provider for Behaviour Type {this.behaviourType}.");
                return;
            }
            var assemblies = CollectAssembiles();
            var behaviours = new Dictionary<string, BehaviourEntry>();
            var buffer = new List<BehaviourInfo>();
            foreach (var assembly in assemblies) {
                provider.Collect(BehaviourType, assembly, buffer);
            }
            foreach (var behaviourInfo in buffer) {
                if (behaviours.ContainsKey(behaviourInfo.identifier)) {
                    Debug.LogError($"Duplicate Identifier '{behaviourInfo.identifier}' for Behaviour '{behaviourInfo.method.Name}' in '{behaviourInfo.method.DeclaringType.AssemblyQualifiedName}' found. Skipping");
                    continue;
                }
                behaviours[behaviourInfo.identifier] = new BehaviourEntry
                {
                    displayName = behaviourInfo.displayName,
                    configurationType = behaviourInfo.configurationType,
                    method = behaviourInfo.method,
                    identifier = behaviourInfo.identifier
                };
            }
            this.behaviours.Clear();
            this.behaviours.AddRange(behaviours.Values);
            this.behaviourView = new ReadOnlyDictionary<string, BehaviourEntry>(behaviours);
            EditorUtility.SetDirty(this);
        }
        public override void Compile(CompileOptions hint = CompileOptions.None, bool forceCompilation = false) {
            if (forceCompilation || (options.ShouldCompile(hint) && !upToDate)) {
                if (this.compiler.Value == null) {
                    Debug.LogError($"Compiler for Behaviour Type {this.behaviourType} is not set.");
                    return;
                }
                var compiler = Activator.CreateInstance(this.compiler.Value) as BehaviourGraphCompiler;
                if (compiler == null) {
                    Debug.LogError($"Unable to create compiler for Behaviour Type {this.behaviourType}.");
                    return;
                }
                /* var behaviourGuids = AssetDatabase.FindAssets($"t:{nameof(BehaviourGraphModel)}");
                var behaviourType = BehaviourType;
                var behaviours = new List<BehaviourGraphModel>(); */

                foreach (var modelData in Models) {
                    var model = AssetDatabase.LoadAssetAtPath<BehaviourGraphModel>(AssetDatabase.GUIDToAssetPath(modelData.asset));
                    if (model == null || model.BehaviourType.Value != behaviourType)
                        continue;
                    if (options != CompileOptions.None && (model.options | options) == 0) {
                        continue;
                    }
                    compiler.Compile(model, forceCompilation);
                }
                compiler.Dispose();
            }
        }
        private void OnValidate() {
            this.models = this.models.Where(m =>
            {
                var path = AssetDatabase.GUIDToAssetPath(m.asset);
                return string.IsNullOrEmpty(path) ? false : File.Exists(path);
            }).Distinct().ToList();
        }
        private struct AssemblyDefinitionData {
            public string name;
            public string[] references;
        }
        [Serializable]
        public struct BehaviourEntry {
            public string identifier;
            public string displayName;
            public SerializableType configurationType;
            public SerializableMethod method;
        }
        [Serializable]
        public struct ModelEntry {
            public string asset;
            public CompileOptions options;
        }
    }


}