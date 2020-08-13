using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {
    public class ObjectGraphSearchWindow : ScriptableObject, ISearchWindowProvider {
        public ObjectGraphView graphView;
        public IObjectGraphNodeProvider[] providers;

        public Func<Vector2, Vector2> screenToWorldConverter;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            if (providers == null || providers.Length == 0)
                return null;
            if (providers.Length == 1) {
                var tree = providers[0].CreateSearchTreeEntries(context, 0);
                return tree;
            }
            else {
                var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new UnityEngine.GUIContent("Effects"),0)

            };
                for (int i = 0; i < providers.Length; i++) {
                    tree.AddRange(providers[i].CreateSearchTreeEntries(context, 1));
                }
                return tree;
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is SearchTreeEntryData data && data.IsValid()) {
                var node = data.provider.Create(data.type, new Rect(graphView.contentViewContainer.WorldToLocal(graphView.panel.visualTree.ChangeCoordinatesTo(graphView.panel.visualTree, screenToWorldConverter(context.screenMousePosition))), new Vector2(100, 100)));
                graphView.AddElement(node);
            }
            return true;
        }
    }
    public struct SearchTreeEntryData {
        public IObjectGraphNodeProvider provider;
        public Type type;
        public bool IsValid() => provider != null && type != null;
    }


}