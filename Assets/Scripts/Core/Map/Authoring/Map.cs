using System;
using NeroWeNeed.Commons;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Map.Authoring {
    [ConverterVersion("Nero", 2)]

    public class Map : MonoBehaviour, IConvertGameObjectToEntity {
        public static readonly float LAYER_OFFSET = 0.0001f;

        [SerializeField]
        private Material mapMaterial;
        /*         [SerializeField]
                private AssetReference<Material> mapMaterial; */

        [SerializeField]
        public MapAsset map;

        [SerializeField]
        public EnumDictionary<MapLayer, Color> layerColors = new EnumDictionary<MapLayer, Color>();

        private Mesh _mesh;

        public Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                    _mesh = map.CreateMesh();
                return _mesh;
            }
        }


#if UNITY_EDITOR

        private void OnValidate() {

            if (map != null) {
                _mesh = map.CreateMesh();
            }
            else {
                _mesh = null;
            }
        }
#endif
        //TODO: Editor Rendering not working for some reason.
        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {

            if (map != null) {
                var mapSystem = dstManager.World.GetOrCreateSystem<MapSystemGroup>();
                dstManager.AppendArchetype(entity, mapSystem.Archetypes.Map);
                var mesh = map.CreateMesh();
                mesh.RecalculateBounds();
                dstManager.AddComponentData(entity, new MapData(map.CreateBlob()));
                dstManager.AddComponentData(entity, new MapHighlightState { dirty = 0, states = new Unity.Collections.LowLevel.Unsafe.UnsafeMultiHashMap<ushort, Point>(16, Allocator.Persistent) });
                dstManager.AddComponentData(entity, new MapCollisionState { value = new Unity.Collections.LowLevel.Unsafe.UnsafeHashMap<Point, Entity>(16, Allocator.Persistent) });
                dstManager.AddComponentData(entity, new MapRenderInfo { baseIndexCount = mesh.GetIndexCount(0), tileSize = 1f, elevationStep = 0.25f });
                var layerEntities = new NativeArray<Entity>(MapLayers.Count, Allocator.TempJob);

                for (int i = 0; i < MapLayers.Count; i++) {
                    layerEntities[i] = conversionSystem.CreateAdditionalEntity(this);
                    dstManager.AppendArchetype(layerEntities[i], mapSystem.Archetypes.MapRenderLayer);
                }

                var layers = (MapLayer[])Enum.GetValues(typeof(MapLayer));
                Material material = mapMaterial;

                for (int i = 0; i < layers.Length; i++) {
#if UNITY_EDITOR
                    dstManager.SetName(layerEntities[i], $"{this.name} ({layers[i]} Layer)");
#endif
                    dstManager.SetSharedComponentData(layerEntities[i], new RenderMesh
                    {
                        mesh = mesh,
                        material = material,
                        subMesh = i,
                        castShadows = ShadowCastingMode.Off,
                        receiveShadows = false,
                        layer = transform.gameObject.layer
                    });
                    dstManager.SetComponentData(layerEntities[i], new MapElement { value = entity });
                    dstManager.SetComponentData(layerEntities[i], new MaterialColor
                    {
                        Value = layerColors[layers[i]].ToFloat4()
                    });

                    dstManager.SetComponentData(layerEntities[i], new RenderBounds { Value = mesh.bounds.ToAABB() });
                    /*                 dstManager.AddComponentData(layerEntities[i], new Translation { Value = new float3(0, 0, 0) });
                                    dstManager.AddComponentData(layerEntities[i], new Rotation { Value = quaternion.identity });
                                    dstManager.AddComponentData(layerEntities[i], new CompositeScale { Value = float4x4.Scale(1) });
                                    dstManager.AddComponentData(layerEntities[i], new PerInstanceCullingTag());
                                    dstManager.AddComponentData(layerEntities[i], new BuiltinMaterialPropertyUnity_RenderingLayer { Value = 1 }); */
                    //Overlapping submeshes can't overlap anymore so offset position slightly upward for each layer.
                    dstManager.SetComponentData(layerEntities[i], new LocalToWorld { Value = float4x4.Translate(new float3(0, transform.position.y + (LAYER_OFFSET * i), 0)) });
                    dstManager.GetBuffer<MapLayerRenderer>(entity).Add(new MapLayerRenderer { entity = layerEntities[i], layer = layers[i] });
                    //dstManager.AddBuffer<MapTileEffect>(entity);
                    dstManager.AddComponentData(layerEntities[i], new Parent { Value = entity });
                    conversionSystem.ConfigureEditorRenderData(layerEntities[i], this.gameObject, true);
                }
                var children = dstManager.AddBuffer<Child>(entity);
                children.AddRange(layerEntities.Reinterpret<Child>());
                conversionSystem.DeclareLinkedEntityGroup(this.gameObject);
                conversionSystem.ConfigureEditorRenderData(entity, this.gameObject, true);
                layerEntities.Dispose();
            }

        }


    }
}