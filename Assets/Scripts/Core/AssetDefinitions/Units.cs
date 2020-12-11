using System;
using Reactics.Core.Commons;
using Unity.Entities;

namespace Reactics.Core.AssetDefinitions {
    [Serializable]
    public struct UnitData {
        public ushort id;
        public uint maxHealthPoints;
        public uint maxManaPoints;
        public uint physicalStrength;
        public uint magicalStrength;
        public uint physicalDefense;
        public uint magicalDefense;
        public uint speed;
        public uint movement;
        public uint passiveSkillSlots;
        public BlobArray<ProficiencyDefinition> proficiencies;
        public BlobArray<DataReference<PassiveSkillEffectData>> inheritPassiveSkillIds;
        public DataReference<CommanderSkillEffectData> commanderSkillId;
    }
    [Serializable]
    public struct UnitDefinition {
        public ushort id;
        public BlobArray<DataReference<PassiveSkillEffectData>> passiveSkillIds;
        public BlobArray<DataReference<ActiveSkillEffectData>> activeSkillIds;
        public DataReference<StatSkillEffectData> statSkillId;
        /// <summary>
        /// Generally 1, but some characters can wield multiple weapons.
        /// </summary>
        public BlobArray<DataReference<WeaponData>> weaponIds;

    }
    [Serializable]
    public struct TeamDefinition {
        public BlobString name;
        public BlobArray<UnitDefinition> units;
        public byte commander;
    }
}