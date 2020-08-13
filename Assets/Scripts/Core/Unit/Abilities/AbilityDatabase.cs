using System;

namespace Reactics.Core.Battle.Abilities {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Ability : Attribute {
        public readonly string identifier;
        public Ability(string identifier) {
            this.identifier = identifier;
        }
    }
}