using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.UI {

    //[DisableAutoCreation]
    public class UIMeshBuilderSystem : SystemBase {
        private EntityQuery query;
        public static readonly VertexAttributeDescriptor[] descriptor = new VertexAttributeDescriptor[] {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3,0),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3,0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2,0)
        };
        private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate() {
            query = GetEntityQuery(ComponentType.Exclude<UIParent>(), ComponentType.ReadOnly<UIElement>());
            query.SetChangedVersionFilter(ComponentType.ReadOnly<UIElement>());
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            //RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            var meshData = Mesh.AllocateWritableMeshData(query.CalculateEntityCount());
            new RenderJob
            {
                vertexData = GetBufferFromEntity<UIMeshData>(true),
                childData = GetBufferFromEntity<UIChild>(true),
                entityHandle = GetEntityTypeHandle(),
                meshDataArray = meshData,
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.Schedule(query).Complete();
        }
        [BurstCompile]
        public struct RenderJob : IJobChunk {
            [ReadOnly]
            public BufferFromEntity<UIMeshData> vertexData;
            [ReadOnly]
            public BufferFromEntity<UIChild> childData;
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            public Mesh.MeshDataArray meshDataArray;

            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {


                var entities = chunk.GetNativeArray(entityHandle);
                int currentSubmesh;
                NativeList<UIMeshData> meshDataBuffer = new NativeList<UIMeshData>(Allocator.Temp);
                NativeList<SubMeshItem> descriptors = new NativeList<SubMeshItem>(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++) {
                    currentSubmesh = 1;

                    UpdateMesh(entities[i], ref currentSubmesh, meshDataArray[i], meshDataBuffer, descriptors);


                }


            }
            public void UpdateMesh(Entity entity, ref int submesh, Mesh.MeshData meshData, NativeList<UIMeshData> meshDataBuffer, NativeList<SubMeshItem> descriptors) {
                if (vertexData.HasComponent(entity)) {
                    var v = vertexData[entity];
                    var start = meshDataBuffer.Length;
                    meshDataBuffer.AddRange(v.AsNativeArray());
                    var s = new SubMeshItem
                    {
                        subMesh = submesh++,
                        descriptor = new SubMeshDescriptor(start, v.Length)
                    };
                    descriptors.Add(s);
                }
                if (childData.HasComponent(entity)) {
                    var children = childData[entity];
                    for (int i = 0; i < children.Length; i++) {
                        UpdateMesh(children[i], ref submesh, meshData, meshDataBuffer, descriptors);
                    }

                }
            }
        }
        public struct UIMeshSlice {
            public int startIndex;
            public int length;
        }
        public struct SubMeshItem {
            public int subMesh;
            public SubMeshDescriptor descriptor;
        }

    }
}
