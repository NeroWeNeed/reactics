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
        private UISchema schema;
        private BlobAssetReference<CompiledUISchema> compiledSchema;
        protected override void OnCreate() {
            base.OnCreate();
            schema = UIGlobalSettings.GetOrCreateSettings().schema;
            compiledSchema = this.schema.Compile(Allocator.Persistent);
        }
        protected override void OnDestroy() {
            compiledSchema.Dispose();
        }
        protected unsafe override void OnUpdate() {
            var contexts = new NativeList<UIContextData>(8, Allocator.TempJob);
            var graphData = new NativeList<UIGraphData>(8, Allocator.TempJob);
            Entity schemaEntity = Entity.Null;
            int nodeCount = 0;
            meshes.Clear();
            Entities.ForEach((UIObject obj) =>
            {

                //TODO: Writes every conversion frame but will crash under certain conditions otherwise. Crashes observed involving opening and closing subscenes without modifying anything after script reload.

                var guid = obj.model?.GetOutputGuid();

                if (!string.IsNullOrEmpty(guid)) {
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
                    contexts.Add(UIContextData.CreateContext(obj.camera));
                    meshes.Add(new Mesh());
                    if (schemaEntity == Entity.Null) {
                        schemaEntity = CreateAdditionalEntity(obj);
                        DstEntityManager.SetName(schemaEntity, "UI Schema");
                        DstEntityManager.AddSharedComponentData(schemaEntity, new UISchemaData { value = schema });
                    }
                    DeclareAssetDependency(obj.gameObject, schema);
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
                    schema = compiledSchema,
                    graphs = graphDataArray,
                    contexts = contexts,
                    meshDataArray = meshData
                }.Schedule(graphData.Length, 1).Complete();
                new UINodeDecompositionJob
                {
                    schema = compiledSchema,
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
                    if (!string.IsNullOrEmpty(obj.cachedGuid)) {
                        var entity = GetPrimaryEntity(obj);
                        var gd = graphDataArray[index];
                        DstEntityManager.AddComponentData(entity, new UIGraph { value = new BlittableAssetReference(obj.cachedGuid) });
                        DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, false);
                        Material material;
                        if (gd.TryGetConfigBlock(0, UIConfigLayoutTable.MaterialConfig, out IntPtr result)) {
                            material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(((MaterialConfig*)(result.ToPointer()))->material.ToHex()));                           
                        }
                        else {
                            material = obj.model.GetMaterial();
                        }



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
                        DstEntityManager.AddComponentData(entity, new UIPixelScale { value = obj.pixelScale });
                        var bounds = meshes[index].GetSubMesh(0).bounds.ToAABB();
                        DstEntityManager.AddComponentData(entity, new RenderBounds { Value = bounds });

                        DstEntityManager.AddComponent<UIContext>(entity);

                        if (obj.camera != null) {
                            DstEntityManager.AddComponentData(entity, new UIContextSource { value = GetPrimaryEntity(obj.camera) });
                            var ltc = new LocalToCamera
                            {
                                cameraLTW = obj.camera.transform.localToWorldMatrix,
                                clipPlane = new float2(obj.camera.nearClipPlane, obj.camera.farClipPlane),
                                alignment = obj.alignment,
                                offsetX = obj.offsetX,
                                offsetY = obj.offsetY
                            };
                            DstEntityManager.AddComponentData(entity, ltc);
                            var rotation = quaternion.LookRotation(ltc.cameraLTW.c2.xyz, ltc.cameraLTW.c1.xyz);
                            var translate = ltc.cameraLTW.c3.xyz + new float3(ltc.alignment.GetOffset(bounds.Size.xy, new float2(Screen.currentResolution.height * obj.camera.aspect, Screen.currentResolution.height)), 0) + math.mul(rotation, math.forward() * ltc.clipPlane.x * 2f) + (math.mul(rotation, math.right()) * ltc.offsetX.Normalize(contexts[index])) + (math.mul(rotation, math.up()) * ltc.offsetY.Normalize(contexts[index]));
                            DstEntityManager.SetComponentData(entity, new LocalToWorld { Value = float4x4.TRS(translate, rotation, obj.pixelScale) });
                            DeclareDependency(obj, obj.camera);
                        }

                        DstEntityManager.AddComponent<PerInstanceCullingTag>(entity);

                        nodeEntities.Clear();
                        var nodeInfoIter = nodes.GetValuesForKey(index);
                        while (nodeInfoIter.MoveNext()) {
                            var nodeInfo = nodeInfoIter.Current;
                            var nodeEntity = CreateAdditionalEntity(obj);
                            var name = graphData[index].GetNodeName(nodeInfo.nodeIndex);
                            if (string.IsNullOrEmpty(name))
                                name = $"Node#{nodeInfo.nodeIndex}";
                            DstEntityManager.SetName(nodeEntity, $"{DstEntityManager.GetName(entity)}[{name}]");
                            DstEntityManager.AddComponentData(nodeEntity, new UINodeInfo { index = nodeInfo.nodeIndex, submesh = nodeInfo.submesh });
                            DstEntityManager.AddComponentData(nodeEntity, new UIParent { value = entity });
                            DstEntityManager.AddComponentData(nodeEntity, new Parent { Value = entity });
                            DstEntityManager.AddComponentData(nodeEntity, new LocalToWorld { Value = float4x4.identity });
                            DstEntityManager.AddComponentData(nodeEntity, new Rotation { Value = quaternion.identity });
                            DstEntityManager.AddComponentData(nodeEntity, new Scale { Value = 1f });
                            DstEntityManager.AddComponentData(nodeEntity, new LocalToParent { Value = float4x4.identity });
                            if (gd.TryGetConfigBlock(0, UIConfigLayoutTable.MaterialConfig, out result)) {
                                material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(((MaterialConfig*)result.ToPointer())->material.ToHex()));
                            }
                            else {
                                material = obj.model.GetMaterial();
                            }
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