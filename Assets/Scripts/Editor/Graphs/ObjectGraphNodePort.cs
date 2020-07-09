using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Commons;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace Reactics.Editor.Graph
{
    [CustomVisualElementProvider(typeof(NodeReference))]
    [OutputContainerElement]
    [StandaloneField]
    public class ObjectGraphNodePort : VisualElementDrawer<NodeReference>
    {
        private static Dictionary<Type, Color> PortColorRegistry = new Dictionary<Type, Color>();

        public static bool TryGetColor(Type type, out Color color) => PortColorRegistry.TryGetValue(type, out color);

        public static Color GetColor(Type type)
        {

            return PortColorRegistry[type];
        }

        public static void RegisterColor(Type type, Color color)
        {
            PortColorRegistry[type] = color;
        }
        private NodeReference _value;
        public ObservablePort Port { get; protected set; }



        public override void Initialize(string label, NodeReference initialValue, Attribute[] attributes = null)
        {
            SerializeNodeIndex portType = (attributes != null ? Array.Find(attributes, (attr) => attr is SerializeNodeIndex) : null) as SerializeNodeIndex;
            Port = new ObservablePort(Orientation.Horizontal, Direction.Output, Capacity.Single, EffectGraphModule.GetPortType(portType.nodeType));
            if (!EffectGraphModule.TryGetPortColor(Port.portType, out Color portColor))
                portColor = Color.white;
            Port.portColor = portColor;
            Port.portName = label;
            this.Add(Port);
            this.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                SetValueWithoutNotify(initialValue);
            });

        }
        public override void SetValueWithoutNotify(NodeReference newValue)
        {
            _value = newValue;
            var graphView = this.GetFirstAncestorOfType<GraphView>();
            if (!string.IsNullOrEmpty(newValue.nodeId) && graphView != null)
            {
                var node = graphView.GetElementByGuid(newValue.nodeId);
                if (node != null)
                {
                    var input = node.Q<Port>(null, ObjectGraphNode.InputPortClassName);
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