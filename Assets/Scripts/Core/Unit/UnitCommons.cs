namespace Reactics.Core.Unit {
    /// <summary>
    /// Represents a Unit's proficiency in some skill. Unit Proficiencies can be modified under the following conditions:
    /// 
    /// 1. Unit's must already have a proficiency in that skill. Unit's can not have proficiencies added to them.
    /// 2. The Unit's Proficiency Level must not be an F or FIXED. F Proficiencies serve as inadequacies and are used to apply negative traits to units for balancing. FIXED Levels are there for skills that don't have any skill curve associated with them.
    /// 
    /// </summary>
    public struct Proficiency {

        public string identifier;
        public ProficiencyLevel level;
        public bool IsProgressable() {
            return (int)level > 0;
        }
    }
    public enum ProficiencyLevel {
        FIXED = -1, F = 0, D = 1, C = 2, B = 3, A = 4, S = 5
    }
}