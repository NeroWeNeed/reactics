using System.Drawing;
using System.Reflection.Emit;
using System;
using Reactics.Commons;
using UnityEditor;
using UnityEngine;
using Reactics.Battle.Map;

namespace Reactics.Editor
{

    public abstract class EnumDictionaryPropertyDrawer<IEnum, IValue> : PropertyDrawer where IEnum : Enum
    {
        private bool showingContent = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginProperty(position, label, property);

            showingContent = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), showingContent, label.text);
            if (showingContent)
            {
                EditorGUI.indentLevel++;
                var enums = Enum.GetValues(typeof(IEnum));
                Array.Sort(enums);
                var entriesProp = property.FindPropertyRelative("entries");

                if (entriesProp.arraySize > enums.Length)
                {
                    for (int i = entriesProp.arraySize; i > enums.Length; i--)
                    {
                        entriesProp.DeleteArrayElementAtIndex(i);
                    }
                }
                else if (entriesProp.arraySize < enums.Length)
                {
                    for (int i = entriesProp.arraySize; i < enums.Length; i++)
                    {
                        entriesProp.InsertArrayElementAtIndex(i);
                        entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("key").intValue = Convert.ToInt32(enums.GetValue(i));
                    }
                }

                for (int i = 0; i < enums.Length; i++)
                {
                    Rect enumPosition = new Rect(position.x + EditorGUI.indentLevel, position.y + (i + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width - EditorGUI.indentLevel, EditorGUIUtility.singleLineHeight);

                    enumPosition = EditorGUI.PrefixLabel(enumPosition, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(enums.GetValue(i).ToString()));

                    GuessField(enumPosition, entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("value"));
                    
                }


            }

            EditorGUI.EndProperty();
            //property.serializedObject.ApplyModifiedProperties();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (showingContent)
                return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (Enum.GetValues(typeof(IEnum)).Length + 1);
            else
                return EditorGUIUtility.singleLineHeight;
        }
        protected void GuessField(Rect position, SerializedProperty property)
        {
            var type = typeof(IValue);
            if (type == typeof(Color))
            {

                var oldColor = property.colorValue;
                var newColor = EditorGUI.ColorField(position, oldColor);
                if (oldColor != newColor)
                {
                    property.colorValue = newColor;
                    
                }


            }
        }
        protected abstract IValue GetValue(SerializedProperty property);

    }
    [CustomPropertyDrawer(typeof(EnumDictionary<MapLayer, Color>))]
    public class MapLayerColorPropertyDrawer : EnumDictionaryPropertyDrawer<MapLayer, Color>
    {
        protected override Color GetValue(SerializedProperty property)
        {
            var result = property.colorValue;
            if (result == null)
                return Color.black;
            else
                return result;
        }
    }
}