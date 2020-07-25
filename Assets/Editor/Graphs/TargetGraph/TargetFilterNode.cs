using System;
using Reactics.Battle;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {

    public class TargetFilterGraphNode : ObjectGraphNode {
        public TargetFilterGraphNode(object source, string guid) : base(source, guid) {
        }

        public TargetFilterGraphNode(Type type, string guid) : base(type, guid) {
        }

        public TargetFilterGraphNode(ObjectGraphModel.Entry entry, string guid) : base(entry, guid) {
        }

        public TargetFilterGraphNode(string guid) : base(guid) {
        }

        public TargetFilterGraphNode() : base() {
        }

        public override Edge ConnectToMaster(Port port, Node master) {
            var target = master.Q<Port>(null, TargetFilterGraphModule.PortClassName);
            if (target != null)
                return port.ConnectTo(target);
            else
                return null;
        }

        protected override Color GetPortColor(Type type) {
            if (ObjectGraphModuleUtility.TryGetTargetType(typeof(ITargetFilter<>), type, TargetFilterGraphModule.SuperTypes, out Type r)) {
                return ObjectGraphNodePort.GetColor(r);
            }
            else {
                return Color.black;
            }
        }

        public override Type GetSuperTargetType(Type type) => ObjectGraphModuleUtility.GetTargetType(typeof(ITargetFilter<>), type, TargetFilterGraphModule.SuperTypes);
    }
}