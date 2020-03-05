using System;
using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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

        public ref BlittableTile this[int x, int y] => ref map.Value.tiles[(y * Width) + x];

        public ref BlittableTile this[ushort x, ushort y] => ref map.Value.tiles[(y * Width) + x];

        public ref BlittableTile this[Point point] => ref map.Value.tiles[(point.y * Width) + point.x];
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
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct MapBody : IComponentData
    {
        public Point point;
        //Measured in tiles/second
        public float speed;

        /// <summary>
        /// Represents the offset from the tile center.
        /// </summary>
        public float3 offset;

        public BlittableBool solid;
    }

    public struct MapBodyMeshOffset : IComponentData
    {
        public float3 offset;

        public MapBodyAnchor anchor;

        public MapBodyMeshOffset(float3 offset, MapBodyAnchor anchor = MapBodyAnchor.BOTTOM_CENTER)
        {
            this.offset = offset;
            this.anchor = anchor;
        }
    }

    public enum MapBodyAnchor
    {
        TOP_NORTH = 0b100001,
        TOP_NORTHEAST = 0b100010,
        TOP_EAST = 0b010010,
        TOP_SOUTHEAST = 0b000010,
        TOP_SOUTH = 0b000001,
        TOP_SOUTHWEST = 0b000000,
        TOP_WEST = 0b010000,
        TOP_NORTHWEST = 0b100000,
        TOP_CENTER = 0b010001,

        MIDDLE_NORTH = 0b100101,
        MIDDLE_NORTHEAST = 0b100110,
        MIDDLE_EAST = 0b010110,
        MIDDLE_SOUTHEAST = 0b000110,
        MIDDLE_SOUTH = 0b000101,
        MIDDLE_SOUTHWEST = 0b000100,
        MIDDLE_WEST = 0b010100,
        MIDDLE_NORTHWEST = 0b100100,
        MIDDLE_CENTER = 0b010101,

        BOTTOM_NORTH = 0b101001,
        BOTTOM_NORTHEAST = 0b101010,
        BOTTOM_EAST = 0b011010,
        BOTTOM_SOUTHEAST = 0b001010,
        BOTTOM_SOUTH = 0b001001,
        BOTTOM_SOUTHWEST = 0b001000,
        BOTTOM_WEST = 0b011000,
        BOTTOM_NORTHWEST = 0b101000,
        BOTTOM_CENTER = 0b011001
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
    public struct MapBodyTranslationStep : IBufferElementData, IComparable<MapBodyTranslationStep>
    {
        public Point point;
        public int order;

        public float completion;

        public int CompareTo(MapBodyTranslationStep other)
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