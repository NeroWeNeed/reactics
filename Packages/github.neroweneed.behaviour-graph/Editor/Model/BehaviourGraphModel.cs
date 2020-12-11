using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using Newtonsoft.Json;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    //TODO: Copy/Paste
    //TODO: Undo/Redo
    //TODO: Resource Pooling for references
    //TODO: Higher level asset modeling
    //
    public class BehaviourGraphModel : CompileableObject {

        public static readonly Vector2 DEFAULT_SIZE = new Vector2(100, 100);
        public static BehaviourGraphModel CreateInstance<TModel, TBehaviour>(string name) where TModel : BehaviourGraphModel {
            return CreateInstance(name, typeof(TModel), typeof(TBehaviour));
        }
        public static BehaviourGraphModel CreateInstance(string name, Type modelType, Type behaviourType) {
            var asset = CreateInstance(modelType) as BehaviourGraphModel;
            asset.BehaviourType = behaviourType;
            if (asset.Entries.Count == 0) {
                var rootEntry = asset.CreateRootEntry();
                if (rootEntry != null)
                    asset.Entries.Add(rootEntry);
            }

            var settings = BehaviourGraphGlobalSettings.GetBehaviourGraphSettings(behaviourType);
            asset.outputDirectory = settings.outputDirectory;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject) + $"/{name}.asset";
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), InitBehaviourGraphModel.Value, path, (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image, null);
            return asset;
        }
        [SerializeField]
        private SerializableType behaviourType;
        public SerializableType BehaviourType { get => behaviourType; protected set => behaviourType = value; }
        [NonSerialized]
        private BehaviourGraphSettings settings;
        public BehaviourGraphSettings Settings { get => settings ??= BehaviourGraphGlobalSettings.GetBehaviourGraphSettings(BehaviourType.Value); }
        [SerializeReference, HideInInspector]
        private List<IEntry> entries = new List<IEntry>();
        public List<IEntry> Entries { get => entries; private set => entries = value; }

        public void Clean() {
            entries = entries.Where(e => e != null).ToList();
        }
        public IEntry GetEntry(string id) {
            return entries.Find(e => e.Id == id);
        }
        public int GetEntryIndex(string id) {
            return entries.FindIndex(e => e.Id == id);
        }
        private void OnValidate() {
            if (string.IsNullOrWhiteSpace(this.outputDirectory)) {
                this.outputDirectory = Settings.outputDirectory;
            }
            if (string.IsNullOrWhiteSpace(this.outputFileName)) {
                this.outputFileName = name;
            }
            if (this.Entries.Count == 0) {
                var rootEntry = CreateRootEntry();
                if (rootEntry != null)
                    this.Entries.Add(rootEntry);
            }

        }
        public virtual IEntry CreateRootEntry() {
            return new MasterEntry();
        }
        public byte[] GetBytes(int index) {
            var entry = Entries[index];
            if (entry is BehaviourEntry e) {
                var behaviour = Settings.Behaviours[e.BehaviourIdentifier];
                e.ConfigureMemory(behaviour.configurationType.Value, e.Fields, e.Data, out byte[] result, out BehaviourEntry.FieldData[] _);
                return result;
            }
            else {
                return Array.Empty<byte>();
            }
        }


        public override void Compile(CompileOptions hint = CompileOptions.None, bool forceCompilation = false) {
            if (forceCompilation || (options.ShouldCompile(hint) && !upToDate)) {
                var settings = this.Settings;
                if (settings.compiler.Value == null) {
                    Debug.LogError($"Compiler for Behaviour Type {this.behaviourType} is not set.");
                    return;
                }
                var compiler = Activator.CreateInstance(settings.compiler.Value) as BehaviourGraphCompiler;
                if (compiler == null) {
                    Debug.LogError($"Unable to create compiler for Behaviour Type {this.behaviourType}.");
                    return;
                }
                compiler.Compile(this, forceCompilation);
                compiler.Dispose();
            }
        }

    }

    public class InitBehaviourGraphModel : EndNameEditAction {
        private static InitBehaviourGraphModel value;
        public static InitBehaviourGraphModel Value => value ?? CreateInstance<InitBehaviourGraphModel>();
        public override void Action(int instanceId, string pathName, string resourceFile) {

            var model = EditorUtility.InstanceIDToObject(instanceId) as BehaviourGraphModel;
            if (model != null) {
                model.outputFileName = GetName(pathName);


                AssetDatabase.CreateAsset(model, pathName);
                model.Settings.Models.Add(new BehaviourGraphSettings.ModelEntry
                {
                    asset = AssetDatabase.GUIDFromAssetPath(pathName).ToString(),
                    options = CompileOptions.All
                });
                EditorUtility.SetDirty(model.Settings);
                EditorUtility.SetDirty(model);
            }

        }
        private string GetName(string pathName) {
            var index1 = pathName.LastIndexOf('/');
            var index2 = pathName.LastIndexOf('.');
            return pathName.Substring(index1 + 1, index2 - (index1 + 1));
        }
    }
}