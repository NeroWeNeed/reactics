using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using UnityEngine;
namespace Reactics.Core.AssetDefinitions {
    [Serializable]
    public struct WeaponData {
        public ushort id;
        public ushort baseMagicalDamage;
        public ushort basePhysicalDamage;
        public ElementalAttribute elementalAttributes;
        public PhysicalAttribute physicalAttributes;
        public ushort maxAmmo;
        public DataReference<PassiveSkillEffectData> effect;
    }
}