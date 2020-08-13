using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {
    public class ObjectGraphInspector : Blackboard {
        public const string USS_PATH = "Assets\\EditorResources\\UIElements\\ObjectGraphInspector.uss";
        public const string CLASS_NAME = "inspector";
        private TabView tabView;
        private VisualElement settingsView;

        private VisualElement variableView;
        public ObjectGraphInspector(GraphView associatedGraphView = null) : base(associatedGraphView) {
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH));
            AddToClassList(CLASS_NAME);
            title = "Inspector";
            Debug.Log("Creating...");
            this.Q<Button>("addButton").RemoveFromHierarchy();
            this.Q<Label>("subTitleLabel").RemoveFromHierarchy();
            tabView = new TabView();
            settingsView = new VisualElement
            {
                name = "settings"
            };
            variableView = new VisualElement
            {
                name = "variables"
            };
            tabView.AddTab(0, "Settings", settingsView);
            tabView.AddTab(1, "Variables", variableView);
            //variableView.StretchToParentWidth();
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
        public void AddVariables(Type container) {

            if (container.StructLayoutAttribute.Value == LayoutKind.Sequential || container.StructLayoutAttribute.Value == LayoutKind.Explicit) {
                foreach (var variable in container.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    variableView.Add(new ObjectGraphVariable(container, variable));
                }
            }
        }


    }
}