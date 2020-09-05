using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphValuePort : VisualElement {
        public const string ValuePortContainerClassName = "value-port-container";
        public const string ValuePortClassName = "value-port";

        public const string DirectPortClassName = "direct-port";
        public const string ValuePortElementClassName = "value-port-element";
        public const string ValuePortContainerElementClassName = "value-port-container-element";
        public const string ValuePortHiddenContainerElementClassName = "value-port-container-element--hidden";
        public const string ValuePortVisibleContainerElementClassName = "value-port-container-element--visible";
        private ObjectGraphValuePortContainer container;
        public Port input;
        private Attacher attacher = null;
        public ObjectGraphValuePort(VisualElementDrawer drawer, Direction direction) {
            Init(drawer, direction, drawer.GetValueType());
        }
        public void Init(VisualElementDrawer element, Direction direction, Type portType = null) {
            container = new ObjectGraphValuePortContainer(element, direction == Direction.Input ? Direction.Output : Direction.Input, portType)
            {
                tooltip = portType.GetRealName()
            };
            input = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, portType);
            input.portName = element.Label;
            input.MakeObservable();
            input.MakeDependent();
            input.portColor = element.GetValueType().GetColor();

            container.output.portType = element.GetValueType();
            container.output.portColor = element.GetValueType().GetColor();
            element.Label = null;
            input.viewDataKey = element.viewDataKey;
            container.output.viewDataKey = element.viewDataKey;
            this.viewDataKey = element.viewDataKey;
            this.Add(input);
            this.AddToClassList(ValuePortElementClassName);
            input.AddToClassList(ValuePortClassName);
            input.AddToClassList(DirectPortClassName);
            this.AddToClassList(ObjectGraphNode.ConfigurableFieldClassName);
            this.RegisterCallback<AttachToPanelEvent>(OnAttach);
            this.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            input.RegisterCallback<PortChangedEvent>(OnPortChanged);
        }
        public void OnPortChanged(PortChangedEvent evt) {
            var current = evt.target as Port;
            if (evt.edges?.Any() != true) {
                if (!container.ClassListContains(ValuePortVisibleContainerElementClassName)) {
                    container.AddToClassList(ValuePortVisibleContainerElementClassName);
                }
                if (container.ClassListContains(ValuePortHiddenContainerElementClassName)) {
                    container.RemoveFromClassList(ValuePortHiddenContainerElementClassName);
                }
                var gv = current.GetFirstAncestorOfType<GraphView>();
                var edge = input.ConnectTo(container.output);
                edge.capabilities = 0;
                gv.AddElement(edge);
            }
            else {
                var node = this.GetFirstAncestorOfType<Node>();
                if (evt.edges.Any((edge) => edge.output.node != node)) {
                    var gv = current.GetFirstAncestorOfType<GraphView>();
                    foreach (var edge in evt.edges.Where((edge) => edge.output.node == node).ToArray()) {
                        current.Disconnect(edge);
                        gv.RemoveElement(edge);
                    }
                    if (container.ClassListContains(ValuePortVisibleContainerElementClassName)) {
                        container.RemoveFromClassList(ValuePortVisibleContainerElementClassName);
                    }
                    if (!container.ClassListContains(ValuePortHiddenContainerElementClassName)) {
                        container.AddToClassList(ValuePortHiddenContainerElementClassName);
                    }
                }
            }
        }
        public void OnAttach(AttachToPanelEvent evt) {
            var current = evt.target as ObjectGraphValuePort;
            var parent = current?.GetFirstAncestorOfType<Node>();
            var gv = current.GetFirstAncestorOfType<GraphView>();
            if (parent == null)
                return;
            if (container.parent != parent) {
                attacher?.Detach();
                container.RemoveFromHierarchy();
                parent.Add(container);
                attacher = new Attacher(container, current.input, input.direction == Direction.Input ? SpriteAlignment.LeftCenter : SpriteAlignment.RightCenter);
            }
            if (!input.connected) {
                var edge = input.ConnectTo(container.output);
                edge.capabilities = 0;
                gv.AddElement(edge);
                container.AddToClassList(ValuePortVisibleContainerElementClassName);
            }
            else {
                container.AddToClassList(ValuePortHiddenContainerElementClassName);
            }
        }
        public void OnDetach(DetachFromPanelEvent evt) {
            if (attacher != null) {
                attacher.Detach();
                attacher = null;
            }
        }

    }
    public class ObjectGraphValuePortContainer : VisualElement {
        internal VisualElement element;
        internal Port output;
        public ObjectGraphValuePortContainer(VisualElement element, Direction direction, Type portType) {
            this.element = element;
            output = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, portType);
            output.portColor = TypeCommons.GetColor(portType);
            output.portName = string.Empty;
            output.MakeDependent();
            output.AddToClassList(ObjectGraphValuePort.DirectPortClassName);
            this.AddToClassList(ObjectGraphNode.ConfigurableFieldClassName);
            this.AddToClassList(ObjectGraphValuePort.ValuePortContainerClassName);
            this.element.AddToClassList(ObjectGraphValuePort.ValuePortContainerElementClassName);
            this.Add(element);
            this.Add(output);
        }
    }
}