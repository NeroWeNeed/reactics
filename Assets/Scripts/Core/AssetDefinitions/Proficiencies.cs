using System;
using System.Collections.Generic;
using UnityEngine;
namespace Reactics.Core.AssetDefinitions {

    public struct ProficiencyData {
        public ushort id;
        public ProficiencyType type;
    }
    public struct ProficiencyDefinition {
        public ushort id;
        public ProficiencyLevel level;
    }
    public enum ProficiencyLevel : sbyte {
        //Represents a Proficiency that cannot be leveled up.
        Constant = -2,
        F = -1,
        None = 0,
        D = 1,
        C = 2,
        B = 3,
        A = 4,
        S = 5
    }
    public enum ProficiencyType {
        Constant, Upgradeable
    }
}