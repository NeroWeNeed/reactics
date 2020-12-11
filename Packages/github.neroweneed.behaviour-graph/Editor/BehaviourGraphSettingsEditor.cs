using System.Linq;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NeroWeNeed.BehaviourGraph.Editor {
    [CustomEditor(typeof(BehaviourGraphSettings))]
    public class BehaviourGraphSettingsEditor : UnityEditor.Editor {
        private ReorderableList assemblies;
        private void OnEnable() {
            assemblies = new ReorderableList(serializedObject, serializedObject.FindProperty("assemblies"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => GUI.Label(rect, "Search Assemblies"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var prop = serializedObject.FindProperty("assemblies").GetArrayElementAtIndex(index);

                    var label = prop.objectReferenceValue != null ? new GUIContent(prop.objectReferenceValue.name) : new GUIContent("(Missing Reference)");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), prop, label);
                },
                onAddCallback = (ReorderableList _) =>
                {
                    var prop = serializedObject.FindProperty("assemblies");
                    var index = prop.arraySize;
                    prop.InsertArrayElementAtIndex(index);
                    prop.GetArrayElementAtIndex(index).objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }

            };
        }
        private void OnDisable() {
            var cleanedUpAssemblies = serializedObject.FindProperty("assemblies").ToArray(prop => prop.objectReferenceValue).Distinct().Where(obj => obj != null).ToArray();
            serializedObject.FindProperty("assemblies").WriteArray(cleanedUpAssemblies, (index, element, property) => property.objectReferenceValue = element);
        }
        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviourName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("compiler"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("provider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variableDefinition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("outputDirectory"));
            assemblies.DoLayoutList();
            if (GUILayout.Button("Refresh Data")) {
                (serializedObject.targetObject as BehaviourGraphSettings)?.RefreshData();
            }
            if (GUILayout.Button("Compile All")) {
                (serializedObject.targetObject as BehaviourGraphSettings)?.Compile(forceCompilation: true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}