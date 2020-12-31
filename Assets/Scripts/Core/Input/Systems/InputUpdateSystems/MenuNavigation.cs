using NeroWeNeed.UIDots;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputUpdateSystemGroup))]

    public class MenuInputUpdateSystem : SystemBase, Controls.IMenuActions {
        private InputUpdateSystemGroup inputUpdateSystem;
        private MenuInputData inputData;
        public void OnBack(InputAction.CallbackContext context) {
            inputData.back = InputValue<bool>.FromButton(context);
        }

        public void OnDirectionalNavigation(InputAction.CallbackContext context) {
            inputData.directionalNavigation = InputValue<float2>.FromContextVector2(context);
        }

        public void OnPointerNavigation(InputAction.CallbackContext context) {
            inputData.pointerNavigation = InputValue<float2>.FromContextVector2(context);
        }

        public void OnSelect(InputAction.CallbackContext context) {
            inputData.select = InputValue<bool>.FromButton(context);
        }
        private EntityQuery menuControlsQuery;
        protected override void OnCreate() {
            base.OnCreate();
            inputUpdateSystem = World.GetOrCreateSystem<InputUpdateSystemGroup>();
            inputUpdateSystem.Controls.Menu.SetCallbacks(this);
            
            menuControlsQuery = GetEntityQuery(typeof(MenuInputData));
            RequireForUpdate(menuControlsQuery);
        }
        protected override void OnStartRunning() {
            inputData = default;
        }
        protected override void OnStopRunning() {
            inputData = default;
        }
        protected override void OnUpdate() {
            Entities.ForEach((ref MenuInputData menuControlsData) => menuControlsData = inputData).WithoutBurst().Run();
            inputData.Update(Time);
        }
    }
}