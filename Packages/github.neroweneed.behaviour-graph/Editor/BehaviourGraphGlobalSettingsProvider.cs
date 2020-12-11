
using System;
using System.Linq;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {

    public class BehaviourGraphGlobalSettingsProvider : SettingsProvider {

        private ReorderableList behaviours;
        private SerializedObject serializedObject;
        private bool showBehaviourTypes = true;
        public BehaviourGraphGlobalSettingsProvider(string path, SettingsScope scopes = SettingsScope.User) : base(path, scopes) {
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() {
            return new BehaviourGraphGlobalSettingsProvider("Project/BehaviourGraphSettings", SettingsScope.Project)
            {
                label = "Behaviour Graph Settings"
            };
        }
        public override void OnActivate(string searchContext, VisualElement rootElement) {

            this.serializedObject = BehaviourGraphGlobalSettings.SerializedSettings;

            behaviours = new ReorderableList(serializedObject, serializedObject.FindProperty("behaviours"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => GUI.Label(rect, "Behaviour Types"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var prop = serializedObject.FindProperty("behaviours").GetArrayElementAtIndex(index);
                    var propText = prop.FindPropertyRelative("assemblyQualifiedName");
                    var label = propText.stringValue != null ? new GUIContent(propText.stringValue) : new GUIContent("(Missing Type)");

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), prop, label);
                },
                onAddCallback = (ReorderableList _) =>
                {
                    var prop = serializedObject.FindProperty("behaviours");
                    var index = prop.arraySize;
                    prop.InsertArrayElementAtIndex(index);
                    prop.GetArrayElementAtIndex(index).FindPropertyRelative("assemblyQualifiedName").stringValue = null;
                    serializedObject.ApplyModifiedProperties();
                }

            };
        }
        public override void OnDeactivate() {
            if (serializedObject == null)
                return;
            var cleanedUpBehaviours = serializedObject.FindProperty("behaviours").ToArray(prop => prop.FindPropertyRelative("assemblyQualifiedName").stringValue).Distinct().Where(obj => obj != null).ToArray();
            serializedObject.FindProperty("behaviours").WriteArray(cleanedUpBehaviours, (index, element, property) => property.FindPropertyRelative("assemblyQualifiedName").stringValue = element);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        public override void OnGUI(string searchContext) {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseOutputDirectory"));
            behaviours.DoLayoutList();
            if (GUILayout.Button("Regenerate Behaviour Data")) {
                RegenerateBehaviourData();
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void RegenerateBehaviourData() {
            if (serializedObject == null)
                return;
            if (!AssetDatabase.IsValidFolder($"Assets/{BehaviourGraphGlobalSettings.SETTINGS_FOLDER}/{BehaviourGraphGlobalSettings.DATA_FOLDER}"))
                AssetDatabase.CreateFolder($"Assets/{BehaviourGraphGlobalSettings.SETTINGS_FOLDER}", BehaviourGraphGlobalSettings.DATA_FOLDER);
            var types = serializedObject.FindProperty("behaviours").ToArray(b => Type.GetType(b.FindPropertyRelative("assemblyQualifiedName").stringValue));
            var data = serializedObject.FindProperty("entries");
            var baseOutputDirectory = serializedObject.FindProperty("baseOutputDirectory").stringValue;
            data.ClearArray();
            var index = 0;
            foreach (var type in types) {
                if (type == null) {
                    continue;
                }
                var assetPath = $"Assets/{BehaviourGraphGlobalSettings.SETTINGS_FOLDER}/{BehaviourGraphGlobalSettings.DATA_FOLDER}/{type.FullName}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<BehaviourGraphSettings>(assetPath);
                if (asset == null) {
                    asset = ScriptableObject.CreateInstance<BehaviourGraphSettings>();
                    asset.outputDirectory = $"{ baseOutputDirectory}/{type.FullName}";
                    AssetDatabase.CreateAsset(asset, assetPath);
                    AssetDatabase.SaveAssets();
                }
                EditorUtility.SetDirty(asset);
                data.InsertArrayElementAtIndex(index);
                var prop = data.GetArrayElementAtIndex(index);
                prop.FindPropertyRelative("type.assemblyQualifiedName").stringValue = type.AssemblyQualifiedName;
                prop.FindPropertyRelative("asset").objectReferenceValue = asset;
                index++;
            }
            (serializedObject.targetObject as BehaviourGraphGlobalSettings)?.RefreshEntryView();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();


        }


    }
}