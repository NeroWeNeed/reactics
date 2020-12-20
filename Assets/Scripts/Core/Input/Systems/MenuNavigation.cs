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
            if (!Query.IsEmpty) {
                Entities.ForEach((ref UICursorInput input,ref UICursorDirty dirtyState, in UICursor cursor) =>
                {
                    float2 dir = InputUpdateSystem.Controls.MenuControls.DirectionalNavigation.ReadValue<Vector2>();
                    if (math.length(dir) != 0) {

                        dirtyState.value = true;
                        input.direction = quaternion.LookRotation(new float3(dir.x, dir.y, 0), math.back());
                        
                        
                    }
                }).WithoutBurst().Run();

                
            }
        }
    }
}