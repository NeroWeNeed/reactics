using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {

    public class TargetFilterGraphModule : BaseObjectGraphNodeModule<TargetFilterGraphNode> {

        static TargetFilterGraphModule() {
            ObjectGraphNodePort.RegisterColor(typeof(ITargetFilter<Point>), Color.blue);
            ObjectGraphNodePort.RegisterColor(typeof(ITargetFilter<MapBodyDirection>), Color.green);
            ObjectGraphNodePort.RegisterColor(typeof(ITargetFilter<MapBodyTarget>), Color.red);
        }
        public static readonly Type[] SuperTypes = { typeof(ITargetFilter<MapBodyTarget>), typeof(ITargetFilter<MapBodyDirection>), typeof(ITargetFilter<MapBodyTarget>) };
        public const string PortClassName = "target-filter-graph-node-port";
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
    }
}


