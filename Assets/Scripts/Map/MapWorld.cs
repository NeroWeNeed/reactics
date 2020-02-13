#define UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


namespace Reactics.Battle
{
    
    public class MapWorld : World
    {

        public readonly WorldArchetypes Archetypes;

        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;

        public MapWorld(Map map) : base($"Map World ({map.Name})")
        {
            Archetypes = new WorldArchetypes(EntityManager);
            var mapEntity = map.CreateEntity(EntityManager);
            var mapRenderer = EntityManager.CreateEntity(Archetypes.MapRenderer);
            EntityManager.SetSharedComponentData(mapRenderer, new RenderMesh
            {
                mesh = map.GenerateMesh(),
                material = baseMaterial,
                subMesh = 0
            });
/*             EntityManager.SetSharedComponentData(mapRenderer, new MapRender
            {
                map = mapEntity
                
            }); */
            //AddSystem(new MapHighlightSystem2(Archetypes));
        }

        public struct WorldArchetypes
        {
            public readonly EntityArchetype Player;
            public readonly EntityArchetype MapBody;
            public readonly EntityArchetype Map;
            public readonly EntityArchetype MapRenderer;
            public readonly EntityArchetype MapRendererChild;
            public WorldArchetypes(EntityManager manager)
            {
                Player = manager.CreateArchetype(typeof(MapPlayer));
                MapBody = manager.CreateArchetype(typeof(MapBody), typeof(LocalToWorld));
                Map = manager.CreateArchetype(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroupPoint));
                MapRenderer = manager.CreateArchetype(typeof(RenderMap),typeof(RenderMapLayerChild), typeof(RenderMesh), typeof(LocalToWorld),typeof(Translation));
                MapRendererChild = manager.CreateArchetype(typeof(RenderMesh), typeof(LocalToWorld),typeof(Translation));
            }
        }
    }


    public struct MapPlayer : IComponentData
    {
        public NativeString128 name;
        public long id;
    }
}