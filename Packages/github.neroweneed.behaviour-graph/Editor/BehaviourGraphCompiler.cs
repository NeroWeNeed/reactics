using System;
using System.Collections.Generic;
using System.IO;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public abstract class BehaviourGraphCompiler : IDisposable {
        public virtual string Extension { get; } = "bytes";
        public virtual void Compile(BehaviourGraphModel model, bool forceCompilation = false) {
            if (forceCompilation || !model.upToDate) {
                var outputDirectory = string.IsNullOrWhiteSpace(model.outputDirectory) ? model.Settings.outputDirectory : model.outputDirectory;
                if (string.IsNullOrWhiteSpace(outputDirectory)) {
                    Debug.LogError($"Output Directory for Behaviour Graph {model.name} is null. Skipping.");
                    return;
                }
                var outputFileName = model.outputFileName;
                if (string.IsNullOrWhiteSpace(outputFileName)) {
                    Debug.LogError($"Output File Name for Behaviour Graph {model.name} is null. Skipping.");
                    return;
                }

                Directory.CreateDirectory(outputDirectory);
                var outputFile = $"{outputDirectory}/{outputFileName}{(string.IsNullOrWhiteSpace(Extension) ? "" : $".{Extension}")}";
                var result = Compile(model, outputFile, out string error);
                if (!string.IsNullOrWhiteSpace(error)) {
                    Debug.LogError($"Error Compiling Behaviour Graph {model.name}: {error}");
                }
                if (!result) {
                    return;
                }
                var behaviourSettings = model.Settings;
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                settings.AddLabel(behaviourSettings.behaviourName);
                AddressableAssetGroup assetGroup = GetGroup();
                var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(outputFile), assetGroup);
                entry.address = $"Behaviour.{behaviourSettings.behaviourName}.{outputFileName}";
                entry.labels.Add(behaviourSettings.behaviourName);
                AssetDatabase.Refresh();
                model.upToDate = true;
            }
        }
        protected abstract bool Compile(BehaviourGraphModel model, string outputFile, out string error);

        public AddressableAssetGroup GetGroup() => GetGroup(AddressableAssetSettingsDefaultObject.Settings);
        private AddressableAssetGroup GetGroup(AddressableAssetSettings settings) {
            return settings.FindGroup("Behaviours") ?? settings.CreateGroup("Behaviours", false, false, true, new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
        }

        public virtual void Dispose() { }
    }
}