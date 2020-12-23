using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputHandlingSystemGroup))]

    [UpdateAfter(typeof(InputUpdateSystem))]

    public abstract class BaseInputHandlingSystem : SystemBase {
        protected InputActionMap inputActionMap;
        protected InputControlScheme inputControlScheme;
        private InputContext inputContext;
        public InputContext InputContext { get => inputContext; }
        private EntityQuery query;
        public EntityQuery Query { get => query; }
        private InputUpdateSystem inputUpdateSystem;
        public InputUpdateSystem InputUpdateSystem { get => inputUpdateSystem; }

        protected override void OnCreate() {
            inputUpdateSystem = World.GetOrCreateSystem<InputUpdateSystem>();
            inputControlScheme = GetControlScheme(inputUpdateSystem.Controls);
            inputActionMap = GetActionMap(inputUpdateSystem.Controls);
            inputContext = new InputContext(inputActionMap, inputControlScheme);
    
            //query.SetSharedComponentFilter<InputData2>(new InputActionMapData { name = inputActionMap.id.ToString() });
            
        }
        protected abstract InputActionMap GetActionMap(Controls controls);
        protected abstract InputControlScheme GetControlScheme(Controls controls);
    }
}