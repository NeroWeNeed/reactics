using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Core.Commons;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Reactics.Core.Editor.Graph {
    [CustomVisualElementProvider(typeof(NodeReference))]
    [EmbeddedOutputLayout]
    //TODO: Edge not deleted when port deleted.
    public class ObjectGraphNodePort : VisualElementDrawer<NodeReference> {
        private static Dictionary<Type, Color> PortColorRegistry = new Dictionary<Type, Color>();

        public static bool TryGetColor(Type type, out Color color) {
            if (type == null) {
                color = Color.black;
                return false;
            }
            else
                return PortColorRegistry.TryGetValue(type, out color);
        }

        public static Color GetColor(Type type) {
            if (type == null)
                return Color.black;
            else
                return PortColorRegistry[type];
        }

        public static void RegisterColor(Type type, Color color) {
            PortColorRegistry[type] = color;
        }
        public Port Port { get; protected set; }
        public override string Label { get => Port.portName; set => Port.portName = value; }




        public override void Initialize(string label, NodeReference initialValue, Attribute[] attributes = null) {
            _value = initialValue;
            Type portType = ((attributes != null ? Array.Find(attributes, (attr) => attr is SerializeNodeIndex) : null) as SerializeNodeIndex)?.nodeType;
            Port = Create<Edge>(Orientation.Horizontal, Direction.Output, Capacity.Single, portType);
            Port = PortUtility.Create(portType, label, Direction.Output, Capacity.Single, TryGetColor(Port.portType, out Color portColor) ? portColor : Color.white);
            Port.MakeObservable();


            this.Add(Port);
            Port.RegisterCallback<PortChangedEvent>((evt) =>
            {

                if (evt.edges != null) {
                    value = new NodeReference(evt.edges.FirstOrDefault()?.input?.node?.viewDataKey);
                }
                else {
                    value = default;
                }

            });
            this.RegisterCallback<DetachFromPanelEvent>((evt) =>
            {

                foreach (var connection in Port.connections) {
                    connection.RemoveFromHierarchy();
                }


            });
            this.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                Reconnect();
            });

        }

        private void Reconnect() {
            if (!string.IsNullOrEmpty(value.nodeId)) {
                if (panel == null)
                    return;
                var graphView = GetFirstAncestorOfType<ObjectGraphView>();
                if (graphView == null)
                    return;
                var target = graphView.GetNodeByGuid(value.nodeId);
                if (target == null) {
                    _value = default;
                    Port.DisconnectAll();
                    return;
                }
                if (target is ObjectGraphNode targetObjectGraphNode) {
                    if (Port.connections.Count() > 0 && (Port.connections.Count() > 1 || Port.connections.First().input.node != target))
                        Port.DisconnectAll();
                    var edge = Port.ConnectTo(targetObjectGraphNode.InputPort);

                    if (edge != null)
                        graphView.AddElement(edge);
                    else
                        Port.DisconnectAll();

                }
                else if (Port.node is ObjectGraphNode sourceObjectGraphNode) {
                    if (Port.connections.Count() > 0 && (Port.connections.Count() > 1 || Port.connections.First().input.node.viewDataKey != value.nodeId))
                        Port.DisconnectAll();
                    var edge = sourceObjectGraphNode.ConnectToMaster(Port, target);

                    if (edge != null)
                        graphView.AddElement(edge);
                    else
                        Port.DisconnectAll();
                }
            }
            else
                Port.DisconnectAll();
        }

        public override void SetValueWithoutNotify(NodeReference newValue) {
            _value = newValue;
            Reconnect();

        }

    }



}