using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{

    public class EffectGraphModule : ObjectGraphModule<EffectGraphNode>
    {


        public static readonly Type[] Types = { typeof(Point), typeof(MapBodyDirection), typeof(MapBodyTarget) };
        static EffectGraphModule()
        {
            ObjectGraphNodePort.RegisterColor(typeof(Point), Color.cyan);
            ObjectGraphNodePort.RegisterColor(typeof(MapBodyDirection), Color.yellow);
            ObjectGraphNodePort.RegisterColor(typeof(MapBodyTarget), Color.magenta);
        }
        public static Color GetPortColor(Type type)
        {
            if (typeof(IEffect).IsAssignableFrom(type))
                return GetPortColor(type.GetInterfaces().First((x) => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IEffect<>))).GetGenericArguments()[0]);
            return ObjectGraphNodePort.GetColor(type);
        }
        public static bool TryGetPortColor(Type type, out Color color)
        {
            if (typeof(IEffect).IsAssignableFrom(type))
                return TryGetPortColor(type.GetInterfaces().First((x) => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IEffect<>))).GetGenericArguments()[0], out color);
            return ObjectGraphNodePort.TryGetColor(type, out color);
        }
        public static Type GetPortType(Type type)
        {
            if (typeof(IEffect).IsAssignableFrom(type))
                return GetPortType(type.GetInterfaces().First((x) => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IEffect<>))).GetGenericArguments()[0]);
            var index = Array.IndexOf(EffectGraphModule.Types, type);
            if (index != -1)
            {
                return Types[index];
            }
            else
                throw new ArgumentException($"Unsupported Type", "type");
        }
        public const string PortClassName = "effect-graph-node-port";
        private List<Type> validTypes;

        public ReadOnlyCollection<Type> ValidTypes { get; private set; }

        public EffectGraphModule()
        {
            validTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (IsValidType(type))
                        validTypes.Add(type);
            ValidTypes = validTypes.AsReadOnly();

        }
        public static bool IsValidType(Type type)
        {
            return type.IsUnmanaged() && typeof(IEffect).IsAssignableFrom(type) && !typeof(IUtilityEffect).IsAssignableFrom(type);
        }

        public override void ConfigureMaster(Node master)
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IEffect));
            port.portName = "Effect";
            master.inputContainer.Add(port);
        }


        public override EffectGraphNode CreateNode(Type type, Rect layout)
        {
            var node = new EffectGraphNode(type: type, guid: Guid.NewGuid());
            node.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                node.SetPosition(layout);
            });
            return node;
        }

        public override ObjectGraphNodeSet<EffectGraphNode> CollectNodes(Node master)
        {
            throw new NotImplementedException();
        }
    }
}