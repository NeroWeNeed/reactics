using System;
using System.Collections.Generic;
using Reactics.Battle;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor
{
    public class EffectGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public EffectGraph effectGraph;

        public GraphView graphView;

        public Func<Vector2, Vector2> screenToWorldConverter;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new UnityEngine.GUIContent("Effects"),0)

            };
            var typeGroups = new Dictionary<Type, List<Type>>();
            foreach (var type in EffectGraphController.SupportedPortTypes)
            {
                typeGroups[typeof(IEffect<>).MakeGenericType(type)] = new List<Type>();
            }
            foreach (var type in effectGraph.EffectTypes)
            {
                foreach (var key in typeGroups.Keys)
                {
                    if (key.IsAssignableFrom(type))
                    {
                        typeGroups[key].Add(type);
                        break;
                    }
                }
            }
            foreach (var kv in typeGroups)
            {
                tree.Add(new SearchTreeGroupEntry(new UnityEngine.GUIContent($"{kv.Key.GenericTypeArguments[0].Name} Effects"), 1));
                foreach (var type in kv.Value)
                {
                    var content = new UnityEngine.GUIContent(type.Name);
                    tree.Add(new SearchTreeEntry(content)
                    {
                        level = 2,
                        userData = type
                    });
                }
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {

            EffectGraphController.CreateNode(graphView, effectGraph, (Type)searchTreeEntry.userData, new Rect(graphView.contentViewContainer.WorldToLocal(graphView.panel.visualTree.ChangeCoordinatesTo(graphView.panel.visualTree, screenToWorldConverter(context.screenMousePosition))), new Vector2(100, 100)));
            return true;
        }
    }
}