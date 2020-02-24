using System;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Battle
{
    public struct MapData : IComponentData
    {
        public BlobAssetReference<MapBlob> map;

        public ushort Width { get => map.Value.width; }

        public ushort Length { get => map.Value.length; }

        public int Elevation { get => map.Value.elevation; }

        public ref BlobString Name { get => ref map.Value.name; }

        public ref BlobArray<BlittableTile> Tiles { get => ref map.Value.tiles; }

        public ref BlobArray<MapBlobSpawnGroup> SpawnGroups { get => ref map.Value.spawnGroups; }


    }

    public struct MapRenderData : IComponentData
    {
        public float tileSize;

        public float elevationStep;
    }

    /// <summary>
    /// Marker Component to signal if an entity is rendering a specific MapLayer. BASE is ignored, as it's handled by the MapRootRenderLayer.
    /// </summary>
    public struct RenderMap : IComponentData
    {
        public MapLayer layer;
    }


    /// <summary>
    /// Buffer Element Data to signal which tiles to highlight. Adding these to the map entity will highlight tiles on the map using the specified layer. Tiles will remain highlighted until the tile is removed.
    /// </summary>
    public struct HighlightTile : IBufferElementData
    {
        public Point point;
        public MapLayer layer;

    }
    /// <summary>
    /// Component to represent movable entities on the map. Point holds the current coordinate they are on.
    /// </summary>
    public struct MapBody : IComponentData
    {
        public Point point;
        //Measured in tiles/second
        public float speed;
    }

    /// <summary>
    /// Component to signal MapBody entities to move. Add this to a MapBody, and a system will transition the body to the point. 
    /// This Component is removed when <c>MapBody.point==MapBodyTranslation.transitionPoint=MapBodyTranslation.destinationPoint</c>
    /// </summary>
    public struct MapBodyTranslation : IComponentData
    {
        /// <summary>
        /// Represents the final point the MapBody should be on before removal.
        /// </summary>
        public Point point;
    }
    public struct MapBodyTranslationPoint : IBufferElementData, IComparable<MapBodyTranslationPoint>
    {
        public Point point;
        public int order;

        public float completion;

        public int CompareTo(MapBodyTranslationPoint other)
        {
            return order.CompareTo(other.order);

        }
    }
    /// <summary>
    /// Component to signal to lock the entity onto it's point. Useful for initialization
    /// </summary>
    public struct MapBodySnap : IComponentData { }
    /// <summary>
    /// Component to represent tile effects on the map. Point refers to the location of the tile.
    /// </summary>
    public struct MapTileEffect : IBufferElementData
    {
        public Point point;
    }


}