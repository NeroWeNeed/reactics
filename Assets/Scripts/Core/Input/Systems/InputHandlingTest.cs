using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputHandlingSystemGroup))]

    [UpdateAfter(typeof(InputUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    
    public class InputHandlingTest : BaseInputHandlingSystem {

        protected override InputActionMap GetActionMap(Controls controls) {
            return controls.MenuControls.Get();
        }

        protected override InputControlScheme GetControlScheme(Controls controls) {

            return controls.KeyboardMouseScheme;
        }

        protected override void OnUpdate() {
            if (!Query.IsEmpty) {
                
                if (InputUpdateSystem.Controls.MenuControls.Select.triggered) {
                    Debug.Log("TRIGGER");
                }
            }
        }
    }
}