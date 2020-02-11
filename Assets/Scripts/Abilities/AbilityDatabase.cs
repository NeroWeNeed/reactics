using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;

namespace Reactics.Battle.Abilities
{

    public class AbilityDatabase
    {

    }

    public class AbilityConfigurator
    {
        public delegate void ConfiguratorDelegate(ref EntityCommandBuffer entityCommandBuffer,ref Entity entity);
        public readonly string Identifier;
        public readonly long Id;

        private ConfiguratorDelegate configureAction;

        public AbilityConfigurator(string identifier, long id, ConfiguratorDelegate configureAction)
        {

            Identifier = identifier;
            Id = id;
            this.configureAction = configureAction;
        }

        public void Configure(ref EntityCommandBuffer entityCommandBuffer,ref Entity entity) {
            configureAction.Invoke(ref entityCommandBuffer,ref entity);
        }
    }
    public sealed class AbilityProviderFactory : System.Attribute { }

    public sealed class AbilityProvider : Attribute
    {
        public readonly string identifier;

        public AbilityProvider(string identifier)
        {
            this.identifier = identifier;
        }
    }
    public struct Ability : IComponentData
    {
        public Entity unitEntity;
    }


}