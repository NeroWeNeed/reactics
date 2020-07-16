using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ValuePortElement : VisualElement {
        public const string ValuePortContainerClassName = "value-port-container";
        public const string ValuePortContainerElementClassName = "value-port-container-element";
        private Attacher attacher = null;
        private VisualElement element;

        private Container container;
        private Port port;
        /*         private bool defaultElementValueSet = false;
                private object defaultElementValue; */
        public static ValuePortElement Create(VisualElementDrawer drawer, Node node, Direction direction) {
            var valuePort = Create(element: drawer, node, direction);
            valuePort.port.portName = drawer.Label;
            drawer.Label = null;


            /*             drawer.OnValueChanged((_, newValue) =>
                        {
                            if (valuePort.container.panel != null && valuePort.port.panel != null) {
                                var graphView = valuePort.GetFirstAncestorOfType<GraphView>();

                                if (!valuePort.defaultElementValueSet)
                                    valuePort.defaultElementValue = newValue.GetType().IsValueType ? Activator.CreateInstance(newValue.GetType()) : null;
                                if (EqualityComparer<object>.Default.Equals(newValue, valuePort.defaultElementValue)) {
                                    Debug.Log("G");
                                    valuePort.port.DisconnectAll();
                                    valuePort.container.port.DisconnectAll();
                                }
                                else if (!valuePort.port.connections.Any((edge) => valuePort.port.direction == Direction.Input ? edge.output == valuePort.container.port : edge.input == valuePort.container.port)) {
                                    graphView.AddElement(valuePort.port.ConnectTo(valuePort.container.port));
                                }
                            }
                        }); */
            return valuePort;
        }
        public static ValuePortElement Create(VisualElement element, Node node, Direction direction) {
            var valuePort = new ValuePortElement(element, direction);
            node.Add(valuePort.container);
            return valuePort;
        }
        public ValuePortElement(VisualElement element, Direction direction) {
            Init(element, direction, null);
        }
        private void Init(VisualElement element, Direction direction, Type portType) {
            this.element = element;
            port = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, portType);
            this.Add(port);
            this.container = new Container(this);
            this.AddToClassList(ObjectGraphNode.ConfigurableFieldClassName);
            this.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            this.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            /*             this.port.RegisterCallback<GeometryChangedEvent>((evt) =>
                        {
                            container.visible = port.visible;
                            foreach (var connection in container.port.connections) {
                                connection.visible = port.visible;
                            }
                            foreach (var connection in port.connections) {
                                connection.visible = port.visible;
                            }
                        }); */
            this.container.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

        }
        private void OnDetachFromPanel(DetachFromPanelEvent evt) {
            if (attacher != null) {
                attacher.Detach();
                foreach (var con in port.connections) {
                    con.RemoveFromHierarchy();
                }
                port.DisconnectAll();
                this.container.port.DisconnectAll();
                attacher = null;
            }
        }
        private void OnAttachToPanel(AttachToPanelEvent evt) {
            Debug.Log(port.node);
            if (port.node != null && this.port.FindCommonAncestor(this.container) != null) {
                container.RemoveFromHierarchy();
                port.node.Add(container);
                attacher = new Attacher(this.container, port, port.direction == Direction.Input ? SpriteAlignment.LeftCenter : SpriteAlignment.RightCenter);
                var graphView = this.GetFirstAncestorOfType<GraphView>();
                if (graphView != null) {
                    if (port.connected)
                        port.DisconnectAll();
                    var edge = this.container.port.ConnectTo(this.port);
                    edge.capabilities = 0;
                    graphView.AddElement(edge);
                }


            }
        }



        public void Attach() {
            if (this.container.panel != null && port.panel != null && attacher == null && this.port.FindCommonAncestor(this.container) != null) {
                attacher = new Attacher(this.container, port, port.direction == Direction.Input ? SpriteAlignment.LeftCenter : SpriteAlignment.RightCenter);


                var graphView = this.GetFirstAncestorOfType<GraphView>();
                if (graphView != null) {
                    var edge = this.container.port.ConnectTo(this.port);
                    edge.capabilities = 0;
                    graphView.AddElement(edge);
                }
            }
        }
        public void Detach() {
            attacher?.Detach();
            foreach (var connection in port.connections) {
                connection.RemoveFromHierarchy();
            }
            foreach (var connection in container.port.connections) {
                connection.RemoveFromHierarchy();
            }
            this.port.DisconnectAll();
            this.container.port.DisconnectAll();
            attacher = null;
        }
        /*         public void Reattach() {
                    if (attacher != null && this.container.panel != null && port.panel != null)
                        attacher.Reattach();
                    foreach (var connection in port.connections) {
                        connection.RemoveFromHierarchy();
                    }
                    foreach (var connection in container.port.connections) {
                        connection.RemoveFromHierarchy();
                    }
                    this.port.DisconnectAll();
                    this.container.port.DisconnectAll();
                    var graphView = this.GetFirstAncestorOfType<GraphView>();
                    if (graphView != null) {
                        var edge = this.container.port.ConnectTo(this.port);
                        edge.capabilities = 0;
                        graphView.AddElement(edge);
                    }
                } */
        public class Container : VisualElement {
            public Port port;
            public VisualElement element;

            public Container(ValuePortElement source) {
                this.AddToClassList(ObjectGraphNode.ConfigurableFieldClassName);
                this.AddToClassList(ValuePortContainerClassName);
                this.port = Port.Create<Edge>(Orientation.Horizontal, source.port.direction == Direction.Input ? Direction.Output : Direction.Input, Port.Capacity.Single, source.port.portType);
                this.port.portName = null;
                this.element = source.element;
                this.element.AddToClassList(ValuePortContainerElementClassName);
                this.Add(element);
                this.Add(port);
            }
        }
    }
}