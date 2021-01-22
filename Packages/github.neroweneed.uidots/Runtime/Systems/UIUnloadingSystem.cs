using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UIInitializationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class UIUnloadingSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate() {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }
        protected unsafe override void OnUpdate() {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            Entities.WithNone<UIGraph>().ForEach((Entity entity, in UIGraphData graph) =>
            {
                if (graph.IsCreated) {
                    UnsafeUtility.Free(graph.value.ToPointer(), Allocator.Persistent);
                }
                ecb.RemoveComponent<UIGraphData>(entity);
            }).Schedule();
            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
        protected unsafe override void OnDestroy() {
            Entities.WithNone<UIGraph>().ForEach((Entity entity, in UIGraphData graph) =>
            {
                if (graph.IsCreated) {
                    UnsafeUtility.Free(graph.value.ToPointer(), Allocator.Persistent);
                }
                EntityManager.RemoveComponent<UIGraphData>(entity);
            }).WithStructuralChanges().WithoutBurst().Run();
        }
    }
}