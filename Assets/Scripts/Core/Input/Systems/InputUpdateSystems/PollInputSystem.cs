using Unity.Entities;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputUpdateSystemGroup), OrderFirst = true)]
    public class PollInputSystem : SystemBase {
        protected override void OnUpdate() {
            
            InputSystem.Update();
        }
    }
}