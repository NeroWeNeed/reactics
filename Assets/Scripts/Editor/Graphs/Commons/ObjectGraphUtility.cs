using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;
using Reactics.Commons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {



    public static class ObjectGraphUtility {

        public static TNode[] CollectNodes<TNode>(Port origin) where TNode : ObjectGraphNode {
            if (origin == null || !origin.connected)
                return default;
            var nodes = new List<TNode>();
            foreach (var connection in origin.connections) {
                CollectNodes(connection.output?.node, nodes);
            }
            return nodes.ToArray();
        }
        private static void CollectNodes<TNode>(Node target, List<TNode> nodes) where TNode : ObjectGraphNode {
            if (target is TNode node && !nodes.Contains(node)) {
                nodes.Add(node);
                node.Query<Port>().ForEach((port) =>
                {
                    switch (port.direction) {
                        case Direction.Input:
                            foreach (var connection in port.connections)
                                CollectNodes<TNode>(connection.output?.node, nodes);
                            break;
                        case Direction.Output:
                            foreach (var connection in port.connections)
                                CollectNodes<TNode>(connection.input?.node, nodes);
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
            else if (target is ObjectGraphNode objectGraphNode) {
                graphView.AddElement(port.ConnectTo(objectGraphNode.InputPort));
            }
            else {
                var edge = node.ConnectToMaster(port, target);
                if (edge != null)
                    graphView.AddElement(edge);


            }
        }

    }


}