using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [UpdateBefore(typeof(MeshUpdateSystemUInt32))]
    public class MapSystemGroup : ComponentSystemGroup {

        public ArchetypeContainer Archetypes { get; private set; }
        protected override void OnCreate() {
            base.OnCreate();
            Archetypes = new ArchetypeContainer(EntityManager);
        }


        public struct ArchetypeContainer {
            public readonly EntityArchetype Map;

            public readonly EntityArchetype MapRenderLayer;

            public readonly EntityArchetype MapTileHighlighter;

            public readonly EntityArchetype MapBody;

            public ArchetypeContainer(EntityManager manager) {

                //Map = manager.CreateArchetype(ComponentType.ChunkComponent<MapHighlightDirtyLayerData>(), typeof(MapHighlightData), typeof(MapData));
                Map = manager.CreateArchetype(typeof(MapData),
                typeof(MapHighlightState),
                typeof(MapCollisionState),
                typeof(MapLayerRenderer),
                typeof(MapRenderInfo));
                MapRenderLayer = manager.CreateArchetype(
                typeof(MaterialColor),
                typeof(MapElement),
                typeof(RenderMesh),
                typeof(RenderBounds),
                typeof(LocalToWorld),
                typeof(MeshIndexUpdate),
                typeof(MeshIndexUpdateData32));
                MapTileHighlighter = manager.CreateArchetype(typeof(MapElement), typeof(HighlightTile), typeof(HighlightSystemTile));
                MapBody = manager.CreateArchetype(typeof(MapBody), typeof(MapElement));
            }

        }
    }

    [UpdateInGroup(typeof(MapSystemGroup))]
    public class MapHighlightSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(MapSystemGroup))]
    public class MapBodyManagementSystemGroup : ComponentSystemGroup { }
}