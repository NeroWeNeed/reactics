using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Reactics.Battle
{
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
}