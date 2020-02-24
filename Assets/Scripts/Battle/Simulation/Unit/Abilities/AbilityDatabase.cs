using System;

namespace Reactics.Battle.Abilities
{

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Ability : Attribute
    {
        public readonly string identifier;
        public Ability(string identifier)
        {
            this.identifier = identifier;
        }
    }
}