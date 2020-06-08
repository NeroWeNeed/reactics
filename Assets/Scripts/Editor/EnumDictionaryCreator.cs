using System.Drawing;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using static Reactics.Editor.FileSelector;
using Reactics.Commons;

namespace Reactics.Editor
{
    public class EnumDictionaryCreator : EditorWindow
    {

        [MenuItem("Window/UIElements/EnumDictionaryCreator")]
        public static void ShowExample()
        {
            EnumDictionaryCreator wnd = GetWindow<EnumDictionaryCreator>();
            wnd.titleContent = new GUIContent("EnumDictionaryCreator");
            wnd.ShowPopup();
        }


        public void OnEnable()
        {

            VisualElement root = rootVisualElement;


            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Resources/Editor/EnumDictionaryCreator.uxml");
            TypeSelector enumField = new TypeSelector("Enum Field", type => type.IsEnum);
            TypeSelector valueField = new TypeSelector("Value Field");
            FileSelector fileSelectorField = new FileSelector(FileSelectorMode.SAVE_FILE, "asset", "Assets/Resources/EnumMappings");
            Button createButton = new Button { text = "Create" };
            root.Add(enumField);
            root.Add(valueField);
            root.Add(fileSelectorField);
            root.Add(createButton);

            enumField.RegisterCallback<ChangeEvent<Type>>(evt =>
            {
                Debug.Log("ENUM TYPE IS " + evt.newValue);
                UpdateCreateButtonAvailability(createButton, enumField, valueField, fileSelectorField);
            });
            valueField.RegisterCallback<ChangeEvent<Type>>(evt =>
            {
                Debug.Log("VALUE TYPE IS " + evt.newValue);
                UpdateCreateButtonAvailability(createButton, enumField, valueField, fileSelectorField);
            });
            fileSelectorField.RegisterValueChangedCallback(evt =>
            {
                Debug.Log("FILE IS " + evt.newValue);
                UpdateCreateButtonAvailability(createButton, enumField, valueField, fileSelectorField);
            });
            UpdateCreateButtonAvailability(createButton, enumField, valueField, fileSelectorField);
            createButton.clicked += () =>
            {
                var type = typeof(EnumDictionary<,>);
                type = type.MakeGenericType(enumField.value, valueField.value);
                Debug.Log(type);
                var result = ScriptableObject.CreateInstance(type);
                Debug.Log(result);
                Debug.Log(fileSelectorField.value);
                AssetDatabase.CreateAsset(result, fileSelectorField.value);
                this.Close();
            };


            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Editor/EnumDictionaryCreator.uss");



        }
        private void UpdateCreateButtonAvailability(Button button, TypeSelector enumField, TypeSelector valueField, FileSelector fileSelectorField)
        {
            Debug.Log(enumField.IsValid());
            Debug.Log(valueField.IsValid());
            Debug.Log(fileSelectorField.value);
            button.SetEnabled(enumField.IsValid() && fileSelectorField.value != null && fileSelectorField.value != string.Empty && valueField.IsValid());
        }
    }
}
