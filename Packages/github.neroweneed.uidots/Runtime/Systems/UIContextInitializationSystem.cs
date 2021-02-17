using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UIInitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public class UIContextInitializationSystem : SystemBase {
        //private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate() {
            base.OnCreate();
            //entityCommandBufferSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate() {
            //var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            Entities.WithNone<UIContextData, UIContextSource>().WithAll<UIContext>().ForEach((Entity entity, in UIPixelScale pixelScale) =>
             {
                 EntityManager.AddComponentData(entity, new UIContextData
                 {
                     dpi = Screen.dpi,
                     pixelScale = pixelScale.value,
                     size = new float2(float.PositiveInfinity, float.PositiveInfinity)
                 });
             }).WithoutBurst().WithStructuralChanges().Run();
            Entities.WithNone<UIContextData>().WithAll<UIContext>().ForEach((Entity entity, in UIContextSource source, in UIPixelScale pixelScale) =>
             {
                 var camera = EntityManager.GetComponentObject<Camera>(source.value);
                 EntityManager.AddComponentData(entity, new UIContextData
                 {
                     dpi = Screen.dpi,
                     pixelScale = pixelScale.value,
                     size = new float2(Screen.currentResolution.height *camera.aspect, Screen.currentResolution.height)
                 });
             }).WithoutBurst().WithStructuralChanges().Run();
            Entities.WithNone<UIContext>().WithAll<UIContextData>().ForEach((Entity entity) =>
            {
                EntityManager.RemoveComponent<UIContextData>(entity);
            }).WithoutBurst().WithStructuralChanges().Run();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Entities.WithNone<UIContext>().WithAll<UIContextData>().ForEach((Entity entity) =>
            {
                EntityManager.RemoveComponent<UIContextData>(entity);
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}