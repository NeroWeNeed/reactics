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
                 var mesh = new Mesh();
                 var material = GetUIMaterial(model).material;
                 EntityManager.AddComponentObject(entity, mesh);
                 EntityManager.AddComponentObject(entity, material);
             });
        }
        private unsafe MaterialInfo GetUIMaterial(UIModel model) {
            model.CollectTextures(out Texture2D atlas, out Texture2D[] fonts);
            var info = new MaterialInfo
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Shader>(SHADER_ASSET))
            };
            if (fonts?.Length > 0) {
                /*  var fontTextureSize = fonts.Aggregate(int2.zero, (size, texture) => new int2(math.max(size.x, texture.width), math.max(size.y, texture.height)));
                fontTextureSize = math.ceilpow2(fontTextureSize);
                var fontArray = new Texture2DArray(fontTextureSize.x, fontTextureSize.y, fonts.Length, TextureFormat.RFloat, 0, true);
                var buffer = new Texture2D(fontTextureSize.x, fontTextureSize.y, TextureFormat.RFloat, 0, true);
                for (int i = 0; i < fonts.Length; i++) {
                    Graphics.ConvertTexture(fonts[i], buffer);
                    var target = fontArray.GetPixelData<float>(0, i);
                    var src = buffer.GetPixelData<float>(0);
                    UnsafeUtility.MemCpy(target.GetUnsafePtr(), src.GetUnsafePtr(), src.Length * UnsafeUtility.SizeOf<float>());
                    //scalePixels[i] = new float2(textures[i].width / (float)size.x, textures[i].height / (float)size.y);
                }
                info.material.SetTexture("_Fonts", fontArray);
                info.fonts = fontArray;  */
                info.material.SetTexture("_Fonts", fonts[0]);
            }
            if (atlas != null) {
                info.material.SetTexture("_Images", atlas);
                info.atlas = atlas;
            }
            return info;
        }

        private struct MaterialInfo {
            public Material material;
            public Texture2D atlas;
            public Texture2DArray fonts;
        }
    }
