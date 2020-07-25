using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphVariableBlackboard : Blackboard {

        public const string USS_GUID = "256dcec08179d5a41bbf70ec00648654";
        public const string CLASS_NAME = "inspector";
        public ObjectGraphVariableBlackboard(GraphView associatedGraphView = null) : base(associatedGraphView) {
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
            AddToClassList(CLASS_NAME);
            title = "Inspector";
            this.Q<Button>("addButton").RemoveFromHierarchy();
            this.Q<Label>("subTitleLabel").RemoveFromHierarchy();
            this.scrollable = true;
        }

    }

    /*     public class ObjectGraphInspector : GraphElement, ISelection {

            private VisualElement mainContainerElement;
            private VisualElement rootElement;
            private Label titleElement;
            private ScrollView scrollViewElement;
            private VisualElement contentContainerElement;
            private VisualElement headerElement;
            private Dragger dragger;
            private GraphView graphView;
            private bool windowed;



            public GraphView GraphView
            {
                get
                {
                    if (!windowed && graphView == null) {
                        graphView = GetFirstAncestorOfType<GraphView>();
                    }
                    return graphView;
                }
                set
                {
                    if (windowed)
                        graphView = value;
                }
            }
            public List<ISelectable> selection => graphView?.selection;
            public ObjectGraphInspector(GraphView associatedGraphView = null) {

            }

            public void AddToSelection(ISelectable selectable) {
                throw new System.NotImplementedException();
            }

            public void ClearSelection() {
                throw new System.NotImplementedException();
            }

            public void RemoveFromSelection(ISelectable selectable) {
                throw new System.NotImplementedException();
            }
        }
     */
}