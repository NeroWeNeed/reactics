using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Reactics.Editor
{
    [CustomVisualElementProvider(typeof(NodeIndex))]
    [OutputContainerElement]
    public class GraphFieldPort : GraphField<NodeIndex>
    {

        private NodeIndex _value;
        public EffectGraphPort Port { get; protected set; }
        public override NodeIndex value
        {
            get => _value; set
            {
                if (!value.Equals(_value))
                {

                    if (panel != null)
                    {
                        using (ChangeEvent<NodeIndex> evt = ChangeEvent<NodeIndex>.GetPooled(_value, value))
                        {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }

        public override void Initialize(string label, NodeIndex initialValue)
        {
            Port = new EffectGraphPort(Orientation.Horizontal, Direction.Output, Capacity.Single, EffectGraphController.GetNodeType(initialValue.type));
            if (!EffectGraphController.PortColors.TryGetValue(Port.portType, out Color portColor))
                portColor = Color.white;
            Port.portColor = portColor;
            Port.portName = label;
            this.Add(Port);
            this.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                SetValueWithoutNotify(initialValue);
            });

        }
        public override void SetValueWithoutNotify(NodeIndex newValue)
        {
            _value = newValue;
            var graphView = this.GetFirstAncestorOfType<GraphView>();
            if (newValue.node != Guid.Empty && graphView != null)
            {
                var node = graphView.GetElementByGuid(newValue.node.ToString());
                if (node != null)
                {
                    var input = node.Q<Port>(null, EffectGraphController.InputPortClassName);
                    if (input != null)
                    {
                        if (!Port.connected || !Port.connections.Any((x) => x.input == input))
                        {
                            if (panel != null)
                            {

                                Port.DisconnectAllWithoutNotify();
                                graphView.AddElement(Port.ConnectToWithoutNotify(input));


                            }
                        }
                    }
                }
            }

        }

    }

}