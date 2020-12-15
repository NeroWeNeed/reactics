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
    public class UIStateSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        private EntityQuery query;
        public List<Mesh> meshes;
        public List<RenderMesh> renderMeshes;
        protected override void OnCreate() {
            entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(ComponentType.ReadOnly<UIRoot>(), ComponentType.ReadOnly<UINode>(), ComponentType.ReadWrite<UIDirtyState>());
            query.SetSharedComponentFilter<UIDirtyState>(true);
            this.RequireForUpdate(query);
            this.meshes = new List<Mesh>();
            this.renderMeshes = new List<RenderMesh>();

        }
        protected unsafe override void OnUpdate() {
            this.meshes.Clear();
            this.renderMeshes.Clear();
            var entities = new NativeList<Entity>(8, Allocator.TempJob);
            var referencedGraphs = new NativeList<BlobAssetReference<UIGraph>>(8, Allocator.TempJob);
            Entities.WithAll<UINode>().WithSharedComponentFilter<UIDirtyState>(true).ForEach((Entity entity, in UIRoot root, in UIDirtyState dirtyState, in RenderMesh renderMesh) =>
            {
                if (!meshes.Contains(renderMesh.mesh)) {
                    meshes.Add(renderMesh.mesh);
                    renderMeshes.Add(renderMesh);
                    entities.Add(entity);
                    referencedGraphs.Add(root.graph);
                }
            }).WithoutBurst().Run();
            this.CompleteDependency();
            if (entities.Length > 0) {
                var ecb = entityCommandBufferSystem.CreateCommandBuffer();

                var meshData = Mesh.AllocateWritableMeshData(entities.Length);
                var contexts = new NativeArray<UILengthContext>(entities.Length, Allocator.TempJob);

                var ctx = UILengthContext.CreateContext();

                UnsafeUtility.MemCpyReplicate(contexts.GetUnsafePtr(), UnsafeUtility.AddressOf(ref ctx), UnsafeUtility.SizeOf<UILengthContext>(), entities.Length);
                var graphs = new NativeArray<BlobAssetReference<UIGraph>>(entities.Length, Allocator.TempJob);
                graphs.CopyFrom(referencedGraphs);
                var layoutJob = new UILayoutJob
                {
                    meshDataArray = meshData,
                    graphs = graphs,
                    contexts = contexts
                };
                var layoutHandle = layoutJob.Schedule(entities.Length, 1);
                layoutHandle.Complete();


                Job.WithCode(() =>
                {
                    for (int i = 0; i < entities.Length; i++) {
                        ecb.SetSharedComponent<UIDirtyState>(entities[i], false);
                    }
                    Mesh.ApplyAndDisposeWritableMeshData(meshData, this.meshes);
                    for (int i = 0; i < this.meshes.Count; i++) {
                        var m = this.meshes[i];
                        m.RecalculateBounds();
                        ecb.SetComponent(entities[i], new RenderBounds { Value = m.bounds.ToAABB() });
                        /*                         var rm = renderMeshes[i];
                                                rm.mesh = m;
                                                ecb.SetSharedComponent(entities[i], rm); */
                    }
                }).WithoutBurst().Run();
                this.CompleteDependency();
                entityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
            }
            referencedGraphs.Dispose();
            entities.Dispose();



        }
    }
}