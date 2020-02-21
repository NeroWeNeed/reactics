using Reactics.Battle;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.LowLevel;

namespace Reactics
{

    public interface IMapWorld
    {
        Map Map { get; }
        MapWorldArchetypes Archetypes { get; }
    }
    public sealed class MapWorldContainer
    {
        public MapSimulationWorld Simulation { get; }

        public MapPresentationWorld Presentation { get; }

        private MapWorldContainer(Map map)
        {
            Simulation = new MapSimulationWorld(map);
            Presentation = new MapPresentationWorld(map);
            PlayerLoop.GetDefaultPlayerLoop();
        }

    }

    
    public class MapSimulationWorld : World, IMapWorld
    {
        public Map Map { get; }

        public MapWorldArchetypes Archetypes { get; }
        public MapSimulationWorld(Map map) : base($"Map Simulation World ({map.Name})")
        {
            Map = map;
            Archetypes = new MapWorldArchetypes(EntityManager);
            this.GetOrCreateSystem<InitializationSystemGroup>();
            this.GetOrCreateSystem<Unity.Entities.SimulationSystemGroup>();

        }


    }
    public class MapPresentationWorld : World, IMapWorld
    {
        public Map Map { get; }

        public MapWorldArchetypes Archetypes { get; }
        public MapPresentationWorld(Map map) : base($"Map Presentation World ({map.Name})")
        {
            Map = map;
            Archetypes = new MapWorldArchetypes(EntityManager);
            this.GetOrCreateSystem<InitializationSystemGroup>();
            this.GetOrCreateSystem<PresentationSystemGroup>();
        }
    }
    public struct MapWorldArchetypes
    {
        public readonly EntityArchetype Player;
        public readonly EntityArchetype MapBody;
        public readonly EntityArchetype Map;
        public readonly EntityArchetype MapRenderer;
        public readonly EntityArchetype MapRendererChild;
        internal MapWorldArchetypes(EntityManager manager)
        {
            Player = manager.CreateArchetype(typeof(MapPlayer));
            MapBody = manager.CreateArchetype(typeof(MapBody), typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Scale), typeof(RenderMesh));
            Map = manager.CreateArchetype(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroupPoint));
            MapRenderer = manager.CreateArchetype(typeof(RenderMap), typeof(RenderMapLayerChild), typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation));
            MapRendererChild = manager.CreateArchetype(typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation));
        }
    }


}