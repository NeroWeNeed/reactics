using System.Collections.Generic;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public class BehaviourGraphInspector : GraphElement, ISelection {
        public const string UXML = "Packages/github.neroweneed.behaviour-graph/Editor/Resources/BehaviourGraphInspector.uxml";
        public const string USS = "Packages/github.neroweneed.behaviour-graph/Editor/Resources/BehaviourGraphInspector.uss";
        public const string CLASS = nameof(BehaviourGraphInspector);
        private BehaviourGraphModel Model { get; set; }
        private BehaviourGraphView GraphView { get; set; }
        public VisualElement MainContainer { get; private set; }
        public VisualElement VariableContainer { get; private set; }

        public List<ISelectable> selection => GraphView?.selection;

        public BehaviourGraphInspector(BehaviourGraphView graphView) {
            this.GraphView = graphView;
            this.SetPosition(new Rect(0, 0, 360, 640));
            this.ResetLayer();
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);
            //var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS);
            MainContainer = uxml.Instantiate();
            VariableContainer = MainContainer.Q<VisualElement>("variableContainer");
            //this.styleSheets.Add(uss);
            base.hierarchy.Add(MainContainer);
            this.AddToClassList(CLASS);

            this.capabilities = Capabilities.Collapsible | Capabilities.Movable | Capabilities.Resizable;
            this.AddManipulator(new Dragger
            {
                clampToParentEdges = true
            });
            var resizer = new Resizer();
            resizer.style.opacity = 0;
            base.style.overflow = Overflow.Hidden;
            base.hierarchy.Add(resizer);
        }

        public void AddToSelection(ISelectable selectable) {
            GraphView?.AddToSelection(selectable);
        }

        public void RemoveFromSelection(ISelectable selectable) {
            GraphView?.RemoveFromSelection(selectable);
        }

        public void ClearSelection() {
            GraphView?.ClearSelection();
        }
    }
}