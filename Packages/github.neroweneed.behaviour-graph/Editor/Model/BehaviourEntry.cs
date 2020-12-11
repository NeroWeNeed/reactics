using System;
using System.Linq;
using System.Runtime.InteropServices;
using NeroWeNeed.Commons.Editor;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    [Serializable]
    public class BehaviourEntry : IntermediateEntry<BehaviourGraphNode> {
        public const string ANCHOR_PORT_CLASS = "field-port";
        public const string NODE_CLASS = "behaviour-node";
        public const string VALUE_FIELD_PORT = "value-field-port";
        public const string VALUE_FIELD_CONSTANT_PORT = "value-field-constant-port";
        public const string VALUE_PORT = "value-port";
        public const string MANAGED_FIELD = "managed-field";
        public const string VALUE_FIELD_DEFAULT_EDGE = "value-field-default-edge";

        [SerializeField]
        private string behaviourIdentifier;
        public string BehaviourIdentifier { get => behaviourIdentifier; set => behaviourIdentifier = value; }
        [SerializeField, HideInInspector]
        private byte[] data;

        private byte[] currentBuffer;
        public byte[] Data { get => data; }
        [SerializeField]
        public FieldData[] fields;
        public FieldData[] Fields { get => fields; }
        public BehaviourEntry(string id, Rect layout, BehaviourGraphSettings settings, string behaviourIdentifier = null, string output = null) {
            Id = id;
            Layout = layout;
            this.behaviourIdentifier = behaviourIdentifier;
            this.data = new byte[UnsafeUtility.SizeOf(settings.Behaviours[behaviourIdentifier].configurationType.Value)];
            this.Output = output;
        }
        public BehaviourEntry(string id, BehaviourGraphSettings settings, string behaviourIdentifier = null, string output = null) : this(id, new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE), settings, behaviourIdentifier, output) { }

        public BehaviourEntry(Rect layout, BehaviourGraphSettings settings, string behaviourIdentifier = null, string output = null) : this(Guid.NewGuid().ToString("N"), layout, settings, behaviourIdentifier, output) { }
        public BehaviourEntry(BehaviourGraphSettings settings, string behaviourIdentifier = null, string output = null) : this(Guid.NewGuid().ToString("N"), new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE), settings, behaviourIdentifier, output) { }

        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            BehaviourGraphNode node = (BehaviourGraphNode)base.CreateNode(graphView, settings);
            node.AddToClassList(NODE_CLASS);
            var entry = settings.Behaviours[behaviourIdentifier];
            node.title = entry.displayName;
            var configType = entry.configurationType.Value;
            ConfigureMemory(configType, fields, data, out currentBuffer, out fields);
            this.data = currentBuffer;
            ConfigureHandlers(node, graphView, settings);
            var element = ConfigureElement(configType, node);
            ConfigureValueFields(node, graphView, settings, element);
            //node.RefreshValueFields();
            node.RegisterCallback<AttachToPanelEvent>(evt => (evt.target as BehaviourGraphNode)?.RefreshValueFields());
            return node;
        }
        protected ValueTypeElement ConfigureElement(Type configType, Node node) {
            var element = ValueTypeElement.Create(configType, currentBuffer, RenderChildrenOptions.Render);
            element.RegisterCallback<MemoryTreeUpdateEvent>((evt) =>
            {
                var whole = (evt.currentTarget as ValueTypeElement)?.Pointer ?? IntPtr.Zero;
                if (element.IsCreated) {
                    Marshal.Copy(evt.pointer + evt.offset, currentBuffer, evt.offset, (int)evt.length);
                }
            });
            node.Add(element);
            return element;
        }



        protected unsafe void ConfigureValueFields(Node node, BehaviourGraphView graphView, BehaviourGraphSettings settings, ValueTypeElement element) {
            foreach (var terminal in element.Query<VisualElement>(null, ValueTypeElement.TERMINAL_CLASS).ToList()) {
                var field = terminal.GetFirstAncestorOfType<ValueTypeMemoryField>();
                if (field == null)
                    continue;

                var behaviourType = settings.BehaviourType;
                var portType = typeof(VariableFieldPort<,>).MakeGenericType(behaviourType, field.Type);
                var portColor = field.Type.GetColor();
                var containerPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, portType);
                containerPort.portName = "";
                containerPort.name = "connection-port";
                containerPort.portColor = portColor;
                containerPort.AddToClassList(VALUE_FIELD_CONSTANT_PORT);
                var nodePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, portType);
                var label = field.CreateLabelText();
                nodePort.tooltip = $"{field.Type} {label}";
                nodePort.viewDataKey = field.Path;
                nodePort.AddToClassList(VALUE_FIELD_PORT);
                nodePort.AddToClassList(VALUE_PORT);
                nodePort.portColor = portColor;
                field.Add(containerPort);
                nodePort.portName = label;
                node.inputContainer.Add(nodePort);
                containerPort.userData = nodePort;
                var attacher = new Attacher(field, nodePort, SpriteAlignment.LeftCenter);
                //nodePort.userData = new ValuePortUpdater(field, attacher);
                nodePort.userData = attacher;
                field.RegisterCallback<MemoryNodeUpdateEvent>(evt =>
                {
                    if (evt.target is ValueTypeMemoryField field) {
                        field.GetFirstAncestorOfType<BehaviourGraphNode>()?.RefreshValuePortConnections(field);
                    }
                });
                attacher.Reattach();

            }
        }


        internal void ConfigureMemory(Type type, FieldData[] inputFields, byte[] inputBytes, out byte[] outputBytes, out FieldData[] outputFields) {
            outputBytes = new byte[UnsafeUtility.SizeOf(type)];
            inputFields ??= Array.Empty<FieldData>();
            outputFields = type.Decompose().Select(field =>
                {
                    var oldIndex = Array.FindIndex(inputFields, (inputField) => inputField.info.fullName == field.fullName);
                    return new FieldData(field, oldIndex != -1 ? inputFields[oldIndex].data : null);
                }).ToArray();

            foreach (var outputField in outputFields) {
                if (outputField.info.root) {
                    var inputFieldIndex = Array.FindIndex(inputFields, field => field.info.fullName == outputField.info.fullName);
                    if (inputFieldIndex >= 0) {
                        var inputField = inputFields[inputFieldIndex];
                        if (outputField.info.length == inputField.info.length && outputField.info.type.Value == inputField.info.type.Value) {
                            Array.Copy(inputBytes, inputField.info.offset, outputBytes, outputField.info.offset, outputField.info.length);
                        }
                    }
                }
            }
        }


        public void ConfigureHandlers(Node node, BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            for (int index = 0; index < Fields.Length; index++) {
                if (BehaviourGraphFieldHandlers.TryGetBehaviourGraphFieldHandler(Fields[index].info.type.Value, out BehaviourGraphFieldHandler handler)) {
                    var handleInfo = handler.Initialize(node, this, graphView, settings, index);
                    if (handleInfo.element != null) {
                        handleInfo.element.viewDataKey = Fields[index].info.fullName;
                        handleInfo.element.AddToClassList(MANAGED_FIELD);
                        switch (handleInfo.target) {
                            case BehaviourGraphFieldHandler.NodeTarget.Input:
                                node.inputContainer.Add(handleInfo.element);
                                break;
                            case BehaviourGraphFieldHandler.NodeTarget.Output:
                                node.outputContainer.Add(handleInfo.element);
                                break;
                            case BehaviourGraphFieldHandler.NodeTarget.Extension:
                                node.extensionContainer.Add(handleInfo.element);
                                break;

                        }
                    }

                }
            }


        }

        [Serializable]
        public class FieldData {
            public FieldOffsetInfo info;
            [SerializeField, HideInInspector]
            public string data;
            public FieldData(FieldOffsetInfo info, string data) {
                this.info = info;
                this.data = data;
            }
        }


    }

    public class ValuePortUpdater : IPortUpdater {
        private VisualElement container;
        private Attacher attacher;

        public ValuePortUpdater(VisualElement container, Attacher attacher) {
            this.container = container;
            this.attacher = attacher;
        }

        public void OnConnect(Port source, Edge edge) {
            if (container != null && !edge.ClassListContains(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE)) {
                var other = source == edge.input ? edge.output : edge.input;
                container.visible = other.ClassListContains(BehaviourEntry.VALUE_FIELD_CONSTANT_PORT);
                if (container.visible) {
                    var graphView = source.GetFirstAncestorOfType<BehaviourGraphView>();
                    if (graphView != null) {
                        var defaultEdge = source.ConnectTo(other);
                        defaultEdge.capabilities = Capabilities.Collapsible | Capabilities.Deletable;
                        defaultEdge.pickingMode = PickingMode.Ignore;
                        defaultEdge.AddToClassList(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE);
                        graphView?.AddElement(defaultEdge);
                    }
                    attacher?.Reattach();
                }

            }
        }

        public void OnDisconnect(Port source, Edge edge) {
            if (container != null && !edge.ClassListContains(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE)) {

                if (source.connections.Count(e => e != edge) == 0) {
                    container.visible = true;
                    var graphView = source.GetFirstAncestorOfType<BehaviourGraphView>();
                    var containerPort = container.Q<Port>(null, BehaviourEntry.VALUE_FIELD_CONSTANT_PORT);
                    if (graphView != null) {
                        var defaultEdge = source.ConnectTo(containerPort);
                        defaultEdge.capabilities = Capabilities.Collapsible | Capabilities.Deletable;
                        defaultEdge.pickingMode = PickingMode.Ignore;
                        defaultEdge.AddToClassList(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE);
                        graphView?.AddElement(defaultEdge);
                    }
                    attacher?.Reattach();
                }


            }
        }
        public void Refresh(Port source) {
            if (source.connections.Count(c => !c.ClassListContains(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE)) > 0) {
                container.visible = false;
                source.GetFirstAncestorOfType<BehaviourGraphView>()?.DeleteElements(source.connections.Where(c => c.ClassListContains(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE)));
            }
            else {
                container.visible = true;
                var graphView = source.GetFirstAncestorOfType<BehaviourGraphView>();
                var containerPort = container.Q<Port>(null, BehaviourEntry.VALUE_FIELD_CONSTANT_PORT);
                if (graphView != null) {
                    var defaultEdge = source.ConnectTo(containerPort);
                    defaultEdge.capabilities = Capabilities.Collapsible | Capabilities.Deletable;
                    defaultEdge.pickingMode = PickingMode.Ignore;
                    defaultEdge.AddToClassList(BehaviourEntry.VALUE_FIELD_DEFAULT_EDGE);
                    graphView?.AddElement(defaultEdge);
                }
                attacher?.Reattach();
            }
        }
    }

}