using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Reactics.Util;
using Unity.Transforms;
using Unity.Jobs;
using System;
using System.Collections.Generic;

namespace Reactics.Battle
{


    [UpdateInGroup(typeof(MapRenderSystemGroup))]
    [DisableAutoCreation]
    public class MapRenderSystem : ComponentSystem
    {
        public Mesh MapMesh { get; private set; }
        private EntityQuery renderMapQuery;

        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;

        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material mapMaterial;

        [ResourceField("Materials/Map/PlayerMoveMaterial.mat")]
        private Material playerMoveMaterial;

        [ResourceField("Materials/Map/PlayerAttackMaterial.mat")]
        private Material playerAttackMaterial;

        [ResourceField("Materials/Map/PlayerSupportMaterial.mat")]
        private Material playerSupportMaterial;

        private MapLayerRenderSystem mapLayerRenderSystem;
        protected override void OnCreate()
        {
            this.InjectResources();
            renderMapQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(RenderMap) },
                None = new ComponentType[] { typeof(RenderMesh) }
            });
            RequireSingletonForUpdate<MapData>();
            RequireSingletonForUpdate<MapRenderData>();
            RequireForUpdate(renderMapQuery);
        }
        protected override void OnStartRunning()
        {
            if (MapMesh == null)
            {

                var mapData = GetSingleton<MapData>();
                var mapRenderData = GetSingleton<MapRenderData>();
                ref var dataBlob = ref mapData.map.Value;

                MapMesh = dataBlob.GenerateMesh(mapRenderData.tileSize, mapRenderData.elevationStep);
            }
        }
        protected override void OnUpdate()
        {
            Entities.With(renderMapQuery).ForEach((Entity entity, ref RenderMap renderMap) =>
            {
                if (!EntityManager.HasComponent<RenderMesh>(entity))
                {
                    //TODO: Proper Materials for layers
                    PostUpdateCommands.AddSharedComponent(entity, new RenderMesh
                    {
                        mesh = MapMesh,
                        subMesh = (int)renderMap.layer,
                        material = renderMap.layer == MapLayer.BASE ? mapMaterial : 
                        renderMap.layer == MapLayer.HOVER ? hoverMaterial : 
                        renderMap.layer == MapLayer.PLAYER_MOVE ? playerMoveMaterial :
                        renderMap.layer == MapLayer.PLAYER_ATTACK ? playerAttackMaterial :
                        playerSupportMaterial
                    });
                }

            });
        }
    }
    [UpdateInGroup(typeof(MapRenderSystemGroup))]
    [DisableAutoCreation]
    public class MapLayerRenderSystem : ComponentSystem
    {


        private EntityQuery query;

        private NativeMultiHashMap<int, Point> highlights;


        protected override void OnCreate()
        {
            query = GetEntityQuery(typeof(HighlightTile));
            query.SetChangedVersionFilter(typeof(HighlightTile));
            RequireForUpdate(query);
            RequireSingletonForUpdate<MapData>();

        }
        protected override void OnStopRunning()
        {
            if (highlights.IsCreated)
                highlights.Dispose();
        }

        protected override void OnUpdate()
        {
            NativeMultiHashMap<int, Point> tiles = new NativeMultiHashMap<int, Point>(16, Allocator.Persistent);
            Entities.With(query).ForEach((DynamicBuffer<HighlightTile> highlights) =>
            {
                for (int i = 0; i < highlights.Length; i++)
                    tiles.AddIfMissing((int)highlights[i].layer, highlights[i].point);

            });



            if (!highlights.IsCreated || !tiles.ContentEquals(ref highlights))
            {

                if (highlights.IsCreated)
                    highlights.Dispose();
                highlights = tiles;
                var renderSystem = World.GetOrCreateSystem<MapRenderSystem>();
                var mapData = GetSingleton<MapData>();
                var h = highlights.GetKeyArray(Allocator.TempJob);
                foreach (var layer in h)
                {
                    if (layer == 0)
                        continue;
                    NativeArray<int> triangles = new NativeArray<int>(highlights.CountValuesForKey(layer) * 6, Allocator.TempJob);


                    MapUtils.GenerateMeshTileTriangles(ref triangles, 0, mapData.Width, highlights.GetValuesForKey(layer));
                    renderSystem.MapMesh.subMeshCount = 20;//*&*
                    renderSystem.MapMesh.SetTriangles(triangles.ToArray(), layer);
                    triangles.Dispose();

                }
                h.Dispose();
            }
            else
            {
                tiles.Dispose();
            }
        }
    }
}