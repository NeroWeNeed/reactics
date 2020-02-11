using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Reactics.Util;
using Unity.Transforms;
using System.Linq;
using Reactics.Battle.Components;

namespace Reactics.Battle.Components
{
    public struct MapLayerControlComponent : IComponentData
    {
        public int layer;
        public MapLayerControlComponent(params MapLayer[] layers)
        {
            layer = 0;
            foreach (var item in layers)
            {
                layer |= (int)item;

            }

        }
        public bool Contains(MapLayer layer) => (this.layer & ((int)layer)) != 0;
    }
    public enum MapLayer
    {
        BASE,
        PLAYER_MOVE,
        PLAYER_ATTACK,
        PLAYER_SUPPORT,
        PLAYER_ALL,
        ENEMY_MOVE,
        ENEMY_ATTACK,
        ENEMY_SUPPORT,
        ENEMY_ALL,
        ALLY_MOVE,
        ALLY_ATTACK,
        ALLY_SUPPORT,
        ALLY_ALL,
        UTILITY
    }


    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapRenderDataSystem : ComponentSystem
    {

        private EntityArchetype mapArchetype;
        private EntityArchetype renderDataArchetype;
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;
        protected override void OnCreate()
        {
            this.InjectResources();
            mapArchetype = EntityManager.CreateArchetype(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroup));
            renderDataArchetype = EntityManager.CreateArchetype(typeof(MapRenderData), typeof(RenderMesh), typeof(Translation), typeof(LocalToWorld));
            RequireForUpdate(GetEntityQuery(mapArchetype.GetComponentTypes()));
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> renderDataEntities = Entities.With(GetEntityQuery(renderDataArchetype.GetComponentTypes())).ToEntityQuery().ToEntityArray(Allocator.TempJob);
            NativeHashMap<Entity, Entity> renderDataEntitiesMap = new NativeHashMap<Entity, Entity>(renderDataEntities.Length, Allocator.TempJob);
            foreach (var item in renderDataEntities)
            {
                renderDataEntitiesMap[EntityManager.GetComponentData<MapRenderData>(item).mapEntity] = item;
            }

            Entities.With(GetEntityQuery(mapArchetype.GetComponentTypes())).ForEach((entity) =>
            {
                MapHeader header = EntityManager.GetComponentData<MapHeader>(entity);
                if (renderDataEntitiesMap.ContainsKey(entity))
                {

                }
                else
                {
                    Entity renderData = PostUpdateCommands.CreateEntity(renderDataArchetype);
                    Mesh mesh = new Mesh();
                    MapUtil.GenerateMesh(mesh, header.width, header.length, 1f);
                    PostUpdateCommands.SetComponent(renderData, new MapRenderData { mapEntity = entity });
                    PostUpdateCommands.SetSharedComponent(renderData, new RenderMesh { mesh = mesh, material = baseMaterial, subMesh = 0 });
                }

            });
            renderDataEntities.Dispose();
            renderDataEntitiesMap.Dispose();
        }


    }
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MapRenderDataSystem))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapHighlightSystem : ComponentSystem
    {

        private EntityArchetype renderLayerArchetype;
        private EntityArchetype renderDataArchetype;
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;
        protected override void OnCreate()
        {
            this.InjectResources();

            renderLayerArchetype = EntityManager.CreateArchetype(typeof(MapLayerRender), typeof(RenderMesh));

            renderDataArchetype = EntityManager.CreateArchetype(typeof(MapRenderData), typeof(RenderMesh), typeof(Translation), typeof(LocalToWorld));
            RequireForUpdate(GetEntityQuery(typeof(MapRenderData)));
        }
        protected override void OnUpdate()
        {
            NativeArray<Entity> renderEntities = Entities.With(GetEntityQuery(renderLayerArchetype.GetComponentTypes())).ToEntityQuery().ToEntityArray(Allocator.TempJob);

            Entities.WithAll(typeof(MapRenderData)).ForEach((entity) =>
            {
                MapRenderData renderData = EntityManager.GetComponentData<MapRenderData>(entity);
                RenderMesh meshData = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                if (renderData.isRenderingBase && !renderEntities.Any(x =>
                {
                    var render = EntityManager.GetComponentData<MapLayerRender>(x);
                    return render.layer == 0 && render.mapRenderDataEntity == entity;
                }))
                {
                    var baseLayerEntity = PostUpdateCommands.CreateEntity(renderLayerArchetype);
                    PostUpdateCommands.SetComponent(baseLayerEntity, new MapLayerRender { mapRenderDataEntity = entity, layer = 0 });
                    PostUpdateCommands.SetSharedComponent(baseLayerEntity, new RenderMesh { mesh = meshData.mesh, subMesh = 0, material = baseMaterial });
                }
                if (EntityManager.HasComponent<HighlightTile>(renderData.mapEntity))
                {
                    DynamicBuffer<HighlightTile> highlights = EntityManager.GetBuffer<HighlightTile>(renderData.mapEntity);
                    NativeMultiHashMap<int, Point> highlightedTiles = new NativeMultiHashMap<int, Point>(highlights.Length, Allocator.TempJob);
                    foreach (var item in highlights)
                    {
                        highlightedTiles.Add((int)item.layer, item.point);
                    }
                    foreach (var layer in highlightedTiles.GetKeyArray(Allocator.TempJob))
                    {
                        if (layer == 0)
                            continue;
                        UpdateMesh(meshData.mesh, layer, highlightedTiles.GetValuesForKey(layer), highlightedTiles.CountValuesForKey(layer), EntityManager.GetComponentData<MapHeader>(renderData.mapEntity).width);

                    }
                    
                }


            });
            renderEntities.Dispose();
        }

        private void UpdateMesh(Mesh mesh, int layer, NativeMultiHashMap<int, Point>.Enumerator points, int size, ushort Width)
        {
            int[] triangles = new int[size * 6];
            int index = 0;
            foreach (var point in points)
            {
                triangles[index * 6] = point.y * (Width + 1) + point.x;
                triangles[(index * 6) + 1] = point.y * (Width + 1) + point.x + Width + 1;
                triangles[(index * 6) + 2] = point.y * (Width + 1) + point.x + Width + 2;
                triangles[(index * 6) + 3] = point.y * (Width + 1) + point.x;
                triangles[(index * 6) + 4] = point.y * (Width + 1) + point.x + Width + 2;
                triangles[(index * 6) + 5] = point.y * (Width + 1) + point.x + 1;
                index++;
            }
            mesh.SetTriangles(triangles, layer);
        }

    }
    /*     [UpdateInGroup(typeof(PresentationSystemGroup))]
        [UpdateBefore(typeof(RenderMeshSystemV2))]
        [UpdateAfter(typeof(MapRenderDataSystem))]
        public class MapRenderSystem : ComponentSystem
        {
            [ResourceField("Materials/Map/MapMaterial.mat")]
            private Material baseMaterial;

            private EntityArchetype renderDataArchetype;

            private EntityArchetype renderArchetype;

            protected override void OnCreate()
            {
                this.InjectResources();
                renderDataArchetype = EntityManager.CreateArchetype(typeof(MapRenderData));
                renderArchetype = EntityManager.CreateArchetype(typeof(MapLayerRender), typeof(RenderMesh));

            }
            protected override void OnUpdate()
            {
                NativeArray<Entity> renderers = Entities.With(GetEntityQuery(renderArchetype.GetComponentTypes())).ToEntityQuery().ToEntityArray(Allocator.TempJob);

                NativeMultiHashMap<Entity, Entity> rendererMap = new NativeMultiHashMap<Entity, Entity>(renderers.Length, Allocator.TempJob);
                foreach (var item in renderers)
                {
                    rendererMap.Add(EntityManager.GetComponentData<MapLayerRender>(item).mapRenderDataEntity, item);
                }
                Entities.With(GetEntityQuery(renderDataArchetype.GetComponentTypes())).ForEach((entity) =>
                {
                    MapRenderData renderData = EntityManager.GetComponentData<MapRenderData>(entity);
                    if (rendererMap.ContainsKey(entity))
                    {

                        foreach (var item in rendererMap.GetValuesForKey(entity))
                        {
                            item.
                        }
                    }

                });
                throw new System.NotImplementedException();
            }
        } */
    /*     [UpdateInGroup(typeof(PresentationSystemGroup))]
        [UpdateBefore(typeof(RenderMeshSystemV2))]

        public class MapRenderSystem : ComponentSystem
        {
            private EntityArchetype renderMeshArchetype;

            [ResourceField("Materials/Map/MapMaterial.mat")]
            private Material baseMaterial;

            protected override void OnCreate()
            {
                base.OnCreate();
                this.InjectResources();
                //RequireSingletonForUpdate<MapLayerControlComponent>();
                RequireForUpdate(Entities.WithAll(typeof(MapHeaderComponent), typeof(MapTileElement), typeof(MapSpawnGroupElement)).ToEntityQuery());
                renderMeshArchetype = EntityManager.CreateArchetype(typeof(RenderMesh), typeof(MapRenderComponent), typeof(MapLayerControlComponent), typeof(LocalToWorld), typeof(Translation));
            }

            protected override void OnUpdate()
            {
                //MapLayerControlComponent controlComponent = GetSingleton<MapLayerControlComponent>();
                NativeArray<Entity> renderComponents = Entities.WithAll<MapRenderComponent, RenderMesh, MapLayerControlComponent, LocalToWorld, Translation>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
                NativeHashMap<Entity, EntityRenderData> renderData = new NativeHashMap<Entity, EntityRenderData>(renderComponents.Length, Allocator.TempJob);
                MapRenderComponent current;
                foreach (var item in renderComponents)
                {
                    current = EntityManager.GetComponentData<MapRenderComponent>(item);
                    renderData[current.entity] = new EntityRenderData
                    {
                        renderEntity = item,
                        mapEntity = current.entity,
                        lastVersion = current.lastVersion
                    };
                }
                Entities.WithAll<MapHeaderComponent, MapTileElement, MapSpawnGroupElement>().ForEach(entity =>
                {

                    if (!renderData.ContainsKey(entity))
                    {
                        DynamicBuffer<MapTileElement> tiles = EntityManager.GetBuffer<MapTileElement>(entity);
                        MapHeaderComponent header = EntityManager.GetComponentData<MapHeaderComponent>(entity);
                        if (tiles.Length == header.width * header.length)
                        {
                            var renderEntity = PostUpdateCommands.CreateEntity(renderMeshArchetype);
                            PostUpdateCommands.SetComponent(renderEntity, new MapRenderComponent { entity = entity, lastVersion = entity.Version });
                            Mesh mesh = new Mesh();
                            MapUtil.GenerateMesh(mesh, header.width, header.length, 1f);
                            PostUpdateCommands.SetSharedComponent(renderEntity, new RenderMesh
                            {
                                mesh = mesh,
                                material = baseMaterial,
                                subMesh = 0
                            });
                        }
                        else {
                            //TODO: Exception
                        }
                    }
                    else if (entity.Version - renderData[entity].lastVersion > 0)
                    {
                        var renderEntity = renderData[entity].renderEntity;
                        var header = EntityManager.GetComponentData<MapHeaderComponent>(renderData[entity].mapEntity);
                        DynamicBuffer<MapTileElement> tiles = EntityManager.GetBuffer<MapTileElement>(renderData[entity].mapEntity);
                        if (tiles.Length == header.width * header.length)
                        {
                            PostUpdateCommands.SetComponent(renderEntity, new MapRenderComponent { entity = entity, lastVersion = entity.Version });
                            Mesh mesh = new Mesh();
                            MapUtil.GenerateMesh(mesh, header.width, header.length, 1f);
                            PostUpdateCommands.SetSharedComponent(renderEntity, new RenderMesh
                            {
                                mesh = mesh,
                                material = baseMaterial,
                                subMesh = 0
                            });
                        }
                        else
                        {
                            PostUpdateCommands.DestroyEntity(renderEntity);
                        }
                    }
                });
                renderComponents.Dispose();
                renderData.Dispose();

            }
            private struct EntityRenderData
            {
                public Entity renderEntity;

                public Entity mapEntity;
                public int lastVersion;
            }
        } */
    struct MapRenderComponent : IComponentData
    {
        public Entity entity;
        public int lastVersion;
    }

}