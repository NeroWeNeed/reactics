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

        static MapArchetypes()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Map = manager.CreateArchetype(typeof(MapData));
            Render = manager.CreateArchetype(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            Body = manager.CreateArchetype(typeof(MapBody));
        }
    }
    [UpdateInGroup(typeof(MapSystemGroup))]
    public class RandomPointTickerSystem : ComponentSystem
    {
        private int tick = 0;

        private int maxTick = 100;
        private Entity highlightEntity;
        private int x, y;
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapData>();
        }
        protected override void OnStartRunning()
        {
            highlightEntity = EntityManager.CreateEntity(typeof(HighlightTile));
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
                Debug.Log("UPDATING");
                DynamicBuffer<HighlightTile> highlightMaker = EntityManager.GetBuffer<HighlightTile>(highlightEntity);
                highlightMaker.Clear();
                MapData data = GetSingleton<MapData>();
                x += 2;
                y += 3;
                highlightMaker.Add(new HighlightTile
                {

                    point = new Point(x % data.Width, y % data.Length),
                    layer = MapLayer.HOVER
                });

                Entities.With(GetEntityQuery(typeof(MapBody))).ForEach((entity) =>
                {
                    if (!EntityManager.HasComponent<MapBodyTranslation>(entity))
                        PostUpdateCommands.AddComponent(entity, new MapBodyTranslation
                        {
                            point = new Point(x % data.Width, y % data.Length)
                        });
                });
                tick = 0;
            }
        }
    }



}