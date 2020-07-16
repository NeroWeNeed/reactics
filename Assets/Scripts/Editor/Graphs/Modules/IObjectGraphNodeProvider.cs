using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public interface IObjectGraphNodeProvider : IObjectGraphModule {
        ObjectGraphSerializer<SerializedObject> Serializer { get; }
        ObjectGraphNode Create(string id, Type type, Rect layout);
        ObjectGraphNode Create(ObjectGraphNodeJsonSet.Entry entry);
        List<SearchTreeEntry> CreateSearchTreeEntries(SearchWindowContext context, int depth);
        ObjectGraphNode[] CollectNodes(ObjectGraphView graphView);
        bool GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter, Port targetPort);
    }
    public interface IObjectGraphNodeProvider<TNode> : IObjectGraphNodeProvider where TNode : ObjectGraphNode, new() {

    }
    public static class IObjectGraphNodeProviderExtensions {
        public static TNode Create<TNode>(this IObjectGraphNodeProvider<TNode> _, string id, Type type, Rect layout) where TNode : ObjectGraphNode, new() {
            var node = new TNode
            {
                Id = id
            };
            node.SetPosition(layout);
            node.TargetType = type;
            return node;
        }
        public static TNode Create<TNode>(this IObjectGraphNodeProvider<TNode> provider, Type type, Rect layout) where TNode : ObjectGraphNode, new() => Create(provider, Guid.NewGuid().ToString(), type, layout);

        public static TNode Create<TNode>(this IObjectGraphNodeProvider<TNode> provider, string id, ObjectGraphModel.Entry entry, Rect layout) where TNode : ObjectGraphNode, new() {
            var node = Create(provider, id, entry.type, layout);
            node.Entry = entry;
            return node;
        }
        public static TNode Create<TNode>(this IObjectGraphNodeProvider<TNode> provider, ObjectGraphModel.Entry entry, Rect layout) where TNode : ObjectGraphNode, new() => provider.Create(Guid.NewGuid().ToString(), entry, layout);

        public static TNode Create<TNode>(this IObjectGraphNodeProvider<TNode> provider, ObjectGraphNodeJsonSet.Entry jsonEntry) where TNode : ObjectGraphNode, new() {
            if (jsonEntry.nodeType == typeof(TNode)) {
                return provider.Create(jsonEntry.id, jsonEntry.entry, jsonEntry.layout);
            }
            else {
                return null;
            }
        }

        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, string id, Type type, Rect layout) {
            var node = provider.Create(id, type, layout);
            node.SetPosition(layout);
            node.TargetType = type;
            return node;
        }
        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, Type type, Rect layout) => Create(provider, Guid.NewGuid().ToString(), type, layout);

        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, string id, ObjectGraphModel.Entry entry, Rect layout) {
            var node = Create(provider, id, entry.type, layout);
            node.Entry = entry;
            return node;
        }
        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, ObjectGraphModel.Entry entry, Rect layout) => provider.Create(Guid.NewGuid().ToString(), entry, layout);

        public static ObjectGraphNode[] CollectRoots(this IObjectGraphNodeProvider provider, ObjectGraphView graphView) => provider.CollectNodes(graphView).Where((n) => n.IsRoot).ToArray();

        public static TNode[] CollectRoots<TNode>(this IObjectGraphNodeProvider<TNode> provider, ObjectGraphView graphView) where TNode : ObjectGraphNode, new() => provider.CollectNodes(graphView).OfType<TNode>().Where((n) => n.IsRoot).ToArray();
        public static bool Deserialize(this IObjectGraphNodeProvider provider, SerializedObject target, ObjectGraphView graphView) {
            return provider.Serializer.Deserialize(target, provider, graphView, out SerializedObject _);
        }
        public static bool Serialize(this IObjectGraphNodeProvider provider, SerializedObject target, ObjectGraphView graphView) {
            return provider.Serializer.Serialize(target, provider, graphView, out SerializedObject _);
        }
    }
}