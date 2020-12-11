using System;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {

    [CustomEditor(typeof(BehaviourGraphModel))]
    public class BehaviourGraphModelEditor : UnityEditor.Editor {
        public const string UXML = "Packages/github.neroweneed.behaviour-graph/Editor/Resources/BehaviourGraphModel.uxml";
        private VisualElement rootVisualElement = null;
        public override VisualElement CreateInspectorGUI() {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);

            if (rootVisualElement == null) {
                rootVisualElement = new VisualElement();
            }
            else {
                rootVisualElement.Clear();
            }
            uxml.CloneTree(rootVisualElement);
            var behaviourTypeField = rootVisualElement.Q<PropertyField>("behaviourType");
            behaviourTypeField.SetEnabled(false);
            BehaviourGraphModel model = serializedObject?.targetObject as BehaviourGraphModel;

            if (model != null) {
                rootVisualElement.Q<Button>("editButton").clicked += () => BehaviourGraphEditor.OnOpen(serializedObject.targetObject.GetInstanceID());
                rootVisualElement.Q<Button>("compileButton").clicked += CompileModel;
                rootVisualElement.Q<EnumField>("compileOptions").RegisterValueChangedCallback(e => UpdateSettings((CompileOptions)e.newValue));
            }
            return rootVisualElement;
        }
        private void CompileModel() {
            BehaviourGraphModel model = serializedObject?.targetObject as BehaviourGraphModel;
            if (model != null) {
                try {
                    (serializedObject.targetObject as BehaviourGraphModel)?.Compile(forceCompilation: true);
                    Debug.Log($"Successfully Compiled {model.name} to location '{model.outputDirectory}' as '{model.outputFileName}'");
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }



        }
        private void UpdateSettings(CompileOptions value) {

            if (serializedObject.targetObject is BehaviourGraphModel model) {
                var guid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(model)).ToString();
                var index = model.Settings.Models.FindIndex(m => m.asset == guid);
                var e = new BehaviourGraphSettings.ModelEntry
                {
                    asset = guid,
                    options = value
                };
                if (index < 0) {
                    model.Settings.Models.Add(e);
                }
                else {
                    model.Settings.Models[index] = e;
                }
            }
        }

    }


}