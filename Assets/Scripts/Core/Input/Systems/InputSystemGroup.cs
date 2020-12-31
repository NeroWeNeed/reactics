using Unity.Entities;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    /// <summary>
    /// System Group for storing Systems for determining player input. The Processing of player inputs do not go here, but go in the Player Input Processing Group. 
    /// 
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public class InputSystemGroup : ComponentSystemGroup { }
    [UpdateInGroup(typeof(InputSystemGroup))]
    public class InputUpdateSystemGroup : ComponentSystemGroup {
        private Controls controls = new Controls();
        public Controls Controls { get => controls; }
        protected override void OnStopRunning() {
            base.OnStopRunning();
            controls.Disable();
        }
        protected override void OnStartRunning() {
            base.OnStartRunning();
            controls.Enable();
        }

    }
}