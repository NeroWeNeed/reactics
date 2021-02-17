using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    //TODO: Camera Layer Sizes don't match
    [UpdateInGroup(typeof(UISystemGroup), OrderLast = true)]
    public class UILTCTransformSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((Entity entity, ref LocalToWorld ltw, in LocalToCamera ltc, in UIContextData context, in RenderBounds renderBounds) =>
            {
                var rotation = quaternion.LookRotation(ltc.cameraLTW.c2.xyz, ltc.cameraLTW.c1.xyz);
                var translate = ltc.cameraLTW.c3.xyz + new float3(ltc.alignment.GetOffset(renderBounds.Value.Size.xy, context.size), 0) + math.mul(rotation, math.forward() * ltc.clipPlane.x * 2f) + (math.mul(rotation,math.right())*ltc.offsetX.Normalize(context))+(math.mul(rotation, math.up()) * ltc.offsetY.Normalize(context));
                ltw.Value = float4x4.TRS(translate, rotation, context.pixelScale);

            }).Schedule();

        }
    }
}