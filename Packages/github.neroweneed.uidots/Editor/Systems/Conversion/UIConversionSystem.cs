using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NeroWeNeed.UIDots.Editor {
    [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class UIConversionSystem : GameObjectConversionSystem {
        private List<Mesh> meshes = new List<Mesh>();
        protected unsafe override void OnUpdate() {
            var contexts = new NativeList<UIContext>(8, Allocator.TempJob);
            var graphData = new NativeList<UIGraphData>(8, Allocator.TempJob);
            int nodeCount = 0;
            meshes.Clear();
            Entities.ForEach((UIObject obj) =>
            {
                /*
                TODO: Writes every conversion frame but will crash under certain conditions otherwise. Crashes observed involving opening and closing subscenes without modifying anything after script reload.
                */
                var guid = obj.model?.GetOutputGuid();
                if (guid != null) {
                    IntPtr ptr;
                    long allocatedLength;
                    using (var fs = File.OpenRead(UnityEditor.AssetDatabase.GUIDToAssetPath(guid))) {
                        allocatedLength = math.ceilpow2(fs.Length);
                        ptr = (IntPtr)UnsafeUtility.Malloc(allocatedLength, 0, Allocator.Persistent);
                        using (var us = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), 0, fs.Length, FileAccess.Write)) {
                            fs.CopyTo(us);
                        }
                    }
                    var gd = new UIGraphData { value = ptr, allocatedLength = allocatedLength };
                    graphData.Add(gd);
                    nodeCount += gd.GetNodeCount();
                    contexts.Add(UIContext.CreateContext(obj.uiCamera));
                    meshes.Add(new Mesh());
                }
                obj.cachedGuid = guid;
            });
            if (graphData.Length > 0) {
                var graphDataArray = graphData.AsArray();
                var meshData = Mesh.AllocateWritableMeshData(graphData.Length);
                var submeshes = new NativeArray<int>(graphData.Length, Allocator.TempJob);
                var stream = new NativeStream(nodeCount, Allocator.TempJob);
                new UILayoutJob
                {
                    graphs = graphDataArray,
                    contexts = contexts,
                    meshDataArray = meshData
                }.Schedule(graphData.Length, 1).Complete();
                new UINodeDecompositionJob
                {
                    graphs = graphDataArray,
                    nodes = stream.AsWriter(),
                    submeshCount = submeshes
                }.Schedule(graphData.Length, 1).Complete();
                Mesh.ApplyAndDisposeWritableMeshData(meshData, meshes);
                var result = stream.ToNativeArray<DedicatedNodeInfo>(Allocator.Temp);
                var nodes = new NativeMultiHashMap<int, DedicatedNodeInfo>(graphData.Length, Allocator.Temp);
                for (int i = 0; i < result.Length; i++) {
                    nodes.Add(result[i].graphIndex, result[i]);
                }
                stream.Dispose();
                submeshes.Dispose();
                int index = 0;
                var nodeEntities = new NativeList<Entity>(Allocator.Temp);
                Entities.ForEach((UIObject obj) =>
                {
                    if (obj.cachedGuid != null) {
                        var entity = GetPrimaryEntity(obj);
                        DstEntityManager.AddComponentData(entity, new UIGraph { value = new BlittableAssetReference(obj.cachedGuid) });
                        DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, false);
                        var material = obj.model.GetMaterial();
                        DeclareAssetDependency(obj.gameObject, obj.model);
                        DeclareAssetDependency(obj.gameObject, material);
                        DstEntityManager.AddSharedComponentData(entity, new RenderMesh
                        {
                            mesh = meshes[index],
                            material = material,
                            subMesh = 0,
                            castShadows = ShadowCastingMode.Off,
                            receiveShadows = false,
                            needMotionVectorPass = false,
                            layer = obj.gameObject.layer
                        });
                        DstEntityManager.AddComponentData(entity, new RenderBounds { Value = meshes[index].GetSubMesh(0).bounds.ToAABB() });
                        DstEntityManager.AddComponent<UIContext>(entity);
                        DstEntityManager.AddComponentData(entity, new UIContextSource { value = obj.uiCamera == null ? Entity.Null : GetPrimaryEntity(obj.uiCamera) });
                        UIContext uiContext = contexts[index];
                        UICameraContext uiCameraContext = default;
                        if (obj.uiCamera != null) {
                            uiCameraContext = new UICameraContext
                            {
                                cameraLTW = obj.uiCamera.UILayerCameraObject.transform.localToWorldMatrix,
                                clipPlane = new float2(obj.uiCamera.UILayerCamera.nearClipPlane, obj.uiCamera.UILayerCamera.farClipPlane)
                            };
                        }
                        DstEntityManager.AddComponentData(entity, uiContext);
                        DstEntityManager.AddComponentData(entity, uiCameraContext);
                        DstEntityManager.AddComponent<PerInstanceCullingTag>(entity);
                        nodeEntities.Clear();
                        var nodeInfoIter = nodes.GetValuesForKey(index);
                        while (nodeInfoIter.MoveNext()) {
                            var nodeInfo = nodeInfoIter.Current;
                            var nodeEntity = CreateAdditionalEntity(obj);
                            var name = graphData[index].GetNodeName(nodeInfo.nodeIndex) ?? $"Node#{nodeInfo.nodeIndex}";
                            DstEntityManager.SetName(nodeEntity, $"{DstEntityManager.GetName(entity)}[{name}]");
                            DstEntityManager.AddComponentData(nodeEntity, new UINodeInfo { index = nodeInfo.nodeIndex, submesh = nodeInfo.submesh });
                            DstEntityManager.AddComponentData(nodeEntity, new UIParent { value = entity });
                            DstEntityManager.AddComponentData(nodeEntity, new Parent { Value = entity });
                            DstEntityManager.AddComponentData(nodeEntity, new LocalToWorld { Value = float4x4.identity });
                            DstEntityManager.AddComponentData(nodeEntity, new Rotation { Value = quaternion.identity });
                            DstEntityManager.AddComponentData(nodeEntity, new Scale { Value = 1f });
                            DstEntityManager.AddComponentData(nodeEntity, new LocalToParent { Value = float4x4.identity });
                            DstEntityManager.AddSharedComponentData(nodeEntity, new RenderMesh
                            {
                                mesh = meshes[index],
                                material = material,
                                subMesh = nodeInfo.submesh,
                                castShadows = ShadowCastingMode.Off,
                                receiveShadows = false,
                                needMotionVectorPass = false,
                                layer = obj.gameObject.layer
                            });
                            DstEntityManager.AddComponentData(nodeEntity, new RenderBounds { Value = meshes[index].GetSubMesh(nodeInfo.submesh).bounds.ToAABB() });
                            DstEntityManager.AddComponent<PerInstanceCullingTag>(nodeEntity);
                            nodeEntities.Add(nodeEntity);
                            ConfigureEditorRenderData(nodeEntity, obj.gameObject, true);
                        }
                        var buffer = DstEntityManager.AddBuffer<UINode>(entity);
                        buffer.AddRange(nodeEntities.AsArray().Reinterpret<UINode>());
                        if (nodeEntities.Length > 0) {
                            var children = DstEntityManager.AddBuffer<Child>(entity);
                            children.AddRange(nodeEntities.AsArray().Reinterpret<Child>());
                        }
                        UnsafeUtility.Free(graphData[index].value.ToPointer(), Allocator.TempJob);
                        index++;
                        ConfigureEditorRenderData(entity, obj.gameObject, true);
                    }
                });
            }

            contexts.Dispose();
            graphData.Dispose();
        }
    }

}