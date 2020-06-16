/*using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Reactics.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Reactics.Editor
{

    [CustomPropertyDrawer(typeof(ValueRef))]
    public class ValuePropertyDrawer : PropertyDrawer
    {
        private static List<ValueConverter> converters = new List<ValueConverter>();
        private static List<string> names = new List<string>();

        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.UpdateIfRequiredOrScript();
            GetConverters();
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var valueRect = new Rect(position.x, position.y, position.width * 0.8f, position.height);
            var unitRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);

            var valueProperty = property.FindPropertyRelative("value");
            var unitProperty = property.FindPropertyRelative("unit");
            int index = unitProperty.stringValue == String.Empty ? 0 : names.IndexOf(unitProperty.stringValue);
            
            EditorGUI.BeginDisabledGroup(unitProperty.stringValue == "Uniform" || unitProperty.stringValue == "Inherit");
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            index = EditorGUI.Popup(unitRect, index, names.ToArray());
            if (index != -1)
            {
                unitProperty.stringValue = names[index];
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
            //property.serializedObject.ApplyModifiedProperties();
            
        }

        //ALSO NOT SUPPORTED FOR SOME REASON AS OF 2014?
        /*         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {

                    var target = EditorGUILayout.BeginBuildTargetSelectionGrouping();

                    EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width), GUILayout.Height(position.height));

                    EditorGUILayout.LabelField("Value: ");
                    EditorGUILayout.FloatField(property.FindPropertyRelative("value").floatValue);
                    //EditorGUILayout.Popup(0, GetConverters().Select(x => x.Method.Name).ToArray());


                    //EditorGUI.FloatField(new Rect(position.left+20f,position.top,position.right-20f,position.bottom),property.FindPropertyRelative("value").floatValue);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndBuildTargetSelectionGrouping();


                } *//*
        private List<ValueConverter> GetConverters()
        {
            if (converters.Count <= 0)
            {
                ValueUtils.GetConverters(converters);
                names = converters.Select(x => x.Method.Name).ToList();
            }
            return converters;
        }
        //Currently not supported
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();


            //var value = new FloatField("Value");
            var value = new PropertyField(property.FindPropertyRelative("value"));
            var unit = new PopupField<ValueConverter>(ValueUtils.GetConverters(converters), ValueConverters.Pixel, (converter) => converter.Method.Name, (converter) => converter.Method.Name);
            root.Add(value);
            root.Add(unit);
            return root;
        }


    }


}*/