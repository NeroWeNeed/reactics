using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NeroWeNeed.Commons;
using NeroWeNeed.UIDots;
using NeroWeNeed.UIDots.Editor;
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
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Hash128 = Unity.Entities.Hash128;

namespace NeroWeNeed.UIDots {
    /*     [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
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
        }
     */
/*     [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class UIConversionSystem : GameObjectConversionSystem {
        public struct Settings {
            public Hash128 hash;
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
            Entities.ForEach((UIObject obj) =>
            {

                DeclareAssetDependency(obj.gameObject, obj.model);
                var meshHash = new Hash128((uint)(obj.model?.GetHashCode() ?? 0), (uint)(obj.uiCamera?.GetHashCode() ?? 0), 0, 0);
                var blobHash = new Hash128((uint)((obj.model?.GetHashCode()) ?? 0), 0, 0, 0);
                UIContext uiContext = UIContext.CreateContext(obj.uiCamera);
                UICameraContext uiCameraContext = default;
                context.AssociateBlobAssetWithUnityObject(blobHash, obj.model);
                if (context.NeedToComputeBlobAsset(blobHash)) {
                    var settings = new Settings
                    {
                        hash = blobHash
                    };
                    models.Add(obj.model);
                    context.AddBlobAssetToCompute(blobHash, settings);
                }
                if (obj.uiCamera != null) {
                    uiCameraContext = new UICameraContext
                    {
                        cameraLTW = obj.uiCamera.UILayerCameraObject.transform.localToWorldMatrix,
                        clipPlane = new float2(obj.uiCamera.UILayerCamera.nearClipPlane, obj.uiCamera.UILayerCamera.farClipPlane)
                    };
                }

                if (!objectHashes.TryGetValue(meshHash, out int meshIndex)) {
                    meshIndex = this.meshes.Count;
                    this.meshes.Add(new Mesh());
                }
                objectProcessList.Add(ValueTuple.Create(meshHash, blobHash, uiContext, uiCameraContext, meshIndex));
            });


            using var settings = context.GetSettings(Allocator.Temp);
            var nodes = new NativeMultiHashMap<Hash128, UINodeDecompositionJob.NodeInfo>(8, Allocator.TempJob);
            for (int i = 0; i < settings.Length; i++) {
                context.AddComputedBlobAsset(settings[i].hash, models[i].CreateGraphAsset(Allocator.Persistent));
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

                var entity = GetPrimaryEntity(obj);
                DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, true);
                Mesh mesh;
                Material material;
                IEnumerator<UINodeDecompositionJob.NodeInfo> nodeIter;
                bool updateCache;
                var info = objectProcessList[index++];
                if (context.GetBlobAsset(info.Item2, out BlobAssetReference<UIGraph> graph)) {
                    material = obj.model.group?.Material;
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
                    rot = math.mul(rot, obj.uiCamera.UILayerCameraObject.transform.rotation);
                }
                var baseLtw = float4x4.TRS(loc, rot, scale);
                DstEntityManager.AddComponentData(entity, new LocalToWorld { Value = baseLtw });
                if (DstEntityManager.HasComponent<Rotation>(entity)) {
                    DstEntityManager.SetComponentData<Rotation>(entity, new Rotation { Value = math.mul(rot, obj.uiCamera.UILayerCameraObject.transform.rotation) });
                }
                while (nodeIter.MoveNext()) {
                    if (updateCache)
                        obj.cachedNodeData.Add(nodeIter.Current);

                    var nodeEntity = CreateAdditionalEntity(obj);
                    DstEntityManager.AddComponentData(nodeEntity, new UINodeInfo { index = nodeIter.Current.graphNodeIndex, submesh = nodeIter.Current.subMesh });
                    var name = UIConfigUtility.GetName(graph.Value.nodes[nodeIter.Current.graphNodeIndex].configurationMask, ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + nodeIter.Current.location.offset);
                    DstEntityManager.SetName(nodeEntity, $"{obj.name}[{name}]");
                    DstEntityManager.AddComponentData(nodeEntity, new UIParent { value = entity });
                    DstEntityManager.AddComponentData(nodeEntity, new Parent { Value = entity });
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
                    DstEntityManager.AddComponentData<UIParent>(nodeEntity, entity);

                    DstEntityManager.AddComponentData<LocalToWorld>(nodeEntity, new LocalToWorld { Value = float4x4.identity });
                    DstEntityManager.AddComponentData<Rotation>(nodeEntity, new Rotation { Value = quaternion.identity });
                    DstEntityManager.AddComponent<Translation>(nodeEntity);
                    DstEntityManager.AddComponentData<Scale>(nodeEntity, new Scale { Value = 1f });
                    DstEntityManager.AddComponentData(nodeEntity, new LocalToParent { Value = float4x4.identity });
                    DecorateEntity(obj, nodeEntity, graph, nodeIter.Current.graphNodeIndex, nodeIter.Current.subMesh, mesh);
                    children.Add(nodeEntity);
                }
                var uiChildrenBuffer = DstEntityManager.AddBuffer<UINode>(entity);
                uiChildrenBuffer.AddRange(children);
                var childBuffer = DstEntityManager.AddBuffer<Child>(entity);
                childBuffer.AddRange(children.AsArray().Reinterpret<Child>());
                DecorateEntity(obj, entity, graph, 0, 0, mesh);
            });
            nodes.Dispose();

        }
        private unsafe void DecorateEntity(UIObject obj, Entity entity, BlobAssetReference<UIGraph> graph, int currentIndex, int currentSubmesh, Mesh mesh) {

            DstEntityManager.AddComponentData(entity, new RenderBounds
            {
                //Value = currentSubmesh == 0 ? mesh.GetSubMesh(0).bounds.ToAABB() : mesh.bounds.ToAABB()
                Value = mesh.GetSubMesh(currentSubmesh).bounds.ToAABB()
            });
            DstEntityManager.AddComponent<WorldRenderBounds>(entity);
            DstEntityManager.AddComponent<UICameraContext>(entity);
            DstEntityManager.AddComponent<UIContext>(entity);
            if (obj.uiCamera != null) {
                DstEntityManager.AddComponentData(entity, new UIContextSource { value = GetPrimaryEntity(obj.uiCamera.UILayerCameraObject) });
                if (obj.faceCamera) {
                    DstEntityManager.AddComponent<UIFaceScreen>(entity);
                }
                if (obj.screenUI) {
                    DstEntityManager.AddComponentData<UIScreenElement>(entity, new UIScreenElement { alignment = Alignment.Center });
                }
            }
            if (UIConfigUtility.HasConfigBlock(graph.Value.nodes[currentIndex].configurationMask, UIConfigLayoutTable.SelectableConfig)) {
                var configOffset = UIJobUtility.GetConfigOffset(graph, currentIndex, out int configLength);
                var selectable = (SelectableConfig*)UIConfigUtility.GetConfig(graph.Value.nodes[currentIndex].configurationMask, UIConfigLayoutTable.SelectableConfig, ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configOffset);
                DstEntityManager.AddComponent<UISelectable>(entity);
                if (selectable->onSelect.IsCreated) {
                    DstEntityManager.AddComponentData(entity, new UIOnSelect { value = selectable->onSelect });
                }
            }
            DstEntityManager.AddComponent<PerInstanceCullingTag>(entity);

            DstEntityManager.AddComponent<FrozenRenderSceneTag>(entity);
            ConfigureEditorRenderData(entity, obj.gameObject, true);
        }
        protected override void OnDestroy() {
            base.OnDestroy();
        }
    } */
}