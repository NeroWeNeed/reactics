using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reactics.UI
{
    public class UIMeshConfigurationSystem : ComponentSystem
    {
        private EntityQuery query;

        private Dictionary<TypeHash, IUIConfigurator> configurators;
        protected override void OnCreate()
        {
            query = GetEntityQuery(UIArchetypes.DirtyUIElement.GetComponentTypes());
            configurators = new Dictionary<TypeHash, IUIConfigurator>();
            RequireForUpdate(query);
        }
        protected override void OnUpdate()
        {

            Entities.With(query).ForEach((Entity entity, UIElement element) =>
            {
                if (EntityManager.HasComponent<UIClean>(entity))
                    return;
                element.configurator.Configure(entity, PostUpdateCommands, World);
                PostUpdateCommands.AddComponent<UIClean>(entity);
            });

        }
    }
}