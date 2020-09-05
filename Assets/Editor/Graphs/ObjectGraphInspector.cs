using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphInspector : Blackboard {
        public const string USS_PATH = "Assets\\EditorResources\\UIElements\\ObjectGraphInspector.uss";
        public const string CLASS_NAME = "inspector";
        private TabView tabView;
        public BindableElement settingsView { get; private set; }

        public VisualElement variableView { get; private set; }



        public ObjectGraphInspector(GraphView associatedGraphView = null) : base(associatedGraphView) {
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH));
            AddToClassList(CLASS_NAME);
            title = "Inspector";
            this.Q<Button>("addButton").RemoveFromHierarchy();
            this.Q<Label>("subTitleLabel").RemoveFromHierarchy();
            tabView = new TabView();
            settingsView = new BindableElement
            {
                name = "settings"
            };
            variableView = new VisualElement
            {
                name = "variables"
            };
            tabView.AddTab(0, "Settings", settingsView);
            tabView.AddTab(1, "Variables", variableView);

            this.Add(tabView);
            this.scrollable = true;

        }
        public void AddInspector(VisualElement element) {
            settingsView.Add(element);
        }
        public void ClearContents() {
            settingsView.Clear();
            variableView.Clear();
        }
        public void AddVariables(IObjectGraphVariableProvider[] types, ObjectGraphModel model) {

            foreach (var type in types) {
                foreach (var variable in type.BuildVariables()) {
                    if (model.AddVariable(variable)) {
                        this.variableView.Add(variable.provider.BuildField(variable));
                    }
                }
            }


        }



    }
}