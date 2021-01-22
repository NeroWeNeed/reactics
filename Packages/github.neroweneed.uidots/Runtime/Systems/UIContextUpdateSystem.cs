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
        protected override void OnCreate() {
            base.OnCreate();
        }
        protected override void OnUpdate() {
            
            Entities.WithAll<UICameraLayer>().ForEach((Camera camera, in Parent parent) =>
            {
                var parentCamera = EntityManager.GetComponentObject<Camera>(parent.Value);
                camera.orthographicSize = parentCamera.pixelHeight / 2f;
                //camera.orthographicSize = Screen.height / 2f;
            }).WithoutBurst().Run();
            Entities.ForEach((ref UICameraContext cameraContext, ref UIContext context, in UIContextSource source) =>
            {
                var camera = EntityManager.GetComponentObject<Camera>(source.value);
                context.dpi = Screen.dpi;
                context.pixelScale = source.value == Entity.Null ? 0.001f : 0.01f;
                context.size = new float2(camera.orthographicSize * camera.aspect * 2, camera.orthographicSize * 2);
                cameraContext.cameraLTW = camera.transform.localToWorldMatrix;
                cameraContext.clipPlane.x = camera.nearClipPlane;
                cameraContext.clipPlane.y = camera.farClipPlane;
            }).WithoutBurst().Run();
        }
    }
}