using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {



    public static class ObjectGraphUtility {

        public static ObjectGraphNode[] CollectNodes(Port origin) {
            if (origin?.connected != true)
                return default;
            var nodes = new List<ObjectGraphNode>();
            foreach (var connection in origin.connections) {
                CollectNodes(connection.output?.node, nodes);
            }
            return nodes.ToArray();
        }
        private static void CollectNodes(Node target, List<ObjectGraphNode> nodes) {
            if (target is ObjectGraphNode node && !nodes.Contains(node)) {
                nodes.Add(node);
                node.Query<Port>().ForEach((port) =>
                {
                    switch (port.direction) {
                        case Direction.Input:
                            foreach (var connection in port.connections)
                                CollectNodes(connection.output?.node, nodes);
                            break;
                        case Direction.Output:
                            foreach (var connection in port.connections)
                                CollectNodes(connection.input?.node, nodes);
                            break;
                    }

                });
            }
        }

        public static void ConnectById(this ObjectGraphNode node, Port port, string guid) {
            var graphView = port.GetFirstAncestorOfType<GraphView>();
            if (graphView == null)
                return;
            var target = graphView.GetNodeByGuid(guid);

            if (target == null) {
                port.DisconnectAll();
            }
            var targetPort = target.Q<Port>(null, node.TargetInputPortClassName);
            if (targetPort != null) {
                graphView.AddElement(port.ConnectTo(targetPort));
            }
        }

    }


}