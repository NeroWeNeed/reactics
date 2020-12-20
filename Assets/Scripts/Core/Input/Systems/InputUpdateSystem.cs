using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(InputHandlingSystemGroup), OrderFirst = true)]
    public class InputUpdateSystem : SystemBase {
        private Controls controls = new Controls();
        public Controls Controls { get => controls; }

        protected override void OnCreate() {
            base.OnCreate();
            controls.Enable();
            Debug.Log("x");
        }
        protected override void OnUpdate() {
            InputSystem.Update();
            Entities.ForEach((Entity entity, in InputHandlerData handler, in InputControlSchemeData controlSchemeData, in InputActionMapData actionMapData) =>
                    {

                        var input = handler.PlayerInput;
                        var controlSchemeDirty = controlSchemeData.name != input?.currentControlScheme;

                                                var actionMapDirty = actionMapData.name != (input?.currentActionMap?.name);
                                                InputHandlerState state = InputHandlerState.Clean;
                                                if (controlSchemeDirty) {
                                                    state |= InputHandlerState.ControlSchemeDirty;
                                                    EntityManager.SetSharedComponentData(entity, new InputControlSchemeData { name = input?.currentControlScheme });
                                                }
                                                if (actionMapDirty) {
                                                    state |= InputHandlerState.ActionMapDirty;
                                                    EntityManager.SetSharedComponentData(entity, new InputActionMapData { name = input?.currentActionMap?.id.ToString() });
                                                }
                                                if (state != InputHandlerState.Clean) {
                                                    EntityManager.SetComponentData(entity, new InputHandlerStateData { value = state });
                                                }
                    }).WithoutBurst().WithStructuralChanges().Run();


        }
        protected override void OnStartRunning() {
            base.OnStartRunning();
            controls.Enable();
        }
        protected override void OnStopRunning() {
            base.OnStopRunning();
            controls.Disable();
        }

    }
}