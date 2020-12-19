using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeroWeNeed.Commons;
using NeroWeNeed.UIDots;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Hash128 = Unity.Entities.Hash128;

namespace NeroWeNeed.UIDots {
    [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class BindModelSystem : GameObjectConversionSystem {
        public const string SHADER_ASSET = "Packages/github.neroweneed.uidots/Runtime/Resources/UIShader.shadergraph";
        protected override void OnUpdate() {
            Entities.ForEach((UIObject obj) => DeclareReferencedAsset(obj.model));
            Entities.WithNone<Mesh, Material>().ForEach((Entity entity, UIModel model) =>
             {
                 var material = model.GetMaterial();
                 EntityManager.AddComponentObject(entity, material);
             });
        }

        private struct MaterialInfo {
            public Material material;
            public Texture2D atlas;
            public Texture2DArray fonts;
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class UIConversionSystem : GameObjectConversionSystem {
        public struct Settings {
            public Hash128 hash;
            public Entity dataEntity;
        }
        private List<Mesh> meshes = new List<Mesh>();
        private List<UIModel> models = new List<UIModel>();


        protected unsafe override void OnCreate() {
            base.OnCreate();
        }
        protected unsafe override void OnUpdate() {

            //Stores meshHash, blob hash, blob entity
            var objectProcessList = new NativeList<ValueTuple<Hash128, Hash128, UIContext, UICameraContext, int>>(8, Allocator.Temp);
            var objectHashes = new NativeHashMap<Hash128, int>(8, Allocator.Temp);
            var entityMap = new NativeHashMap<Hash128, Entity>(8, Allocator.Temp);

            //var nodeInfoMap = new NativeMultiHashMap<Hash128, UINodeDecompositionJob.NodeInfo>(8, Allocator.Temp);
            models.Clear();
            this.meshes.Clear();
            using var context = new BlobAssetComputationContext<Settings, UIGraph>(BlobAssetStore, 32, Allocator.Temp);
            Entities.ForEach((UIObject uiObject) =>
            {
                var meshHash = new Hash128((uint)(uiObject.model?.GetHashCode() ?? 0), (uint)(uiObject.uiCamera?.GetHashCode() ?? 0), 0, 0);
                var blobHash = new Hash128((uint)((uiObject.model?.GetHashCode()) ?? 0), 0, 0, 0);
                UIContext context = UIContext.CreateContext(uiObject.uiCamera);
                UICameraContext cameraContext = default;

                if (uiObject.uiCamera != null)
                    cameraContext = new UICameraContext
                    {
                        cameraLTW = uiObject.uiCamera.UILayerCameraObject.transform.localToWorldMatrix,
                        clipPlane = new float2(uiObject.uiCamera.UILayerCamera.nearClipPlane, uiObject.uiCamera.UILayerCamera.farClipPlane)
                    };
                if (!objectHashes.TryGetValue(meshHash, out int meshIndex)) {
                    meshIndex = this.meshes.Count;
                    this.meshes.Add(new Mesh());
                }
                Debug.Log("here");
                objectProcessList.Add(ValueTuple.Create(meshHash, blobHash, context, cameraContext, meshIndex));


            });
            Entities.ForEach((Entity entity, UIModel model) =>
            {
                var hash = new Hash128((uint)((model?.GetHashCode()) ?? 0), 0, 0, 0);
                entityMap[hash] = entity;
                context.AssociateBlobAssetWithUnityObject(hash, model);
                if (context.NeedToComputeBlobAsset(hash)) {
                    var settings = new Settings
                    {
                        hash = hash,
                        dataEntity = entity
                    };
                    models.Add(model);

                    context.AddBlobAssetToCompute(hash, settings);
                }
            });


            using var settings = context.GetSettings(Allocator.Temp);
            var nodes = new NativeMultiHashMap<Hash128, UINodeDecompositionJob.NodeInfo>(8, Allocator.TempJob);
            for (int i = 0; i < settings.Length; i++) {
                context.AddComputedBlobAsset(settings[i].hash, models[i].Create(Allocator.Persistent));
            }
            if (objectProcessList.Length > 0) {
                var graphs = new NativeArray<BlobAssetReference<UIGraph>>(objectProcessList.Length, Allocator.TempJob);
                var meshData = Mesh.AllocateWritableMeshData(objectProcessList.Length);
                var contexts = new NativeArray<UIContext>(objectProcessList.Length, Allocator.TempJob);


                int streamSize = 0;
                for (int i = 0; i < objectProcessList.Length; i++) {
                    context.GetBlobAsset(objectProcessList[i].Item2, out var blob);
                    graphs[i] = blob;
                    contexts[i] = objectProcessList[i].Item3;
                    streamSize += blob.Value.nodes.Length;
                }
                var nodeStream = new NativeStream(streamSize, Allocator.TempJob);

                new UILayoutJob
                {
                    graphs = graphs,
                    contexts = contexts,
                    meshDataArray = meshData
                }.Schedule(objectProcessList.Length, 1).Complete();

                new UINodeDecompositionJob
                {
                    graphs = graphs,
                    nodes = nodeStream.AsWriter()
                }.Schedule(objectProcessList.Length, 1).Complete();
                var result = nodeStream.ToNativeArray<UINodeDecompositionJob.NodeInfo>(Allocator.Temp);
                for (int i = 0; i < result.Length; i++) {
                    nodes.Add(objectProcessList[result[i].graphIndex].Item2, result[i]);
                }
                contexts.Dispose();
                nodeStream.Dispose();
                graphs.Dispose();
                Mesh.ApplyAndDisposeWritableMeshData(meshData, meshes);
                foreach (var m in meshes) {
                    m.RecalculateBounds();

                }
            }
            int index = 0;
            Entities.ForEach((UIObject obj) =>
            {

                DeclareAssetDependency(obj.gameObject, obj.model);
                var entity = GetPrimaryEntity(obj);
                DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, true);
                Mesh mesh;
                Material material;
                BlobAssetReference<UIGraph> graph;
                IEnumerator<UINodeDecompositionJob.NodeInfo> nodeIter;
                bool updateCache;
                var info = objectProcessList[index++];
                if (entityMap.TryGetValue(info.Item2, out var dataEntity)) {
                    context.GetBlobAsset(info.Item2, out graph);
                    material = EntityManager.GetComponentObject<Material>(dataEntity);
                    mesh = meshes[info.Item5];
                    nodeIter = nodes.GetValuesForKey(info.Item2);
                    updateCache = true;
                }
                else {
                    mesh = obj.cachedMesh;
                    material = obj.cachedMaterial;
                    graph = obj.cachedBlob;
                    nodeIter = obj.cachedNodeData.GetEnumerator();
                    updateCache = false;
                }
                if (obj.uiCamera != null) {
                    DstEntityManager.AddComponent<UIScreenElement>(entity);
                }
                var contextSource = obj.uiCamera == null ? Entity.Null : GetPrimaryEntity(obj.uiCamera.gameObject);
                DstEntityManager.AddComponentData(entity, new UIRoot(graph, contextSource));

                DstEntityManager.AddSharedComponentData(entity, new RenderMesh
                {
                    mesh = mesh,
                    material = material,
                    subMesh = 0,
                    castShadows = ShadowCastingMode.Off,
                    receiveShadows = false,
                    needMotionVectorPass = false,
                    layer = obj.gameObject.layer
                });

                DstEntityManager.AddComponentData(entity, new RenderBounds
                {
                    Value = mesh.GetSubMesh(0).bounds.ToAABB()
                });
                if (updateCache) {
                    obj.cachedMesh = mesh;
                    obj.cachedBlob = graph;
                    obj.cachedMaterial = material;
                    obj.cachedNodeData.Clear();
                }


                this.DeclareLinkedEntityGroup(obj.gameObject);
                var children = new NativeList<UINode>(Allocator.Temp);

                var loc = obj.transform.position;
                var rot = obj.transform.rotation;
                var scale = obj.transform.localScale;
                if (obj.faceCamera && obj.uiCamera != null) {
                    rot = math.mul(rot, (quaternion)obj.uiCamera.UILayerCameraObject.transform.rotation);

                }
                var baseLtw = float4x4.TRS(loc, rot, scale);
                while (nodeIter.MoveNext()) {
                    if (updateCache)
                        obj.cachedNodeData.Add(nodeIter.Current);
                    var nodeEntity = CreateAdditionalEntity(obj);
                    DstEntityManager.AddComponentData(nodeEntity, new UIConfiguration { index = nodeIter.Current.graphNodeIndex, submesh = nodeIter.Current.subMesh });
                    var ptr = ((UIConfig*)((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + nodeIter.Current.location.offset)));
                    DstEntityManager.SetName(nodeEntity, $"{obj.name}[{ptr->name.ToString(ptr)}]");
                    DstEntityManager.AddComponentData(nodeEntity, new UIParent { value = entity });
                    DstEntityManager.AddSharedComponentData(nodeEntity, new RenderMesh
                    {
                        mesh = mesh,
                        material = material,
                        subMesh = nodeIter.Current.subMesh,
                        castShadows = ShadowCastingMode.Off,
                        receiveShadows = false,
                        needMotionVectorPass = false,
                        layer = obj.gameObject.layer
                    });
                    var subMesh = mesh.GetSubMesh(nodeIter.Current.subMesh);
                    var bounds = subMesh.bounds;
                    DstEntityManager.AddComponentData(nodeEntity, new RenderBounds
                    {
                        Value = mesh.bounds.ToAABB()
                    });
                    /*                             var topLeft = mesh.vertices[subMesh.firstVertex];
                                                float4x4.TRS(new float3(-bounds.extents.x+(topLeft.x)))

                                                var position = obj.transform.localToWorldMatrix.SetTRS(new float3(bounds.e)) */
                    DstEntityManager.AddComponentData(nodeEntity, new LocalToWorld { Value = baseLtw });
                    DstEntityManager.AddComponentData<UIParent>(nodeEntity, entity);
                    DstEntityManager.AddComponent<UICameraContext>(nodeEntity);
                    DstEntityManager.AddComponent<UIContext>(nodeEntity);
                    if (obj.uiCamera != null) {
                        DstEntityManager.AddComponentData(nodeEntity, new UIContextSource { value = GetPrimaryEntity(obj.uiCamera.UILayerCameraObject) });
                        if (obj.faceCamera) {
                            DstEntityManager.AddComponent<UIFaceScreen>(nodeEntity);
                        }
                        if (obj.screenUI) {
                            DstEntityManager.AddComponentData<UIScreenElement>(nodeEntity,new UIScreenElement { alignment = Alignment.Left });
                        }
                    }
                    children.Add(nodeEntity);
                    ConfigureEditorRenderData(nodeEntity, obj.gameObject, true);

                    /*                             DstEntityManager.AddComponent<Translation>(nodeEntity);
                                                DstEntityManager.AddComponent<Rotation>(nodeEntity);
                                                DstEntityManager.AddComponent<Scale>(nodeEntity); */
                }
                DstEntityManager.AddComponentData(entity, new LocalToWorld { Value = baseLtw });
                if (DstEntityManager.HasComponent<Rotation>(entity)) {
                    DstEntityManager.SetComponentData<Rotation>(entity, new Rotation { Value = rot });
                }
                DstEntityManager.AddComponent<UICameraContext>(entity);
                DstEntityManager.AddComponent<UIContext>(entity);
                if (obj.uiCamera != null) {
                    DstEntityManager.AddComponentData(entity, new UIContextSource { value = GetPrimaryEntity(obj.uiCamera.UILayerCameraObject) });
                    if (obj.faceCamera) {
                        DstEntityManager.AddComponent<UIFaceScreen>(entity);
                    }
                    if (obj.screenUI) {
                        DstEntityManager.AddComponentData<UIScreenElement>(entity, new UIScreenElement { alignment = Alignment.Left });
                    }
                }
                var childrenBuffer = DstEntityManager.AddBuffer<UINode>(entity);
                childrenBuffer.AddRange(children);
                ConfigureEditorRenderData(entity, obj.gameObject, true);

            });
            nodes.Dispose();

        }
        protected override void OnDestroy() {
            base.OnDestroy();
        }
    }
}