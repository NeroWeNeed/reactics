using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ValuePortElement : VisualElement {
        public const string ValuePortContainerClassName = "value-port-container";
        public const string ValuePortClassName = "value-port";
        public const string ValuePortElementClassName = "value-port-element";
        public const string ValuePortContainerElementClassName = "value-port-container-element";
        public const string ValuePortHiddenContainerElementClassName = "value-port-container-element--hidden";
        public const string ValuePortVisibleContainerElementClassName = "value-port-container-element--visible";
        private Attacher attacher = null;
        private VisualElement element;

        private Container container;

        private bool displayElement;
        public bool DisplayElement
        {
            get => displayElement;
            set
            {
                if (displayElement == value)
                    return;
                Debug.Log("SETTING TO " + value + " FROM " + displayElement);
                if (value) {
                    container.RemoveFromClassList(ValuePortHiddenContainerElementClassName);
                    container.AddToClassList(ValuePortVisibleContainerElementClassName);
                    /* this.port.DisconnectAll();
                    this.container.port.DisconnectAll();
                    this.GetFirstAncestorOfType<GraphView>()?.AddElement(port.ConnectTo(container.port)); */

                }
                else {
                    container.RemoveFromClassList(ValuePortVisibleContainerElementClassName);
                    container.AddToClassList(ValuePortHiddenContainerElementClassName);
                    /*                     if (port.connected && port.connections.Any((connection) => connection.output?.node is ObjectGraphNode))
                                            container.port.DisconnectAll(); */
                }
                displayElement = value;

            }
        }
        private Port port;

        private string nodeId = null;

        private ObjectGraphModel model;
        private Node node;
        private string field;

        private ObjectGraphVariableNode variableNode;

        public ObjectGraphVariableNode VariableNode
        {
            get => variableNode; set
            {
                this.variableNode = value;
                //Refresh();
            }
        }

        public static ValuePortElement Create(VisualElementDrawer drawer, Node node, Direction direction, Type portType = null) {
            var valuePort = Create(element: drawer, node, direction, portType);
            valuePort.port.portName = drawer.Label;
            drawer.Label = null;
            return valuePort;
        }
        public static ValuePortElement Create(VisualElement element, Node node, Direction direction, Type portType = null) {
            var valuePort = new ValuePortElement(element, direction, node, portType);
            node.Add(valuePort.container);
            if (node is ObjectGraphNode oNode) {
                valuePort.nodeId = oNode.Id;
                valuePort.model = oNode.Model;
            }

            valuePort.node = node;
            return valuePort;
        }
        public ValuePortElement(VisualElement element, Direction direction, Node node, Type portType = null) {
            Init(element, direction, portType);
        }
        public void RefreshDisplayState() {

            DisplayElement = this.port.connections.All((edge) => edge.input.node == edge.output.node);

        }
        private void Init(VisualElement element, Direction direction, Type portType) {
            this.element = element;
            this.AddToClassList(ValuePortElementClassName);
            port = PortUtility.Create(portType, null, direction, Port.Capacity.Single, portType.GetColor());
            port.MakeObservable();
            port.AddToClassList(ValuePortClassName);
            this.tooltip = $"{portType.GetRealName()}";
            if (element is VisualElementDrawer d) {
                Debug.Log(d.viewDataKey);
                port.viewDataKey = d.viewDataKey;
                this.viewDataKey = d.viewDataKey;
            }
            port.RegisterCallback<PortChangedEvent>((evt) =>
            {
                this.variableNode = evt.edges.FirstOrDefault()?.output?.node as ObjectGraphVariableNode;
                Refresh();
            });


            this.Add(port);
            this.container = new Container(this)
            {
                tooltip = $"{portType.GetRealName()}"
            };

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
            //this.container.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            DisplayElement = true;
        }
        private void Refresh() {
            return;
            if (variableNode == null) {
                /* if (port.connected)
                    port.DisconnectAll(); */
                //model.RemoveVariable(nodeId, viewDataKey);
                DisplayElement = true;
            }
            else {
                //if (!port.connected)
                //this.GetFirstAncestorOfType<GraphView>()?.AddElement(port.ConnectTo(variableNode.output));
                //model.SetVariable(nodeId, viewDataKey, variableNode.Data);
                container.RemoveFromClassList(ValuePortVisibleContainerElementClassName);
                container.AddToClassList(ValuePortHiddenContainerElementClassName);
                displayElement = false;
            }
            /* 
                         */
        }
        private void InitConnectionStatus() {

        }
        private void OnDetachFromPanel(DetachFromPanelEvent evt) {
            if (attacher != null) {
                attacher.Detach();
                attacher = null;
            }
        }
        private void OnAttachToPanel(AttachToPanelEvent evt) {
            if (!(port.connected && port.connections.First().output.node is ObjectGraphVariableNode)) {
                if (port.node != null && this.port.FindCommonAncestor(this.container) != null) {
                    container.Disconnect();
                    container.RemoveFromHierarchy();
                    port.node.Add(container);
                    attacher = new Attacher(this.container, port, port.direction == Direction.Input ? SpriteAlignment.LeftCenter : SpriteAlignment.RightCenter);
                    var graphView = this.GetFirstAncestorOfType<GraphView>();
                    /*                     if (graphView != null) {
                                            if (port.connected)
                                                port.DisconnectAll();
                                            if (displayElement) {
                                                var edge = this.container.port.ConnectTo(this.port);
                                                //edge.capabilities = 0;
                                                graphView.AddElement(edge);
                                            }
                                        } */
                }

            }
            else {

            }

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
                this.port = PortUtility.Create(source.port.portType, null, source.port.direction == Direction.Input ? Direction.Output : Direction.Input, Port.Capacity.Single, source.port.portType.GetColor());

                this.element = source.element;
                this.element.AddToClassList(ValuePortContainerElementClassName);
                this.Add(element);
                this.Add(port);
            }
            public void Disconnect() {
                port.DisconnectAll();
            }
        }

    }
    public class ValuePortEdgeConnector : IEdgeConnectorListener {
        public void OnDrop(GraphView graphView, Edge edge) {
            Debug.Log("HIT");
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            Debug.Log("MISS");
        }
    }




}