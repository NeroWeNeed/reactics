using System;
using Reactics.UI;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reactics.UI
{
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIMeshSystemGroup))]
    [UpdateBefore(typeof(LTSTransformSystem))]
    public class UIToLTSSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Entities.WithChangeFilter<UIElementBounds, UIElement>().WithAll<UIElement>().ForEach((ref LocalToScreen lts, in UIElementBounds bounds) =>
             {
                 lts.location = new float2(bounds.left, -bounds.top);
             }).Schedule();
        }
    }
}