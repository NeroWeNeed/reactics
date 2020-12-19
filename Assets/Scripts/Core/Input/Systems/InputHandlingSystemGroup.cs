using Reactics.Core.Commons;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class InputHandlingSystemGroup : ComponentSystemGroup {

        public bool updateInputData;
        protected override void OnCreate() {
            base.OnCreate();
            RequireForUpdate(GetEntityQuery(typeof(InputHandlerData)));


        }

    }
}