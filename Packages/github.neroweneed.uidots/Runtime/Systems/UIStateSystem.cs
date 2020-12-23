using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace NeroWeNeed.UIDots {
    /*     [DisableAutoCreation]
        //[WorldSystemFilter(WorldSystemFilterFlags.All)]
        [UpdateInGroup(typeof(SimulationSystemGroup))]
        public class UIDirtyStateSystem : SystemBase {
            //private EntityCommandBufferSystem entityCommandBufferSystem;
            private EntityQuery query;
            protected override void OnCreate() {
                //  entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
                query = GetEntityQuery(ComponentType.ReadOnly<UIRoot>(), ComponentType.ReadOnly<UINode>(), ComponentType.ReadWrite<UIDirtyState>(), ComponentType.ReadWrite<UIByteData>());
            }
            protected unsafe override void OnUpdate() {
                var updateBytesJob = new UpdateUIConfigurationJob
                {
                    entityHandle = GetEntityTypeHandle(),
                    rootHandle = GetComponentTypeHandle<UIRoot>(true),
                    byteAccessor = GetBufferFromEntity<UIByteData>(false),
                    nodeHandle = GetBufferTypeHandle<UINode>(true),
                    configHandle = GetComponentDataFromEntity<UIConfiguration>(true),
                    dirtyHandle = GetComponentTypeHandle<UIDirtyState>()
                }.Schedule(query, this.Dependency);
                updateBytesJob.Complete();
            }
        } */



    /* [WorldSystemFilter(WorldSystemFilterFlags.All)]
    [UpdateInGroup(typeof(SimulationSystemGroup))] */
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIContextUpdateSystem))]
    public class UIStateSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        private EntityQuery query;
        public List<Mesh> meshes;
        protected override void OnCreate() {
            entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(ComponentType.ReadOnly<UIRoot>(), ComponentType.ReadOnly<UINode>(), ComponentType.ReadWrite<UIDirtyState>());
            query.SetSharedComponentFilter<UIDirtyState>(true);
            this.RequireForUpdate(query);
            this.meshes = new List<Mesh>();

        }
        protected unsafe override void OnUpdate() {
            //TODO: Convert to switch table generation method
            this.meshes.Clear();
            var entities = new NativeList<Entity>(8, Allocator.TempJob);
            var contexts = new NativeList<UIContext>(8, Allocator.TempJob);
            var referencedGraphs = new NativeList<BlobAssetReference<UIGraph>>(8, Allocator.TempJob);
            var entityUpdate = new NativeMultiHashMap<int, ValueTuple<Entity, int>>(8, Allocator.Temp);
            Entities.WithSharedComponentFilter<UIDirtyState>(true).ForEach((Entity entity, DynamicBuffer<UINode> nodes, in UIRoot root, in UIContext context, in RenderMesh renderMesh) =>
            {
                int index = meshes.IndexOf(renderMesh.mesh);
                if (index < 0) {
                    index = meshes.Count;
                    meshes.Add(renderMesh.mesh);
                    entities.Add(entity);
                    referencedGraphs.Add(root.graph);
                    contexts.Add(context);
                }
                entityUpdate.Add(index, ValueTuple.Create(entity, 0));
                foreach (var node in nodes) {
                    entityUpdate.Add(index, ValueTuple.Create(node.value, GetComponent<UINodeInfo>(node.value).submesh));
                }
            }).WithoutBurst().Run();
            this.CompleteDependency();
            if (entities.Length > 0) {
                var ecb = entityCommandBufferSystem.CreateCommandBuffer();
                var meshData = Mesh.AllocateWritableMeshData(entities.Length);
                var layoutJob = new UILayoutJob
                {
                    meshDataArray = meshData,
                    graphs = referencedGraphs.AsArray(),
                    contexts = contexts.AsArray()
                };
                var layoutHandle = layoutJob.Schedule(entities.Length, 1);
                layoutHandle.Complete();
                Job.WithCode(() =>
                {
                    for (int i = 0; i < entities.Length; i++) {
                        ecb.SetSharedComponent<UIDirtyState>(entities[i], false);
                    }
                    Mesh.ApplyAndDisposeWritableMeshData(meshData, this.meshes);
                    //TODO: Push Bound calculation to end of layout job.
                    for (int i = 0; i < this.meshes.Count; i++) {
                        var m = this.meshes[i];
                        m.RecalculateBounds();
                        var iter = entityUpdate.GetValuesForKey(i);

                        while (iter.MoveNext()) {
                            ecb.SetComponent(iter.Current.Item1, new RenderBounds { Value = m.GetSubMesh(iter.Current.Item2).bounds.ToAABB() });
                        }
                    }
                }).WithoutBurst().Run();
                this.CompleteDependency();
                entityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
            }
            contexts.Dispose();
            referencedGraphs.Dispose();
            entities.Dispose();



        }
    }
}