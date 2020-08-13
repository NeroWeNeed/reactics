using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.UI {

    [UpdateInGroup(typeof(UISizeProviderSystemGroup))]
    public class UIRectangleSizeProvider : SystemBase {
        protected override void OnCreate() {

        }
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIRectangle>().ForEach((ref UISize size, in UIRectangle rect) =>
            {
                size.Width = rect.width;
                size.Height = rect.height;
            }).Schedule(Dependency).Complete();
        }
    }


}