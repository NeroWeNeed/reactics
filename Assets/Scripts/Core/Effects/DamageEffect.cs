using System;
using NeroWeNeed.BehaviourGraph;

namespace Reactics.Core.Effects {
    [Serializable]
    public struct DamageEffect {
        public long value;

        public ElementalAttribute elementalAttributes;
        public PhysicalAttribute physicalAttributes;
    }
    [Serializable]
    public struct TrackingEffect {
        public NodeIndex onCaught;


    }
}