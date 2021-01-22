using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NeroWeNeed.UIDots {
    /* [UpdateInGroup(typeof(UIInitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public class UINodeDecompositionSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        private EntityQuery query;
        private EntityArchetype nodeArchetype;
        protected override unsafe void OnCreate() {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            nodeArchetype = EntityManager.CreateArchetype(
                typeof(UINodeInfo),
                typeof(UIParent),
                typeof(Parent),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(Rotation),
                typeof(Translation),
                typeof(Scale),
                typeof(LocalToParent),
                typeof(RenderBounds),
                typeof(WorldRenderBounds));
            query = GetEntityQuery(ComponentType.Exclude<UINode>(), ComponentType.ReadOnly<UIGraphData>(), ComponentType.ReadOnly<RenderMesh>());
            RequireForUpdate(query);
        }
        protected unsafe override void OnUpdate() {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            var graphs = query.ToComponentDataArray<UIGraphData>(Allocator.TempJob);
            var submeshCount = new NativeArray<int>(graphs.Length, Allocator.TempJob);
            var entities = query.ToEntityArray(Allocator.TempJob);
            var streamSize = 0;
            for (int i = 0; i < graphs.Length; i++) {
                streamSize += graphs[i].GetNodeCount();
            }
            var stream = new NativeStream(streamSize, Allocator.TempJob);
            new UINodeDecompositionJob
            {
                graphs = graphs,
                nodes = stream.AsWriter(),
                submeshCount = submeshCount
            }.Schedule(graphs.Length, 1).Complete();
            Job.WithCode(() =>
            {
                var result = stream.ToNativeArray<DedicatedNodeInfo>(Allocator.Temp);
                var nodes = new NativeMultiHashMap<int, DedicatedNodeInfo>(graphs.Length, Allocator.Temp);
                var nodeEntities = new NativeList<Entity>(Allocator.Temp);

                for (int i = 0; i < result.Length; i++) {
                    nodes.Add(result[i].graphIndex, result[i]);
                }
                for (int i = 0; i < graphs.Length; i++) {
                    var entity = entities[i];
                    nodeEntities.Clear();
                    var nodeInfoIter = nodes.GetValuesForKey(i);
                    var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                    renderMesh.mesh.subMeshCount = submeshCount[i] + 1;
                    while (nodeInfoIter.MoveNext()) {
                        var nodeInfo = nodeInfoIter.Current;
                        var nodeEntity = ecb.CreateEntity(nodeArchetype);
#if UNITY_EDITOR
                        var name = graphs[i].GetNodeName(nodeInfo.nodeIndex) ?? $"Node {nodeInfo.nodeIndex}";
                        EntityManager.SetName(nodeEntity, $"{EntityManager.GetName(entity)}[{name}]");
#endif
                        ecb.SetComponent(nodeEntity, new UINodeInfo { index = nodeInfo.nodeIndex, submesh = nodeInfo.submesh });
                        ecb.SetComponent(nodeEntity, new UIParent { value = entity });
                        ecb.SetComponent(nodeEntity, new Parent { Value = entity });
                        ecb.SetComponent(nodeEntity, new LocalToWorld { Value = float4x4.identity });
                        ecb.SetComponent(nodeEntity, new Rotation { Value = quaternion.identity });
                        ecb.SetComponent(nodeEntity, new Scale { Value = 1f });
                        ecb.SetComponent(nodeEntity, new LocalToParent { Value = float4x4.identity });
                        ecb.SetSharedComponent(nodeEntity, new RenderMesh
                        {
                            mesh = renderMesh.mesh,
                            material = renderMesh.material,
                            subMesh = nodeInfo.submesh,
                            castShadows = ShadowCastingMode.Off,
                            receiveShadows = false,
                            needMotionVectorPass = false,
                            layer = renderMesh.layer
                        });
                        ecb.SetComponent(nodeEntity, new RenderBounds { Value = renderMesh.mesh.GetSubMesh(nodeInfo.submesh).bounds.ToAABB() });
                        nodeEntities.Add(nodeEntity);
                    }
                    var buffer = ecb.AddBuffer<UINode>(entity);
                    buffer.AddRange(nodeEntities.AsArray().Reinterpret<UINode>());
                    if (nodeEntities.Length > 0) {
                        var children = ecb.AddBuffer<Child>(entity);
                        children.AddRange(nodeEntities.AsArray().Reinterpret<Child>());
                        var entityGroup = ecb.AddBuffer<LinkedEntityGroup>(entity);
                        entityGroup.Add(entity);
                        entityGroup.AddRange(nodeEntities.AsArray().Reinterpret<LinkedEntityGroup>());
                    }
                }
            }).WithoutBurst().Run();
            graphs.Dispose();
            entities.Dispose();
            submeshCount.Dispose();
            stream.Dispose();
        }
    } */
}