using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {

    public class ObjectGraphVariable : Pill {

        public const string OBJECT_GRAPH_VARIABLE_CLASS_NAME = "object-graph-variable";
        public const string OBJECT_GRAPH_VARIABLE_ICON_CLASS_NAME = "object-graph-variable-icon";
        public const string ICON_PATH = "Assets\\EditorResources\\Icons\\checkbox-blank-circle.png";
        public Guid sourceTypeGuid;
        public string variableName;
        public Type variableType;
        public int offset;
        public long length;

        public ObjectGraphVariable(Type container, FieldInfo field) {
            sourceTypeGuid = container.GUID;
            variableName = field.Name;
            variableType = field.FieldType;
            AddToClassList(OBJECT_GRAPH_VARIABLE_CLASS_NAME);
            offset = Marshal.OffsetOf(field.DeclaringType, field.Name).ToInt32();
            length = Marshal.SizeOf(variableType);
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, variableType);
            port.portName = $"{container.Name}.{variableName}";
            port.portColor = TypeCommons.GetColor(variableType);
            icon = AssetDatabase.LoadAssetAtPath<Texture>(ICON_PATH);
            this.Q<Image>("icon").tintColor = TypeCommons.GetColor(container);
            right = port;
        }

    }
}