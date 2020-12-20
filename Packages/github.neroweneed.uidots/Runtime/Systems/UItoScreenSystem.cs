using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIContextUpdateSystem))]
    public class UIToScreenSystem : SystemBase {

        protected override void OnUpdate() {

            Entities.ForEach((Entity entity, ref LocalToWorld ltw, in UIScreenElement screen, in RenderBounds bounds,in UIContext context, in UICameraContext cameraContext) =>
            {
                 var rotation = new quaternion(cameraContext.cameraLTW);

                float3 cameraForward = math.mul(rotation, math.forward()); 
                math.transform(cameraContext.cameraLTW, float3.zero);
                 float3 cameraPosition = new float3(cameraContext.cameraLTW.c3.x, cameraContext.cameraLTW.c3.y, cameraContext.cameraLTW.c3.z);
                float3 cameraUp = math.mul(rotation, math.up());
                float3 cameraRight = math.mul(rotation, math.right()); 
                
                float elapsedTime = (float)Time.ElapsedTime;
                 float drawDistance = cameraContext.clipPlane.x;
                //float2 position = new float2(screen.x.Normalize(contextData), screen.y.Normalize(contextData)); 
                ltw.Value = float4x4.TRS(
                    cameraPosition + (cameraForward * (1 + drawDistance)) + GetTranslation( cameraUp, cameraRight, context.size, bounds.Value, screen.alignment),
                     rotation,
                     1);

            }).WithoutBurst().Run();

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetTranslation(float3 up, float3 right, float2 size, AABB bounds, Alignment alignment) {
            Debug.Log(size);
            var offset = alignment.GetOffset(new float2(bounds.Extents.x * 2, bounds.Extents.y * 2), size,Alignment.BottomLeft)/size;
            Debug.Log(offset);
            return (right * offset.x) + (up * offset.y);
        }

    }
}