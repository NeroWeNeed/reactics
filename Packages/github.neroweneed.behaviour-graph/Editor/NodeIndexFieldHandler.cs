using System.Linq;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {
    [BehaviourGraphFieldHandler(typeof(NodeIndex))]
    public sealed class NodeIndexBehaviourGraphFieldHandler : BehaviourGraphFieldHandler {
        public const string NODE_INDEX_PORT = "node-index-output-port";
        public override HandleData Initialize(Node node, BehaviourEntry entry, BehaviourGraphView graphView, BehaviourGraphSettings settings, int index) {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BehaviourPort<>).MakeGenericType(settings.BehaviourType));
            port.portName = (index >= 0 && index < (entry.Fields?.Length ?? 0)) ? entry.Fields[index].info.fullName : null;
            port.portColor = settings.BehaviourType.GetColor();
            port.AddToClassList(NODE_INDEX_PORT);
            port.userData = new FieldPortUpdater(entry, index);
            port.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (evt.target is Port self && !self.connected) {
                    var graphView = self.GetFirstAncestorOfType<BehaviourGraphView>();
                    if (graphView != null && self.userData is FieldPortUpdater updater && updater.index >= 0 && updater.index < (updater.entry.Fields?.Length ?? 0) && graphView.GetNodeByGuid(updater.entry.Fields[updater.index].data) is Node node) {
                        var inputPort = node.Q<Port>(null, IntermediateEntry.INPUT_PORT);
                        if (inputPort != null) {
                            graphView.AddElement(self.ConnectTo(inputPort));
                        }
                    }



                }
            });
            return new HandleData
            {
                element = port,
                target = NodeTarget.Output
            };


        }

        public override string Serialize(Node node, BehaviourGraphView graphView, BehaviourGraphSettings settings, FieldOffsetInfo fieldOffsetInfo, VisualElement element) {
            if (element is Port port && port.connected) {
                var connectedNode = port.connections.First().input.node;

                if (connectedNode is Node) {
                    Debug.Log(connectedNode.viewDataKey);
                    return connectedNode.viewDataKey;
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }


        }
    }
    public class FieldPortUpdater : IPortUpdater {

        public BehaviourEntry entry;
        public int index;

        public FieldPortUpdater(BehaviourEntry entry, int index) {
            this.entry = entry;
            this.index = index;
        }

        public void OnConnect(Port source, Edge edge) {
            var other = source == edge.input ? edge.output : edge.input;
            entry.fields[index].data = other?.node?.viewDataKey;
        }

        public void OnDisconnect(Port source, Edge edge) {
            entry.fields[index].data = null;
        }
    }

}