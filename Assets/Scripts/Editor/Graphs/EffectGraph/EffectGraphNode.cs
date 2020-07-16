using System;
using System.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class EffectGraphNode : ObjectGraphNode {


        public EffectGraphNode(object source, string guid) : base(source, guid) {
        }
        public EffectGraphNode(Type type, string guid) : base(type, guid) {
        }
        public EffectGraphNode(ObjectGraphModel.Entry entry, string guid) : base(entry, guid) {
        }

        public EffectGraphNode(string guid) : base(guid) {
        }

        public EffectGraphNode() {
        }

        public override Edge ConnectToMaster(Port port, Node master) {
            var target = master.Q<Port>(null, EffectGraphModule.PortClassName);
            if (target != null)
                return port.ConnectTo(target);
            else
                return null;
        }

        protected override Color GetPortColor(Type type) {
            if (ObjectGraphModuleUtility.TryGetTargetType(typeof(IEffect<>), type, EffectGraphModule.SuperTypes, out Type r)) {
                return ObjectGraphNodePort.GetColor(r);
            }
            else {
                return Color.black;
            }
        }


        public override Type GetSuperTargetType(Type type) => ObjectGraphModuleUtility.GetTargetType(typeof(IEffect<>), type, EffectGraphModule.SuperTypes);

    }
}