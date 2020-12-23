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
        }

        protected override void OnUpdate() {

            InputSystem.Update();
            Entities.ForEach((Entity entity, in InputHandlerData handler, in InputContext context) =>
                    {
                        var input = handler.PlayerInput;
                        var controlSchemeDirty = context.controlSchemeName != input?.currentControlScheme;
                        var actionMapDirty = context.actionMapName != (input?.currentActionMap?.name);
                        //InputHandlerState state = InputHandlerState.Clean;
                        var update = context.controlSchemeName != input?.currentControlScheme || context.actionMapName != (input?.currentActionMap?.name);
                        if (update) {
                            EntityManager.SetSharedComponentData(entity,new InputContext { actionMapName = input.currentActionMap.name, controlSchemeName = input.currentControlScheme });
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