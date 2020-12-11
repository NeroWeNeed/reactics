using System;
using System.Collections.Generic;
using System.Linq;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public class BehaviourGraphSearchWindow : ScriptableObject, ISearchWindowProvider {

        public List<BehaviourGraphModel> models = new List<BehaviourGraphModel>();
        public BehaviourGraphView graphView;
        public BaseBehaviourGraphEditor editor;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var result = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Behaviours"), 0)
            };
            foreach (var model in models) {
                foreach (var entry in model.Settings.Behaviours.Values) {
                    result.Add(new SearchTreeEntry(new GUIContent(entry.displayName))
                    {
                        level = 1,
                        userData = new SearchWindowData { identifier = entry.identifier, model = model }
                    });
                }
            }

            return result;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
            if (SearchTreeEntry.userData is SearchWindowData data) {


                var entry = graphView.CreateBehaviour(data.identifier, data.model.Settings, graphView.contentViewContainer.WorldToLocal(graphView.panel.visualTree.ChangeCoordinatesTo(graphView.panel.visualTree, context.screenMousePosition - editor.position.position)));
                data.model.Entries.Add(entry);
                var node = entry.CreateNode(graphView, data.model.Settings);
                if (node is IBehaviourGraphNode behaviourGraphNode) {
                    behaviourGraphNode.Model = data.model;
                }
                node.RefreshExpandedState();
                graphView.AddElement(node);

            }
            return true;
        }
        public struct SearchWindowData {
            public string identifier;
            public BehaviourGraphModel model;
        }
    }
}