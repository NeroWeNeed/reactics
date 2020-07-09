using System;
using System.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using UnityEngine;

namespace Reactics.Editor.Graph
{
    public class EffectGraphNode : ObjectGraphNode
    {


        public EffectGraphNode(object source, Guid guid) : base(source, guid)
        {
        }

        public EffectGraphNode(Type type, Guid guid) : base(type, guid)
        {
        }

        protected override Color GetPortColor(Type type) => EffectGraphModule.GetPortColor(type);

        protected override Type GetPortType(Type type) => EffectGraphModule.GetPortType(type);

    }
}