using System.Drawing;
using System.Reflection.Emit;
using System;
using Reactics.Commons;
using UnityEditor;
using UnityEngine;
using Reactics.Battle.Map;

namespace Reactics.Editor
{
    [CustomPropertyDrawer(typeof(EnumDictionary<,>))]
    public class EnumDictionaryPropertyDrawer : PropertyDrawer
    {
        private bool showingContent = true;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            showingContent = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), showingContent, label.text);
            if (showingContent)
            {
                EditorGUI.indentLevel++;
                var entriesProp = property.FindPropertyRelative("values");
                for (int i = 0; i < entriesProp.arraySize; i++)
                {
                    var prop = entriesProp.GetArrayElementAtIndex(i);
                    var key = prop.FindPropertyRelative("key");
                    Rect entryPosition = new Rect(position.x + EditorGUI.indentLevel, position.y + (key.enumValueIndex + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - EditorGUI.indentLevel, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(entryPosition, prop.FindPropertyRelative("value"), new GUIContent(key.enumDisplayNames[key.enumValueIndex]), true);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (showingContent)
                return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (property.FindPropertyRelative("values").arraySize + 1);
            else
                return EditorGUIUtility.singleLineHeight;
        }

    }

}