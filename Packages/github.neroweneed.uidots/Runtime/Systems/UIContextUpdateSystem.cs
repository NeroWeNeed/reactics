using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UISystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public class UIContextUpdateSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate() {
            base.OnCreate();

        }
        protected override void OnUpdate() {
            Entities.WithAll<WindowCamera>().ForEach((Camera camera) =>
            {
                camera.orthographicSize = Screen.currentResolution.height / 2f;
            }).WithoutBurst().Run();
            Entities.ForEach((Entity entity, ref LocalToCamera cameraContext, ref UIContextData context, in UIContextSource source, in UIPixelScale scale) =>
            {
                var camera = EntityManager.GetComponentObject<Camera>(source.value);
                context.dpi = Screen.dpi;
                context.pixelScale = scale.value;
                context.size = new float2(Screen.currentResolution.height * camera.aspect, Screen.currentResolution.height);
                var old = cameraContext;
                cameraContext.cameraLTW = camera.transform.localToWorldMatrix;
                cameraContext.clipPlane.x = camera.nearClipPlane;
                cameraContext.clipPlane.y = camera.farClipPlane;
            }).WithoutBurst().Run();
        }
    }
}