/*     public struct UIGeneratedBlob : IComponentData {
        public BlobAssetReference<UIGraph> blob;
    }
    [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class UIBlobConversionSystem : GameObjectConversionSystem {
        private List<Mesh> meshes = new List<Mesh>();
        private List<UIModel> models = new List<UIModel>();
        protected unsafe override void OnUpdate() {
            var processBlobAssets = new NativeList<Hash128>(8, Allocator.Temp);
            using var context = new BlobAssetComputationContext<Settings, UIGraph>(BlobAssetStore, 32, Allocator.Temp);
            Entities.ForEach((Entity entity, UIModel model, Mesh mesh) =>
            {

                var hash = new Hash128((uint)((model?.GetHashCode()) ?? 0), 0, 0, 0);
                processBlobAssets.Add(hash);
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
                meshes.Add(mesh);
            });
            using var settings = context.GetSettings(Allocator.Temp);

            var graphs = new NativeArray<BlobAssetReference<UIGraph>>(settings.Length, Allocator.TempJob);
            var meshData = Mesh.AllocateWritableMeshData(settings.Length);
            var contexts = new NativeArray<UILengthContext>(settings.Length, Allocator.TempJob);
            var ctx = new UILengthContext
            {
                dpi = Screen.dpi,
                pixelScale = 0.01f
            };
            UnsafeUtility.MemCpyReplicate(contexts.GetUnsafePtr(), UnsafeUtility.AddressOf(ref ctx), UnsafeUtility.SizeOf<UILengthContext>(), settings.Length);
            for (int i = 0; i < settings.Length; i++) {
                var blob = models[i].Create(Allocator.Persistent);
                context.AddComputedBlobAsset(settings[i].hash, blob);
                graphs[i] = blob;
            }
            new UILayoutJob
            {
                graphs = graphs,
                contexts = contexts,
                meshDataArray = meshData
            }.Schedule(settings.Length, 1).Complete();
            Mesh.ApplyAndDisposeWritableMeshData(meshData, meshes);
            foreach (var m in meshes) {
                m.RecalculateBounds();
            }
            int index = 0;
            Entities.ForEach((Entity entity, UIModel model, Mesh mesh) =>
            {
                var hash = processBlobAssets[index++];
                context.GetBlobAsset(hash, out var blob);
                EntityManager.AddComponentData(entity, new UIGeneratedBlob { blob = blob });
            });
        }

        public struct Settings {
            public Hash128 hash;
            public Entity dataEntity;
        }
    } */
    [WorldSystemFilter(WorldSystemFilterFlags.GameObjectConversion)]
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class UIConversionSystem : GameObjectConversionSystem {
        public struct Settings {
            public Hash128 hash;
            public Entity dataEntity;
        }
        private List<Mesh> meshes = new List<Mesh>();
        private List<UIModel> models = new List<UIModel>();
        private EntityQuery modelQuery;
        protected unsafe override void OnCreate() {
            modelQuery = GetEntityQuery(typeof(UIModel), typeof(Mesh), typeof(Material));
            base.OnCreate();
        }
        protected unsafe override void OnUpdate() {
            var processBlobAssets = new NativeList<Hash128>(8, Allocator.Temp);
            var modelEntities = new NativeList<Entity>(8, Allocator.Temp);
            var entityMap = new NativeHashMap<BlittableAssetReference, ValueTuple<Entity, Hash128>>(8, Allocator.Temp);
            models.Clear();
            this.meshes.Clear();
            using var context = new BlobAssetComputationContext<Settings, UIGraph>(BlobAssetStore, 32, Allocator.Temp);
            Entities.ForEach((Entity entity, UIModel model, Mesh mesh) =>
            {
                var hash = new Hash128((uint)((model?.GetHashCode()) ?? 0), 0, 0, 0);
                processBlobAssets.Add(hash);
                modelEntities.Add(entity);
                entityMap[AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(model))] = new ValueTuple<Entity, Hash128>(entity, hash);
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
                meshes.Add(mesh);
            });

            using var settings = context.GetSettings(Allocator.Temp);
            for (int i = 0; i < settings.Length; i++) {
                context.AddComputedBlobAsset(settings[i].hash, models[i].Create(Allocator.Persistent));
            }
            if (modelEntities.Length > 0) {
                var graphs = new NativeArray<BlobAssetReference<UIGraph>>(modelEntities.Length, Allocator.TempJob);
                var meshData = Mesh.AllocateWritableMeshData(modelEntities.Length);
                var contexts = new NativeArray<UILengthContext>(modelEntities.Length, Allocator.TempJob);
                var ctx = UILengthContext.CreateContext();
                UnsafeUtility.MemCpyReplicate(contexts.GetUnsafePtr(), UnsafeUtility.AddressOf(ref ctx), UnsafeUtility.SizeOf<UILengthContext>(), modelEntities.Length);
                for (int i = 0; i < modelEntities.Length; i++) {
                    context.GetBlobAsset(processBlobAssets[i], out var blob);
                    graphs[i] = blob;
                }
                new UILayoutJob
                {
                    graphs = graphs,
                    contexts = contexts,
                    meshDataArray = meshData
                }.Schedule(modelEntities.Length, 1).Complete();
                Mesh.ApplyAndDisposeWritableMeshData(meshData, meshes);
                foreach (var m in meshes) {
                    m.RecalculateBounds();
                }
            }
            Entities.ForEach((UIObject obj) =>
            {
                
                DeclareAssetDependency(obj.gameObject, obj.model);
                var entity = GetPrimaryEntity(obj);
                DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, true);
                if (entityMap.TryGetValue(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(obj.model)), out var data)) {
                    var dataEntity = data.Item1;
                    context.GetBlobAsset(data.Item2, out var blob);
                    var material = EntityManager.GetComponentObject<Material>(dataEntity);
                    var mesh = EntityManager.GetComponentObject<Mesh>(dataEntity);
                    DstEntityManager.AddComponentData(entity, new UIRoot(blob));
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
                        Value = mesh.bounds.ToAABB()
                    });
                }
                else {
                    DstEntityManager.AddComponent<UIRoot>(entity);
                    DstEntityManager.AddSharedComponentData(entity, new RenderMesh());
                    DstEntityManager.AddComponent<RenderBounds>(entity);
                }
                DstEntityManager.AddComponentData(entity, new LocalToWorld { Value = obj.transform.localToWorldMatrix });
                ConfigureEditorRenderData(entity, obj.gameObject, true);
            });

            processBlobAssets.Dispose();
            /* var assetEntities = new NativeHashMap<BlittableAssetReference, Entity>(8, Allocator.Temp);
            Entities.ForEach((Entity entity, UIModel model, Mesh mesh) =>
            {
                assetEntities[AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(model))] = entity;
            });
            Entities.ForEach((UIObject obj) =>
            {

                DeclareAssetDependency(obj.gameObject, obj.model);
                var dataEntity = assetEntities[AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(obj.model))];
                var entity = GetPrimaryEntity(obj);
                var blob = EntityManager.GetComponentData<UIGeneratedBlob>(dataEntity).blob;
                var material = EntityManager.GetComponentObject<Material>(dataEntity);
                var mesh = EntityManager.GetComponentObject<Mesh>(dataEntity);
                DstEntityManager.AddComponentData(entity, new UIRoot(blob));
                DstEntityManager.AddSharedComponentData<UIDirtyState>(entity, true);
                DstEntityManager.AddSharedComponentData(entity, new RenderMesh
                {
                    mesh = mesh,
                    material = material,
                    subMesh = 0,
                    castShadows = ShadowCastingMode.Off,
                    receiveShadows = false,
                    needMotionVectorPass = false,
                    layer = 0
                });
                DstEntityManager.AddComponentData(entity, new RenderBounds
                {
                    Value = mesh.bounds.ToAABB()
                });
                DstEntityManager.AddComponentData(entity, new LocalToWorld { Value = obj.transform.localToWorldMatrix });
                ConfigureEditorRenderData(entity, obj.gameObject, true);
            }); */
        }
        protected override void OnDestroy() {
            base.OnDestroy();
        }
    }
}