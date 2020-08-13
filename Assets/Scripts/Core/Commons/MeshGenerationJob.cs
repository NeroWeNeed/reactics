using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace Reactics.Core.Commons {
    [BurstCompile]
    public struct MeshGenerationJob<TVertex> : IJob where TVertex : struct {
        public NativeArray<int> indices;
        public NativeArray<TVertex> vertices;
        public MeshDataArray meshes;
        public NativeMultiHashMap<int, MeshGenerationSubMesh> subMeshes;
        public NativeArray<MeshGenerationSlice> meshSlices;
        public static MeshGenerationJob<TVertex> Create(NativeArray<int> indices, NativeArray<TVertex> vertices, NativeMultiHashMap<int, MeshGenerationSubMesh> subMeshes, NativeArray<MeshGenerationSlice> meshSlices, VertexAttributeDescriptor[] descriptors) {
            var m = Mesh.AllocateWritableMeshData(meshSlices.Length);
            for (int i = 0; i < m.Length; i++) {
                m[i].SetIndexBufferParams(meshSlices[i].indexLength, IndexFormat.UInt32);
                m[i].SetVertexBufferParams(meshSlices[i].vertexLength, descriptors);


            }
            return new MeshGenerationJob<TVertex>
            {
                indices = indices,
                vertices = vertices,
                subMeshes = subMeshes,
                meshSlices = meshSlices,
                meshes = m
            };
        }
        public void Execute() {
            for (int i = 0; i < meshSlices.Length; i++) {
                var meshData = meshes[i];
                var vertexData = meshData.GetVertexData<TVertex>();
                vertexData.CopyFrom(vertices.GetSubArray(meshSlices[i].startVertex, meshSlices[i].vertexLength));
                var indexData = meshData.GetIndexData<int>();
                indexData.CopyFrom(indices.GetSubArray(meshSlices[i].startIndex, meshSlices[i].indexLength));

                meshData.subMeshCount = subMeshes.CountValuesForKey(i) + 1;
                foreach (var subMesh in subMeshes.GetValuesForKey(i)) {
                    meshes[i].SetSubMesh(subMesh.index, new SubMeshDescriptor(subMesh.start, subMesh.length, subMesh.topology));
                }
            }
        }

    }
    public struct MeshGenerationSubMesh {
        public int index;
        public int start;
        public int length;
        public MeshTopology topology;

    }
    public struct MeshGenerationSlice {
        public int startVertex, vertexLength, startIndex, indexLength;
    }
}