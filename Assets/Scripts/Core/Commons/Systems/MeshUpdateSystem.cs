using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Commons {


    public abstract class MeshUpdateSystem16 : SystemBase {
        protected readonly List<int> oldIndices = new List<int>();
        protected override void OnUpdate() {

            Entities.WithChangeFilter<MeshIndexUpdateData16>().ForEach((ref DynamicBuffer<MeshIndexUpdateData16> indexUpdateData, in MeshIndexUpdate indexUpdate, in RenderMesh renderMesh) =>
            {

                NativeArray<ushort> buffer;

                if (indexUpdateData.Length > 0) {
                    switch (indexUpdate.mode) {

                        case MeshIndexUpdateMode.Prepend:
                            renderMesh.mesh.GetIndices(oldIndices, renderMesh.subMesh);
                            buffer = new NativeArray<ushort>(oldIndices.Count + indexUpdateData.Length, Allocator.Temp);
                            buffer.Slice(0, indexUpdateData.Length).CopyFrom(indexUpdateData.AsNativeArray().Reinterpret<ushort>().Slice());
                            for (int i = 0; i < oldIndices.Count; i++) {
                                buffer[i + indexUpdateData.Length] = (ushort)oldIndices[i];
                            }
                            renderMesh.mesh.SetIndices(buffer, indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, indexUpdate.baseVertex);

                            break;
                        case MeshIndexUpdateMode.Append:
                            renderMesh.mesh.GetIndices(oldIndices, renderMesh.subMesh);
                            buffer = new NativeArray<ushort>(oldIndices.Count + indexUpdateData.Length, Allocator.Temp);

                            for (int i = 0; i < oldIndices.Count; i++) {
                                buffer[i] = (ushort)oldIndices[i];
                            }
                            buffer.Slice(oldIndices.Count, indexUpdateData.Length).CopyFrom(indexUpdateData.AsNativeArray().Reinterpret<ushort>().Slice());
                            renderMesh.mesh.SetIndices(buffer, indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, indexUpdate.baseVertex);

                            break;
                        case MeshIndexUpdateMode.Set:
                            renderMesh.mesh.SetIndices(indexUpdateData.AsNativeArray().Reinterpret<ushort>(), indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, indexUpdate.baseVertex);
                            break;
                        case MeshIndexUpdateMode.Clear:
                            renderMesh.mesh.SetSubMesh(renderMesh.subMesh, new SubMeshDescriptor(0, 0, indexUpdate.topology));
                            break;
                    }
                    indexUpdateData.Clear();
                }
            }).WithoutBurst().Run();

        }
    }
    public class MeshUpdateSystemUInt32 : SystemBase {
        protected readonly List<int> oldIndices = new List<int>();
        protected override void OnUpdate() {

            Entities.WithChangeFilter<MeshIndexUpdateData32>().ForEach((ref DynamicBuffer<MeshIndexUpdateData32> indexUpdateData, in MeshIndexUpdate indexUpdate, in RenderMesh renderMesh) =>
            {

                NativeArray<uint> buffer;

                if (indexUpdateData.Length > 0) {
                    switch (indexUpdate.mode) {

                        case MeshIndexUpdateMode.Prepend:
                            renderMesh.mesh.GetIndices(oldIndices, renderMesh.subMesh);
                            buffer = new NativeArray<uint>(oldIndices.Count + indexUpdateData.Length, Allocator.Temp);
                            buffer.Slice(0, indexUpdateData.Length).CopyFrom(indexUpdateData.AsNativeArray().Reinterpret<uint>().Slice());
                            for (int i = 0; i < oldIndices.Count; i++) {
                                buffer[i + indexUpdateData.Length] = (uint)oldIndices[i];
                            }
                            renderMesh.mesh.SetIndices(buffer, indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, indexUpdate.baseVertex);

                            break;
                        case MeshIndexUpdateMode.Append:
                            renderMesh.mesh.GetIndices(oldIndices, renderMesh.subMesh);
                            buffer = new NativeArray<uint>(oldIndices.Count + indexUpdateData.Length, Allocator.Temp);

                            for (int i = 0; i < oldIndices.Count; i++) {
                                buffer[i] = (uint)oldIndices[i];
                            }
                            buffer.Slice(oldIndices.Count, indexUpdateData.Length).CopyFrom(indexUpdateData.AsNativeArray().Reinterpret<uint>().Slice());
                            renderMesh.mesh.SetIndices(buffer, indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, indexUpdate.baseVertex);

                            break;
                        case MeshIndexUpdateMode.Set:
                            renderMesh.mesh.SetIndices(indexUpdateData.AsNativeArray().Reinterpret<uint>(), indexUpdate.topology, renderMesh.subMesh, indexUpdate.calculateBounds, (int)indexUpdate.baseVertex);
                            break;
                        case MeshIndexUpdateMode.Clear:
                            renderMesh.mesh.SetSubMesh(renderMesh.subMesh, new SubMeshDescriptor(0, 0, indexUpdate.topology));
                            break;
                    }
                    indexUpdateData.Clear();
                }
            }).WithoutBurst().Run();
        }

    }
}