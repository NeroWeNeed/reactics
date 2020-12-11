using System;
using System.Collections.Generic;
using System.Linq;
using NeroWeNeed.Commons.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {


    [Serializable]
    public class VariableEntry : BaseEntry {
        public const string NODE_CLASS = "variable-node";
        public const string VARIABLE_PORT = "variable-port";

        [SerializeField]
        private FieldOffsetInfo info;
        public FieldOffsetInfo Info { get => info; set => info = value; }
        [SerializeField]
        private List<Output> outputs = new List<Output>();
        public List<Output> Outputs { get => outputs; }
        public override int Priority { get => -2; }
        public VariableEntry(FieldOffsetInfo info, string id, Rect layout) {
            Info = info;
            Id = id;
            Layout = layout;
        }
        public VariableEntry(FieldOffsetInfo info, string id) : this(info, id, new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE)) { }

        public VariableEntry(FieldOffsetInfo info, Rect layout) : this(info, Guid.NewGuid().ToString("N"), layout) { }
        public VariableEntry(FieldOffsetInfo info) : this(info, Guid.NewGuid().ToString("N"), new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE)) { }

        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            var portType = typeof(VariableFieldPort<,>).MakeGenericType(settings.BehaviourType, Info.type.Value);
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, portType);
            outputPort.portName = "";
            outputPort.portColor = info.type.Value.GetColor();
            outputPort.AddToClassList(VARIABLE_PORT);
            outputPort.userData = new VariableOutputPortUpdater(this);
            outputPort.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (evt.target is Port self && !self.connected) {
                    var graphView = self.GetFirstAncestorOfType<BehaviourGraphView>();

                    foreach (var output in Outputs) {
                        var targetPort = graphView?.GetNodeByGuid(output.node)?.inputContainer?.Query<Port>(null, BehaviourEntry.VALUE_PORT).Where(p => p.viewDataKey == output.field).First();
                        if (targetPort != null) {
                            graphView.AddElement(self.ConnectTo(targetPort));
                            (targetPort.userData as ValuePortUpdater)?.Refresh(targetPort);
                        }
                    }
                }
            });
            var node = new BehaviourGraphVariableNode(null, outputPort)
            {
                viewDataKey = Id
            };
            node.title = $"{settings.variableDefinition.Value.Name}.{info.fullName}";
            node.AddToClassList(NODE_CLASS);
            node.SetPosition(Layout);
            node.tooltip = $"{Info.type.Value.FullName}";

            return node;
        }
        [Serializable]
        public struct Output {
            public string node;
            public string field;
        }
    }
    public class VariableOutputPortUpdater : IPortUpdater {
        public VariableEntry entry;

        public VariableOutputPortUpdater(VariableEntry entry) {
            this.entry = entry;
        }

        public void OnConnect(Port source, Edge edge) {
            var other = source == edge.input ? edge.output : edge.input;
            var output = new VariableEntry.Output
            {
                node = other.node.viewDataKey,
                field = other.viewDataKey
            };
            if (!entry.Outputs.Contains(output))
                entry.Outputs.Add(output);
        }

        public void OnDisconnect(Port source, Edge edge) {
            var other = source == edge.input ? edge.output : edge.input;
            entry.Outputs.RemoveAll(e => other.node.viewDataKey == e.node && other.viewDataKey == e.field);
        }
    }

}