using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Core.Battle;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {

    public class TargetFilterGraphModule : BaseObjectGraphNodeModule, IObjectGraphPostSerializerCallback {
        public static readonly Type[] SuperTypes = { typeof(ITargetFilter<MapBodyTarget>), typeof(ITargetFilter<MapBodyDirection>), typeof(ITargetFilter<MapBodyTarget>) };
        public const string PortClassName = "target-filter-graph-node-port";

        public override string NodeClassName { get; } = "target-filter";

        public TargetFilterGraphModule() : this(new Settings
        {
            portName = "Target Filter",
            portType = typeof(ITargetFilter),
            portColor = new Color(0.851f, 0.051f, 1f),
            portClassName = PortClassName,
            superTypes = SuperTypes,
            serializer = new UnityObjectGraphSequenceSerializer
            {
                dataPropertyPath = "filter",
                variablePropertyPath = "variables"

            }
        }) { }
        public TargetFilterGraphModule(Settings settings) : base(settings) { }
        public void OnPostSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
            var property = obj.FindProperty("type");
            var targetType = payload.graphView.GetRoots<ObjectGraphNode>()?.FirstOrDefault()?.Type;
            if (typeof(ITargetFilter<>).IsAssignableFrom(targetType)) {
                property.enumValueIndex = (int)TargetTypeUtility.GetType(targetType.GenericTypeArguments[0]);
            }
            else {
                property.enumValueIndex = 0;
            }
        }
    }
}


