using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Reactics.Util;
using Unity.Transforms;

namespace Reactics.Battle
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapVisualizationSystem : ComponentSystem
    {
        private EntityArchetype mapArchetype;
        private EntityArchetype rootRendererArchetype;



        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;

        protected override void OnCreate()
        {
            
            this.InjectResources();
            mapArchetype = EntityManager.CreateArchetype(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroupPoint));
            rootRendererArchetype = EntityManager.CreateArchetype(typeof(MapRootRenderLayer), typeof(RenderMesh), typeof(Translation), typeof(LocalToWorld));
            RequireForUpdate(GetEntityQuery(mapArchetype.GetComponentTypes()));
        }
        protected override void OnUpdate()
        {
            Entities.With(GetEntityQuery(mapArchetype.GetComponentTypes())).ForEach((entity) =>
            {
                Entity rootRenderer;

                if (EntityManager.HasComponent(entity, typeof(LinkedEntityGroup)))
                {
                    DynamicBuffer<Entity> children = EntityManager.GetBuffer<LinkedEntityGroup>(entity).Reinterpret<Entity>();
                    if (children.Length <= 0 || !children.Find(out rootRenderer, x => EntityManager.HasComponent<MapRootRenderLayer>(x)))
                    {
                        rootRenderer = rootRenderer = AddRootRenderer(entity);
                    }
                }
                else
                {
                    rootRenderer = AddRootRenderer(entity);
                }



            });

        }
        private Entity AddRootRenderer(Entity parent)
        {
            Entity entity = PostUpdateCommands.CreateEntity(rootRendererArchetype);
            MapHeader header = EntityManager.GetComponentData<MapHeader>(parent);
            PostUpdateCommands.SetSharedComponent(entity,
            new RenderMesh
            {
                mesh = MapUtil.GenerateMesh(new Mesh(), header.width, header.length, 1f),
                material = baseMaterial,
                subMesh = 0
            });
            PostUpdateCommands.SetComponent(entity, new MapRootRenderLayer { lastVersion = entity.Version });


            DynamicBuffer<LinkedEntityGroup> buffer = EntityManager.HasComponent<LinkedEntityGroup>(parent) ? EntityManager.GetBuffer<LinkedEntityGroup>(parent) : PostUpdateCommands.AddBuffer<LinkedEntityGroup>(parent);

            buffer.Add(new LinkedEntityGroup { Value = entity });
            return entity;
        }

    }
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MapVisualizationSystem))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapHighlightSystem : ComponentSystem
    {
        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;
        private EntityArchetype mapArchetype;
        private EntityArchetype layerRendererArchetype;


        protected override void OnCreate()
        {
            this.InjectResources();
            mapArchetype = EntityManager.CreateArchetype(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroupPoint));
            layerRendererArchetype = EntityManager.CreateArchetype(typeof(MapRenderLayer), typeof(RenderMesh), typeof(Translation), typeof(LocalToWorld));
            RequireForUpdate(GetEntityQuery(mapArchetype.GetComponentTypes()));
        }
        protected override void OnUpdate()
        {
            Entities.With(GetEntityQuery(mapArchetype.GetComponentTypes())).ForEach((entity) =>
            {
                if (EntityManager.HasComponent(entity, typeof(LinkedEntityGroup)))
                {
                    DynamicBuffer<Entity> children = EntityManager.GetBuffer<LinkedEntityGroup>(entity).Reinterpret<Entity>();
                    if (children.Find(out Entity rootRenderer, x => EntityManager.HasComponent<MapRootRenderLayer>(x)))
                    {
                        DynamicBuffer<Entity> rootRendererChildren = (EntityManager.HasComponent<LinkedEntityGroup>(rootRenderer) ? EntityManager.GetBuffer<LinkedEntityGroup>(rootRenderer) : PostUpdateCommands.AddBuffer<LinkedEntityGroup>(rootRenderer)).Reinterpret<Entity>();
                        NativeList<int> renderLayers = new NativeList<int>(Allocator.Temp);
                        for (int i = 0; i < rootRendererChildren.Length; i++)
                        {
                            if (EntityManager.HasComponent<MapRenderLayer>(rootRendererChildren[i]) && EntityManager.HasComponent<RenderMesh>(rootRendererChildren[i]))
                                renderLayers.Add((int)EntityManager.GetComponentData<MapRenderLayer>(rootRendererChildren[i]).layer);
                        }
                        RenderMesh renderData = EntityManager.GetSharedComponentData<RenderMesh>(rootRenderer);
                        if (EntityManager.HasComponent<HighlightTile>(entity))
                        {
                            DynamicBuffer<HighlightTile> highlights = EntityManager.GetBuffer<HighlightTile>(entity);
                            highlights.ToMultiHashMap(out NativeMultiHashMap<int, Point> highlightedTiles, highlights.Length, Allocator.Temp, x => (int)x.layer, x => x.point);

                            foreach (var layer in highlightedTiles.GetKeyArray(Allocator.Temp))
                            {
                                if (layer == 0)
                                    continue;
                                UpdateMesh(renderData.mesh, layer, highlightedTiles.GetValuesForKey(layer), highlightedTiles.CountValuesForKey(layer), EntityManager.GetComponentData<MapHeader>(entity).width);
                                if (!renderLayers.Contains(layer))
                                {
                                    AddLayerRenderer(rootRenderer, layer);
                                    renderLayers.Add(layer);
                                }
                            }
                        }
                    }
                }
            });
        }

        private Entity AddLayerRenderer(Entity parent, int layer)
        {
            Entity entity = PostUpdateCommands.CreateEntity(layerRendererArchetype);
            PostUpdateCommands.SetComponent(entity, new MapRenderLayer { layer = (MapLayer)layer });
            PostUpdateCommands.SetSharedComponent(entity,
new RenderMesh
{
    mesh = EntityManager.GetSharedComponentData<RenderMesh>(parent).mesh,
    material = hoverMaterial,
    subMesh = layer
});
            DynamicBuffer<LinkedEntityGroup> buffer = EntityManager.HasComponent<LinkedEntityGroup>(parent) ? EntityManager.GetBuffer<LinkedEntityGroup>(parent) : PostUpdateCommands.AddBuffer<LinkedEntityGroup>(parent);
            buffer.Add(new LinkedEntityGroup { Value = entity });
            return entity;
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
}