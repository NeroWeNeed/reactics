using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public sealed class VariableFieldPort<TBehaviour, TField> {

    }
    public sealed class BehaviourPort<TBehaviour> {

    }
    public interface IBehaviourGraphNode {
        public BehaviourGraphModel Model { get; set; }
    }
    public class BehaviourGraphNode : Node, IBehaviourGraphNode {
        public BehaviourGraphModel Model { get; set; }
        protected override void ToggleCollapse() {
            base.ToggleCollapse();
            RefreshValueFields();
        }
        public void RefreshValueFields() {
            if (expanded) {
                this.Query<ValueTypeMemoryField>().ForEach(v =>
                {
                    v.visible = true;
                    RefreshValuePortConnections(v);
                });
            }
            else {
                foreach (var field in this.Query<ValueTypeMemoryField>().ToList()) {
                    if (field.IsDefault) {
                        ((field.userData as Port)?.userData as Attacher)?.Detach();
                        field.visible = false;
                    }
                }

            }
        }
        public void RefreshValuePortConnections(ValueTypeMemoryField field) {
            var port = field.Q<Port>("connection-port");
            var graphView = field.GetFirstAncestorOfType<GraphView>();
            if (port?.userData is Port other && graphView != null) {
                if (field.IsDefault) {
                    var edges = port.connections?.Where(e => e.ClassListContains(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE))?.ToArray();
                    if (edges != null) {
                        foreach (var edge in edges) {
                            edge.input?.Disconnect(edge);
                            edge.output?.Disconnect(edge);
                        }
                        graphView.DeleteElements(edges);

                    }

                }
                else {
                    if (!port.connected || !port.connections.Any(e => e.input == other)) {
                        var defaultEdge = port.ConnectTo(other);
                        defaultEdge.capabilities = Capabilities.Collapsible | Capabilities.Deletable;
                        defaultEdge.pickingMode = PickingMode.Ignore;
                        defaultEdge.AddToClassList(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE);
                        graphView.AddElement(defaultEdge);
                    }
                }

            }
        }
    }
    public class BehaviourGraphVariableNode : TokenNode, IBehaviourGraphNode {
        public BehaviourGraphVariableNode(Port input, Port output) : base(input, output) {
        }
        public BehaviourGraphModel Model { get; set; }
    }

    public static class PortExtensions {
        public static Port GetInputPort(this Node node) {
            return node.inputContainer.Q<Port>(null, IntermediateEntry.INPUT_PORT);
        }
        public static Port GetOutputPort(this Node node) {
            return node.outputContainer.Q<Port>(null, IntermediateEntry.OUTPUT_PORT);
        }
        public static List<Node> GetPath(this Port port) {
            var nodes = new List<Node>();
            GetPath(port, nodes);
            return nodes;
        }

        private static void GetPath(Port port, List<Node> nodes) {
            if (port.node == null)
                return;
            nodes.Add(port.node);
            var connections = port.node.Q<Port>(null, IntermediateEntry.INPUT_PORT)?.connections;
            if (connections == null)
                return;
            foreach (var connection in connections) {
                GetPath(connection.output, nodes);
            }
        }
    }
}