using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Battle
{


    [UpdateInGroup(typeof(BattleSimulationSystemGroup))]
    public class MapSystemGroup : ComponentSystemGroup
    {


    }
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapRenderSystemGroup : ComponentSystemGroup
    {

    }

    public static class MapArchetypes
    {
        public static readonly EntityArchetype Map;

        public static readonly EntityArchetype Render;

        public static readonly EntityArchetype Body;

        public static readonly EntityArchetype RenderableBody;

        public static readonly EntityArchetype RenderableMap;

        static MapArchetypes()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Map = manager.CreateArchetype(ComponentType.ReadOnly(typeof(MapData)));
            RenderableMap = manager.CreateArchetype(ComponentType.ReadOnly(typeof(MapData)), typeof(MapRenderData));
            Render = manager.CreateArchetype(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            Body = manager.CreateArchetype(typeof(MapBody));
            RenderableBody = manager.CreateArchetype(typeof(MapBody), typeof(RenderMesh), typeof(MapBodyMeshOffset));
        }
    }
    [UpdateInGroup(typeof(MapSystemGroup))]
    public class RandomPointTickerSystem : ComponentSystem
    {
        private int tick = 0;

        private int maxTick = 10;
        private Entity highlightEntity;
        private int x, y;
        private Unity.Mathematics.Random random;
        int prefBody = 0;
        protected override void OnCreate()
        {
            random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToFileTime());
            RequireSingletonForUpdate<MapData>();
        }
        protected override void OnStartRunning()
        {
            highlightEntity = EntityManager.CreateEntity(typeof(HighlightTile));
            DynamicBuffer<HighlightTile> highlightMaker = EntityManager.GetBuffer<HighlightTile>(highlightEntity);
            MapData data = GetSingleton<MapData>();
            for (int i = 0; i < data.Tiles.Length; i++)
            {
                if (data.Tiles[i].Inaccessible)
                {
                    highlightMaker.Add(new HighlightTile
                    {

                        point = data.GetTilePoint(i),
                        layer = MapLayer.HOVER
                    });
                }
            }
        }

        protected override void OnStopRunning()
        {
            EntityManager.DestroyEntity(highlightEntity);
        }
        protected override void OnUpdate()
        {
            tick++;
            if (tick > maxTick)
            {
                MapData data = GetSingleton<MapData>();


                int index = 0;
                Entities.With(GetEntityQuery(typeof(MapBody))).ForEach((entity) =>
                {
                    if (index % 2 == prefBody)
                        if (!EntityManager.HasComponent<MapBodyTranslation>(entity))
                            PostUpdateCommands.AddComponent(entity, new MapBodyTranslation
                            {
                                point = new Point((ushort)random.NextUInt() % data.Width, (ushort)random.NextUInt() % data.Length),
                                maxDistance = 10
                            });
                    index++;
                });
                prefBody = (prefBody + 1) % 2;
                tick = 0;
            }
        }
    }



}