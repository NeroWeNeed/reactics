using System.Collections.Generic;
using Reactics.Core.Commons;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.UI {

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UILayoutSystemGroup))]
    public class UIMeshBuilderSystem : SystemBase {
        private EntityQuery query;
        public static readonly VertexAttributeDescriptor[] descriptor = new VertexAttributeDescriptor[] {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3,0),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3,0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2,0)
        };

        protected override void OnCreate() {
            query = GetEntityQuery(ComponentType.Exclude<UIParent>(), ComponentType.ReadOnly<UILayoutVersion>(), ComponentType.ReadOnly<UIMeshVersion>(), ComponentType.ReadOnly<RenderMesh>());
            query.SetChangedVersionFilter(typeof(UIMeshVersion));

            RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            if (query.CalculateEntityCount() > 0) {
                Debug.Log("UPDATE 5");
                var targets = query.ToEntityArray(Allocator.TempJob);
                var indices = new NativeList<int>(Allocator.TempJob);
                var vertices = new NativeList<UIMeshVertexData>(Allocator.TempJob);
                var subMeshes = new NativeMultiHashMap<int, MeshGenerationSubMesh>(8, Allocator.TempJob);
                var meshes = new NativeList<MeshGenerationSlice>(Allocator.TempJob);
                Mesh[] outputMeshes = new Mesh[targets.Length];
                for (int i = 0; i < targets.Length; i++) {
                    outputMeshes[i] = EntityManager.GetSharedComponentData<RenderMesh>(targets[i]).mesh;
                    outputMeshes[i].Clear();
                    Debug.Log(outputMeshes[i].name);
                }
                var renderJobHandle = new RenderJob
                {
                    vertexData = GetBufferFromEntity<UIMeshVertexData>(true),
                    indexData = GetBufferFromEntity<UIMeshIndexData>(true),
                    childData = GetBufferFromEntity<UIChild>(true),
                    entityHandle = GetEntityTypeHandle(),
                    indices = indices,
                    vertices = vertices,
                    subMeshes = subMeshes,
                    meshes = meshes
                }.Schedule(query);
                renderJobHandle.Complete();
                var meshJob = MeshGenerationJob<UIMeshVertexData>.Create(indices, vertices, subMeshes, meshes, descriptor);
                var meshJobHandle = meshJob.Schedule(renderJobHandle);
                meshJobHandle.Complete();
                Mesh.ApplyAndDisposeWritableMeshData(meshJob.meshes, outputMeshes);
                for (int i = 0; i < outputMeshes.Length; i++) {
                    outputMeshes[i].RecalculateBounds();
                }
                indices.Dispose();
                vertices.Dispose();
                subMeshes.Dispose();
                meshes.Dispose();
                targets.Dispose();
            }
        }
        [BurstCompile]
        public struct RenderJob : IJobChunk {
            [ReadOnly]
            public BufferFromEntity<UIMeshVertexData> vertexData;
            [ReadOnly]
            public BufferFromEntity<UIMeshIndexData> indexData;
            [ReadOnly]
            public BufferFromEntity<UIChild> childData;
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            [NativeDisableParallelForRestriction]
            public NativeMultiHashMap<int, MeshGenerationSubMesh> subMeshes;
            [NativeDisableParallelForRestriction]
            public NativeList<int> indices;
            [NativeDisableParallelForRestriction]
            public NativeList<UIMeshVertexData> vertices;
            [NativeDisableParallelForRestriction]
            public NativeList<MeshGenerationSlice> meshes;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {


                var entities = chunk.GetNativeArray(entityHandle);
                int currentSubmesh;
                for (int i = 0; i < entities.Length; i++) {
                    currentSubmesh = 1;
                    var iStart = indices.Length;
                    var vStart = vertices.Length;
                    UpdateMesh(entities[i], i, ref currentSubmesh);
                    meshes.Add(new MeshGenerationSlice
                    {
                        startIndex = iStart,
                        startVertex = vStart,
                        indexLength = indices.Length - iStart,
                        vertexLength = vertices.Length - vStart
                    });
                }


            }
            public void UpdateMesh(Entity entity, int mesh, ref int submesh) {
                if (vertexData.HasComponent(entity) && indexData.HasComponent(entity)) {
                    var v = vertexData[entity].AsNativeArray();
                    var i = indexData[entity].AsNativeArray();
                    var ni = new NativeArray<int>(i.Length, Allocator.Temp);
                    var iStart = indices.Length;
                    var vStart = vertices.Length;
                    for (int j = 0; j < i.Length; j++) {
                        ni[j] = i[j].value + vStart;
                    }
                    indices.AddRange(ni);
                    vertices.AddRange(v);
                    subMeshes.Add(mesh, new MeshGenerationSubMesh
                    {
                        start = iStart,
                        index = submesh++,
                        length = ni.Length,
                        topology = MeshTopology.Triangles
                    });
                }
                if (childData.HasComponent(entity)) {
                    var children = childData[entity];
                    for (int i = 0; i < children.Length; i++) {
                        UpdateMesh(children[i], mesh, ref submesh);
                    }

                }
            }
        }

    }
}
