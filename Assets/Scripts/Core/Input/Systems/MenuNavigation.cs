using NeroWeNeed.UIDots;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputHandlingSystemGroup))]

    [UpdateAfter(typeof(InputUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]

    public class MenuNavigationKeyboardAndMouse : BaseInputHandlingSystem {

        protected override InputActionMap GetActionMap(Controls controls) {
            return controls.MenuControls.Get();
        }

        protected override InputControlScheme GetControlScheme(Controls controls) {

            return controls.KeyboardMouseScheme;
        }

        protected override void OnUpdate() {

            Entities.WithSharedComponentFilter(InputContext).ForEach((ref UICursorInput input, ref UICursorDirty dirtyState, in UICursor cursor) =>
            {
                float2 dir = InputUpdateSystem.Controls.MenuControls.DirectionalNavigation.ReadValue<Vector2>();
                
                if (dir.Equals(float2.zero)) {
                    input.direction = float.NaN;
                }
                else {
                    Debug.Log(InputUpdateSystem.Controls.MenuControls.DirectionalNavigation.phase);
                    input.direction = math.atan2(dir.y, dir.x);
                }


            }).WithoutBurst().Run();



        }
    }
}