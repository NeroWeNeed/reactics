using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Reactics.Util;
using Unity.Transforms;
using Unity.Jobs;
using System;

namespace Reactics.Battle
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    [DisableAutoCreation]
    public class MapRenderSystem : ComponentSystem
    {
        private MapWorld.WorldArchetypes Archetypes;
        private EntityQuery MapRendererQuery;
        private NativeHashMap<int, Entity> childEntityBuffer;

        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;

        protected override void OnCreate()
        {
            this.InjectResources();
            Archetypes = new MapWorld.WorldArchetypes(EntityManager);
            MapRendererQuery = GetEntityQuery(Archetypes.MapRenderer.GetComponentTypes());

            RequireForUpdate(MapRendererQuery);


        }
        protected override void OnStartRunning()
        {
            if (!childEntityBuffer.IsCreated)
                childEntityBuffer = new NativeHashMap<int, Entity>(Enum.GetValues(typeof(MapLayer)).Length, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            ComponentDataFromEntity<RenderMap> targetData = GetComponentDataFromEntity<RenderMap>(true);
            BufferFromEntity<RenderMapLayerChild> childrenData = GetBufferFromEntity<RenderMapLayerChild>(false);

            Entities.With(MapRendererQuery).ForEach((entity) =>
            {
                
                RenderMap target = targetData[entity];
                if (EntityManager.HasComponent<HighlightTile>(target.map))
                {
                    childEntityBuffer.Clear();
                    DynamicBuffer<RenderMapLayerChild> children = childrenData[entity];
                    NativeList<int> renderLayers = new NativeList<int>(Allocator.Temp);
                    for (int i = 0; i < children.Length; i++)
                    {
                        childEntityBuffer[(int)children[i].layer] = children[i].child;
                    }
                    DynamicBuffer<HighlightTile> highlights = EntityManager.GetBuffer<HighlightTile>(target.map);
                    highlights.ToMultiHashMap(out NativeMultiHashMap<int, Point> points, highlights.Length, Allocator.Temp, x => (int)x.layer, x => x.point);
                    Mesh mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;
                    foreach (var layer in points.GetKeyArray(Allocator.Temp))
                    {
                        if (layer == 0)
                            continue;
                        UpdateMesh(mesh, layer, points.GetValuesForKey(layer), points.CountValuesForKey(layer), EntityManager.GetComponentData<MapHeader>(target.map).width);
                        if (!childEntityBuffer.ContainsKey(layer))
                        {
                            childEntityBuffer.Add(layer, AddLayerRenderer(entity, layer));
                        }
                    }
                }
            });
        }

        protected override void OnStopRunning()
        {
            childEntityBuffer.Dispose();
        }
        private Entity AddLayerRenderer(Entity parent, int layer)
        {
            Entity entity = PostUpdateCommands.CreateEntity(Archetypes.MapRendererChild);


            PostUpdateCommands.SetSharedComponent(entity, new RenderMesh
            {
                mesh = EntityManager.GetSharedComponentData<RenderMesh>(parent).mesh,
                material = hoverMaterial,
                subMesh = layer
            });
            PostUpdateCommands.AddBuffer<RenderMapLayerChild>(parent).Add(new RenderMapLayerChild { layer = (MapLayer)layer, child = entity });
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