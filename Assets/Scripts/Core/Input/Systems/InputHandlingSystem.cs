using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputHandlingSystemGroup))]

    [UpdateAfter(typeof(InputUpdateSystem))]
    
    public abstract class BaseInputHandlingSystem : SystemBase {
        private EntityQuery query;
        public EntityQuery Query { get => query; }
        private InputUpdateSystem inputUpdateSystem;
        public InputUpdateSystem InputUpdateSystem { get => inputUpdateSystem; }

        protected override void OnCreate() {
            inputUpdateSystem = World.GetOrCreateSystem<InputUpdateSystem>();
            inputUpdateSystem.Controls.Enable();
            var scheme = GetControlScheme(inputUpdateSystem.Controls);
            var actionMap = GetActionMap(inputUpdateSystem.Controls);
            query = GetEntityQuery(typeof(InputHandlerData), typeof(InputControlSchemeData),typeof(InputActionMapData));
            query.SetSharedComponentFilter<InputControlSchemeData>(scheme.name);
            
            query.SetSharedComponentFilter<InputActionMapData>(new InputActionMapData { name = actionMap.id.ToString() });
            RequireForUpdate(query);
        }
        protected abstract InputActionMap GetActionMap(Controls controls);
        protected abstract InputControlScheme GetControlScheme(Controls controls);
    }
}