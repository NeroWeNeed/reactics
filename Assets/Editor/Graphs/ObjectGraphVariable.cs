using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Reactics.Core.Effects;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphVariable : IEquatable<ObjectGraphVariable> {

        public string name;
        public string fullName;
        public Type type;
        public Guid containerGuid;
        public Type containerType;
        public IObjectGraphVariableProvider provider;
        public string address;
        public bool valid;


        public ObjectGraphVariable(IObjectGraphVariableProvider provider, Type container, FieldInfo field) {
            this.provider = provider;
            containerGuid = container.GUID;
            containerType = container;
            name = field.Name;
            fullName = $"{container.Name}.{name}";
            type = field.FieldType;
            address = field.Name;
            valid = true;
        }
        public ObjectGraphVariable(IObjectGraphVariableProvider provider, Type container, Type type, string address) {
            this.provider = provider;
            containerGuid = container.GUID;
            containerType = container;
            var lastSeparator = address.LastIndexOf('.');
            this.address = address;
            this.name = lastSeparator > 0 ? address.Substring(lastSeparator) : address;
            fullName = $"{container.GetRealName()}.{address}";
            this.type = type;
            valid = true;
        }
        public bool ResolveAddress(out int offset, out long length) => ResolveAddress(containerType, out offset, out length);
        public bool ResolveAddress(Type target, out int offset, out long length) {
            offset = 0;
            Type type = target;
            var addressSegments = address.Split('.');
            /*             if (addressSegments == null || addressSegments.Length == 0)
                            addressSegments = new string[] { address }; */

            foreach (var step in addressSegments) {
                var field = type.GetField(step, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) {
                    offset = -1;
                    length = -1;
                    return false;
                }
                offset += Marshal.OffsetOf(type, field.Name).ToInt32();
                type = field.FieldType;
            }
            if (type == this.type) {
                length = Marshal.SizeOf(type.IsEnum ? Enum.GetUnderlyingType(type) : type);
                return true;
            }
            else {
                offset = -1;
                length = -1;
                return false;
            }
        }

        public override bool Equals(object obj) {
            if (obj is ObjectGraphVariable objectGraphVariable) {
                return Equals(objectGraphVariable);
            }

            return false;
        }

        public bool Equals(ObjectGraphVariable other) {
            return name == other.name &&
                   fullName == other.fullName &&
                   EqualityComparer<Type>.Default.Equals(type, other.type) &&
                   containerGuid.Equals(other.containerGuid) &&
                   EqualityComparer<Type>.Default.Equals(containerType, other.containerType) &&
                   EqualityComparer<IObjectGraphVariableProvider>.Default.Equals(provider, other.provider);
        }

        public override int GetHashCode() {
            int hashCode = 1781420533;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(fullName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + containerGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(containerType);
            hashCode = hashCode * -1521134295 + EqualityComparer<IObjectGraphVariableProvider>.Default.GetHashCode(provider);
            return hashCode;
        }

        public override string ToString() {
            return $"[{containerGuid}]{fullName} (Type={type})";
        }
        public bool MatchAddress(Variable other, string typeName) {

            if (other.containerId.Equals(containerGuid)) {
                ResolveAddress(out int offset, out long length);
                return offset == other.offset && length == other.length && typeName == type.AssemblyQualifiedName;
            }
            return false;
        }

    }
    public interface IObjectGraphVariableProvider : IEquatable<IObjectGraphVariableProvider> {
        ObjectGraphVariable[] BuildVariables();
        VisualElement BuildField(ObjectGraphVariable variable);
        void OnValidateVariables(ObjectGraphView graphView, ObjectGraphVariable[] variables);
    }


    public class ObjectGraphVariableProvider : IObjectGraphVariableProvider {
        public const string OBJECT_VARIABLE_FIELD_CLASS_NAME = ".object-graph-variable-field";
        public Type Type { get; }
        public int currentDefinition;

        public Action<ObjectGraphView, ObjectGraphVariable[]> validator;
        public ObjectGraphVariableProvider(Type type) {
            if ((type.StructLayoutAttribute.Value == LayoutKind.Sequential || type.StructLayoutAttribute.Value == LayoutKind.Explicit) && !type.IsGenericType) {
                Type = type;
            }
            else {
                throw new ArgumentException("Type Layout must be explicit or sequential");
            }
        }
        public ObjectGraphVariable[] BuildVariables() {

            var variables = new List<ObjectGraphVariable>();

            foreach (var field in Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                AddVariable(Type, field.FieldType, field.Name, variables);
            }
            return variables.ToArray();

        }
        private void AddVariable(Type container, Type type, string address, List<ObjectGraphVariable> variables) {
            variables.Add(new ObjectGraphVariable(this, container, type, address));
            if (type.IsValueType && !type.IsPrimitive && !type.IsEnum && (type.IsLayoutSequential || type.IsExplicitLayout)) {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    AddVariable(container, field.FieldType, $"{address}.{field.Name}", variables);
                }
            }
        }

        public bool Equals(IObjectGraphVariableProvider other) {
            return other is ObjectGraphVariableProvider provider && this.Type.Equals(provider?.Type);
        }

        public override int GetHashCode() {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public void OnValidateVariables(ObjectGraphView graphView, ObjectGraphVariable[] variables) {
            validator?.Invoke(graphView, variables);
        }

        public VisualElement BuildField(ObjectGraphVariable variable) {
            var blackboardField = new BlackboardField(ObjectGraphVariableNode.ICON, variable.fullName, $"{variable.type.GetRealName()}")
            {
                capabilities = Capabilities.Selectable | Capabilities.Deletable | Capabilities.Droppable
            };
            blackboardField.userData = variable;
            blackboardField.AddToClassList(OBJECT_VARIABLE_FIELD_CLASS_NAME);
            blackboardField.Q<Image>("icon").tintColor = variable.containerType.GetColor();
            return blackboardField;
        }


        public static implicit operator ObjectGraphVariableProvider(Type type) => new ObjectGraphVariableProvider(type);
    }
    public class ObjectGraphVariableNode : TokenNode {
        private const string ICON_PATH = "Assets\\EditorResources\\Icons\\checkbox-blank-circle.png";
        public static readonly Texture ICON = AssetDatabase.LoadAssetAtPath<Texture>(ICON_PATH);
        public const string OBJECT_GRAPH_VARIABLE_CLASS_NAME = "object-graph-variable";
        public const string OBJECT_GRAPH_VARIABLE_ICON_CLASS_NAME = "object-graph-variable-icon";
        public ObjectGraphVariable Data;

        public string Id
        {
            get => viewDataKey;
            set => viewDataKey = value;
        }
        public ObjectGraphVariableNode(ObjectGraphVariable data) : this(data, default) {

        }
        public ObjectGraphVariableNode(ObjectGraphVariable data, string id = null, Rect position = default) : base(null, Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, data.type)) {
            viewDataKey = id == null ? Guid.NewGuid().ToString() : id;
            this.Data = data;
            AddToClassList(OBJECT_GRAPH_VARIABLE_CLASS_NAME);

            output.portName = null;
            output.portColor = data.type.GetColor();
            output.MakeObservable();
            output.RegisterCallback<PortChangedEvent>((evt) =>
            {
                var gv = (evt.currentTarget as VisualElement).GetFirstAncestorOfType<ObjectGraphView>();
                gv?.Model.SetVariableEntryTargets(viewDataKey, output.connections.Where((edge) => edge.input.node is ObjectGraphNode).Select((edge) => new ObjectGraphModel.VariableEntry.Target
                {
                    id = edge.input.node.viewDataKey,
                    field = edge.input.viewDataKey
                }));
                Debug.Log($"Edges: {evt.edges.Count()}, Filtered: {gv?.Model?.variableEntries[viewDataKey].targets.Count}");

            });
            this.RegisterCallback<AttachToPanelEvent>((evt) => InitConnections());
            icon = AssetDatabase.LoadAssetAtPath<Texture>(ICON_PATH);
            title = data.fullName;
            this.Q<Image>("icon").tintColor = data.containerType.GetColor();
            this.SetPosition(position);
        }
        public void InitConnections() {
            var gv = this.GetFirstAncestorOfType<ObjectGraphView>();
            var entry = gv.Model.GetVariableEntry(viewDataKey);
            foreach (var target in entry.targets) {
                if (!output.connections.Any((edge) => edge.input.viewDataKey == target.field && edge.input.node.viewDataKey == target.id)) {
                    var node = gv.GetNodeByGuid(target.id);
                    Debug.Log(node);
                    var fieldPort = node.Query<Port>(null).Where((port) => port.viewDataKey == target.field && port.direction == Direction.Input).First();
                    gv.AddElement(output.ConnectTo(fieldPort));
                }
            }
        }
    }
}