using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public static class ObjectGraphAssetHandler {
        [InitializeOnLoadMethod]
        public static void GenerateOnReload() {
            Debug.Log("Generated!");
            AssemblyReloadEvents.afterAssemblyReload += () => GenerateAssets(true, false);
        }
        [MenuItem("Reactics/Object Graph/Generate Assets")]
        private static void GenerateAssets() => GenerateAssets(false, true);
        private static void GenerateAssets(bool generateOnlyOnReload, bool forceGenerate) {
            var assetGuids = AssetDatabase.FindAssets("t:ObjectGraphAsset");
            var shouldSave = false;
            foreach (var assetGuid in assetGuids) {
                var asset = AssetDatabase.LoadAssetAtPath<ObjectGraphAsset>(AssetDatabase.GUIDToAssetPath(assetGuid));
                if (!generateOnlyOnReload || asset?.generateOnReload == true) {
                    GenerateAsset(forceGenerate, asset, false);
                    shouldSave = true;
                }
            }
            if (shouldSave)
                AssetDatabase.SaveAssets();
        }
        public static void GenerateAsset(bool forceGenerate, ObjectGraphAsset objectGraphAsset, bool forceSave = true) {
            var location = GetLocation(objectGraphAsset);
            var outputAssetType = objectGraphAsset.GetOutputAssetType();
            if (outputAssetType == null)
                return;
            Debug.Log(outputAssetType);
            var asset = AssetDatabase.LoadAssetAtPath(location, outputAssetType);
            if (asset == null) {
                asset = ScriptableObject.CreateInstance(outputAssetType);
                Debug.Log(asset);
                AssetDatabase.CreateAsset(asset, location);
                forceGenerate = true;
            }
            var version = objectGraphAsset.Version;
            var hash = GetHash(location, version);
            forceGenerate |= hash != objectGraphAsset.Hash;
            if (forceGenerate) {
                var serializedAsset = new SerializedObject(asset);
                objectGraphAsset?.UpdateAsset(serializedAsset);
                var labels = AssetDatabase.GetLabels(asset);

                EditorUtility.SetDirty(asset);
                serializedAsset.ApplyModifiedPropertiesWithoutUndo();
                objectGraphAsset.Hash = hash;
                EditorUtility.SetDirty(objectGraphAsset);
                if (forceSave)
                    AssetDatabase.SaveAssets();
            }
        }

        private static int GetHash(string path, int version) {
            int hashCode = 1196998432;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(path);
            hashCode = hashCode * -1521134295 + version.GetHashCode();
            return hashCode;
        }
        public static string GetLocation(SerializedObject serializedObject) {
            var location = new StringBuilder();
            location.Append(serializedObject.FindProperty("outputPath").stringValue);
            if (location[location.Length - 1] != '/')
                location.Append('/');
            location.Append(serializedObject.FindProperty("outputName").stringValue);
            location.Append(".asset");
            return location.ToString();
        }
        public static string GetLocation(ObjectGraphAsset asset) {
            var location = new StringBuilder();
            location.Append(asset.outputPath);
            if (location[location.Length - 1] != '/')
                location.Append('/');
            location.Append(asset.outputName);
            location.Append(".asset");
            return location.ToString();
        }
    }
    public abstract class ObjectGraphAssetEditor<TEditorWindow, TAsset> : UnityEditor.Editor where TEditorWindow : ObjectGraphEditor<TAsset> where TAsset : ScriptableObject {

        public const string UXML_PATH = "Assets/EditorResources/UIElements/ObjectGraphAssetEditor.uxml";
        public abstract string GetEditorTitle(TAsset asset);
        public VisualElement rootVisualElement = null;
        private void OnEnable() {
            if (rootVisualElement == null)
                rootVisualElement = new VisualElement();
            else
                rootVisualElement.Clear();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH).CloneTree(rootVisualElement);
        }
        public override VisualElement CreateInspectorGUI() {
            rootVisualElement.Q<Button>("edit").clicked += OpenGraphEditor;
            rootVisualElement.Q<Button>("generate").clicked += GenerateAsset;
            return rootVisualElement;
        }

        protected virtual void OpenGraphEditor() {
            ObjectGraphEditor<TAsset>.OnOpen<TEditorWindow>(this.serializedObject.targetObject.GetInstanceID(), 0, GetEditorTitle);
        }
        protected void GenerateAsset() => GenerateAsset(true);
        protected virtual void GenerateAsset(bool force) {
            ObjectGraphAssetHandler.GenerateAsset(force, serializedObject.targetObject as ObjectGraphAsset);

        }


        protected override bool ShouldHideOpenButton() {
            return true;
        }
    }
}