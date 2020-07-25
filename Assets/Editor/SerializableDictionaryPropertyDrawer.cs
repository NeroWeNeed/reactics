using UnityEngine;
using UnityEditor;
using Reactics.Commons;
namespace Reactics.Editor
{
/* 
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private bool showingContent = true;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            showingContent = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), showingContent, label.text);
            if (showingContent)
            {
                EditorGUI.indentLevel++;
                int i = 0;
                if (property.isArray)
                    for (i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        Rect keyPosition = new Rect(position.x + EditorGUI.indentLevel, position.y + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), (position.width - EditorGUI.indentLevel) * 0.45f, EditorGUIUtility.singleLineHeight);
                        Rect valuePosition = new Rect(position.x + EditorGUI.indentLevel + ((position.width - EditorGUI.indentLevel) * 0.45f), position.y + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), (position.width - EditorGUI.indentLevel) * 0.45f, EditorGUIUtility.singleLineHeight);
                        Rect removePosition = new Rect(position.x + EditorGUI.indentLevel + ((position.width - EditorGUI.indentLevel) * 0.9f), position.y + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), (position.width - EditorGUI.indentLevel) * 0.1f, EditorGUIUtility.singleLineHeight);

                        EditorGUI.PropertyField(keyPosition, element.FindPropertyRelative("key"));
                        EditorGUI.PropertyField(valuePosition, element.FindPropertyRelative("value"));
                        GUI.Button(removePosition, "-");
                    }
                if (GUI.Button(new Rect(position.x + EditorGUI.indentLevel, position.y + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), (position.width - EditorGUI.indentLevel), EditorGUIUtility.singleLineHeight), "+"))
                {
                    property.InsertArrayElementAtIndex(0);
                    property.
                }

            }

            EditorGUI.EndProperty();
        }
    } */
}