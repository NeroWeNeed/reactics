using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Core.Commons;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Reactics.Editor.Graph {
    [CustomVisualElementProvider(typeof(NodeReference))]
    [EmbeddedOutputLayout]
    //TODO: Edge not deleted when port deleted.
    public class ObjectGraphNodePort : VisualElementDrawer<NodeReference> {
        public Port Port { get; protected set; }
        public override string Label { get => Port.portName; set => Port.portName = value; }

        private ObjectGraphNode node;



        public override void Initialize(string label, NodeReference initialValue, Attribute[] attributes = null) {
            _value = initialValue;
            Type portType = ((attributes != null ? Array.Find(attributes, (attr) => attr is SerializeNodeIndex) : null) as SerializeNodeIndex)?.nodeType;
            Port = Create<Edge>(Orientation.Horizontal, Direction.Output, Capacity.Single, portType);
            Port = PortUtility.Create(portType, label, Direction.Output, Capacity.Single, Port.portType.GetColor(Color.white));
            Port.MakeObservable();
            Port.MakeDependent();

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
            this.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                node = this.GetFirstAncestorOfType<ObjectGraphNode>();
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
                var targetPort = target.Q<Port>(null, node.TargetInputPortClassName);
                if (targetPort != null) {
                    if (Port.connections.Count() > 0 && (Port.connections.Count() > 1 || Port.connections.First().input.node != target))
                        Port.DisconnectAll();
                    var edge = Port.ConnectTo(targetPort);

                    if (edge != null)
                        graphView.AddElement(edge);
                    else
                        Port.DisconnectAll();
                }
                else {
                    Port.DisconnectAll();
                }
            }
            else {
                Port.DisconnectAll();
            }
        }

        public override void SetValueWithoutNotify(NodeReference newValue) {
            _value = newValue;
            Reconnect();

        }

    }



}