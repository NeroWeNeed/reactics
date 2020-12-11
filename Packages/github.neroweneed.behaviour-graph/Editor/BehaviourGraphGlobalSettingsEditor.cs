using System.Linq;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;


namespace NeroWeNeed.BehaviourGraph.Editor {

    //[CustomEditor(typeof(BehaviourGraphSettings))]
    public class BehaviourGraphGlobalSettingsEditor : UnityEditor.Editor {
        private ReorderableList assemblies;
        public override void OnInspectorGUI() {
            serializedObject.UpdateIfRequiredOrScript();
            assemblies.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        private void OnDisable() {
            var cleanedUpAssemblies = serializedObject.FindProperty("assemblies").ToArray(prop => prop.objectReferenceValue).Distinct().Where(obj => obj != null).ToArray();
            serializedObject.FindProperty("assemblies").WriteArray(cleanedUpAssemblies, (index, element, property) => property.objectReferenceValue = element);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        private void OnEnable() {
            assemblies = new ReorderableList(serializedObject, serializedObject.FindProperty("assemblies"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => GUI.Label(rect, "Assemblies"),
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
    }
}