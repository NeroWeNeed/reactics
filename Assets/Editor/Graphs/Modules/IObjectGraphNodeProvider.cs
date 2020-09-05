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
        List<SearchTreeEntry> CreateSearchTreeEntries(SearchWindowContext context, int depth);
        ObjectGraphNode[] CollectNodes(ObjectGraphView graphView);
        bool GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter, Port targetPort);
    }

    public static class IObjectGraphNodeProviderExtensions {

        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, ObjectGraphNodeJsonSet.Entry entry) => provider.Create(entry.id, entry.nodeType, entry.layout);
        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, Type type, Rect layout) => provider.Create(Guid.NewGuid().ToString(), type, layout);

        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, string id, ObjectGraphModel.NodeEntry entry, Rect layout) => provider.Create(id, entry.type, layout);
        public static ObjectGraphNode Create(this IObjectGraphNodeProvider provider, ObjectGraphModel.NodeEntry entry, Rect layout) => provider.Create(Guid.NewGuid().ToString(), entry, layout);

        public static ObjectGraphNode[] CollectRoots(this IObjectGraphNodeProvider provider, ObjectGraphView graphView) => provider.CollectNodes(graphView).Where((n) => n?.IsRoot == true).ToArray();
        public static bool Deserialize(this IObjectGraphNodeProvider provider, SerializedObject target, ObjectGraphView graphView) {
            return provider.Serializer.Deserialize(target, provider, graphView, out SerializedObject _);
        }
        public static bool Serialize(this IObjectGraphNodeProvider provider, SerializedObject target, ObjectGraphView graphView) {
            return provider.Serializer.Serialize(target, provider, graphView, out SerializedObject _);
        }
    }
}