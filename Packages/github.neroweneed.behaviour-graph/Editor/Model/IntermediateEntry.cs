using System;
using NeroWeNeed.Commons.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    public abstract class IntermediateEntry : BaseEntry {
        public const string INPUT_PORT = "input-port";
        public const string PRIMARY_INPUT_PORT = "primary-input-port";
        public const string OUTPUT_PORT = "output-port";
        public const string PRIMARY_OUTPUT_PORT = "primary-output-port";
        public const string OUTPUT_PORT_NAME = "Output";
        public const string INPUT_PORT_NAME = "Input";
        [SerializeField]
        private string output;
        public string Output { get => output; set => output = value; }
        public override int Priority { get => 0; }

        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            Node node = base.CreateNode(graphView, settings);
            ConfigurePorts(node, settings);
            return node;
        }
        protected virtual void ConfigurePorts(Node node, BehaviourGraphSettings settings) {
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BehaviourPort<>).MakeGenericType(settings.BehaviourType));
            var portColor = settings.BehaviourType.GetColor();

            outputPort.AddToClassList(IntermediateEntry.OUTPUT_PORT);
            outputPort.AddToClassList(IntermediateEntry.PRIMARY_OUTPUT_PORT);
            outputPort.portName = OUTPUT_PORT_NAME;
            outputPort.portColor = portColor;
            outputPort.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (evt.target is Port self && !self.connected) {
                    var graphView = self.GetFirstAncestorOfType<BehaviourGraphView>();
                    if (graphView?.GetNodeByGuid(Output) is Node target) {
                        var inputPort = target.Q<Port>(null, IntermediateEntry.INPUT_PORT);
                        if (inputPort != null) {
                            graphView.AddElement(self.ConnectTo(inputPort));
                        }
                    }
                }
            });
            outputPort.userData = new OutputPortUpdater(this);
            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BehaviourPort<>).MakeGenericType(settings.BehaviourType));

            inputPort.portName = INPUT_PORT_NAME;
            inputPort.portColor = portColor;
            inputPort.AddToClassList(IntermediateEntry.INPUT_PORT);
            inputPort.AddToClassList(IntermediateEntry.PRIMARY_INPUT_PORT);
            node.inputContainer.Add(inputPort);
            node.outputContainer.Add(outputPort);
        }
        public override int CompareTo(IEntry other) {
            var c1 = base.CompareTo(other);
            if (c1 == 0 && other is IntermediateEntry intermediate) {
                return (Convert.ToInt32(output == intermediate.Id) * 1) + (Convert.ToInt32(Id == intermediate.output) * -1);
            }
            else {
                return c1;
            }
        }
        public class OutputPortUpdater : IPortUpdater {
            public IntermediateEntry entry;

            public OutputPortUpdater(IntermediateEntry entry) {
                this.entry = entry;
            }

            public void OnConnect(Port source, Edge edge) {
                var other = source == edge.input ? edge.output : edge.input;
                entry.Output = other?.node?.viewDataKey;
            }

            public void OnDisconnect(Port source, Edge edge) {
                entry.Output = null;
            }
        }


    }
    public abstract class IntermediateEntry<TNode> : IntermediateEntry where TNode : Node, new() {
        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            TNode node = new TNode();
            node.viewDataKey = Id;
            node.SetPosition(Layout);
            ConfigurePorts(node, settings);
            return node;
        }
    }
}