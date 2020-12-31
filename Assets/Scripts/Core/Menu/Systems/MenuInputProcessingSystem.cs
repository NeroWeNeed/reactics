using NeroWeNeed.UIDots;
using Reactics.Core.Input;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.Menu {
    public class MenuInputProcessingSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((ref UICursorInput cursorInputData, in MenuInputData menuInputData) =>
            {
                if (menuInputData.directionalNavigation.duration == 0 && !menuInputData.directionalNavigation.value.Equals(default)) {
                    cursorInputData = new UICursorInput(math.normalize(menuInputData.directionalNavigation.value), new float2(0, 1));
                }
                else {
                    cursorInputData.direction = float2.zero;
                }
            }).WithoutBurst().Run();
        }
    }
}