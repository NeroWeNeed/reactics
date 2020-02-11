using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle
{
    /// <summary>
    /// Represents the header data of a map. 
    /// </summary>
    public struct MapHeader : IComponentData
    {
        /// <summary>
        /// Map name. Max Length is 126 characters.
        /// </summary>
        public NativeString128 name;
        /// <summary>
        /// Represents the Map width in Map Coordinates.
        /// </summary>
        public ushort width;
        /// <summary>
        /// Represents the Map length in Map Coordinates.
        /// </summary>
        public ushort length;
        /// <summary>
        /// Represents the base elevation. that every tile uses to derive their height. By default this value is 0.
        /// </summary>
        public int elevation;
        public MapHeader(string name, ushort width, ushort length, int elevation)
        {
            this.name = new NativeString128(name);
            this.width = width;
            this.length = length;
            this.elevation = elevation;
        }
    }
    /// <summary>
    /// Represents A Map tile. Map tiles are stored in a way that the index can be calculated via <c>index = y * width + x</c>.
    /// Conversely, the tile coordinates can be calculated by <c>x = index % width</c> and <c>y = index / width </c>, where y is truncated.
    /// </summary>
    public struct MapTile : IBufferElementData
    {
        public readonly Tile Value;
        public MapTile(Tile value)
        {
            Value = value;
        }
        public static explicit operator MapTile(Tile value)
        {
            return new MapTile(value);
        }
        public static implicit operator Tile(MapTile value)
        {
            return value.Value;
        }
    }
    /// <summary>
    /// Represents a spawn point for a spawn group. 
    /// </summary>
    public struct MapSpawnGroupPoint : IBufferElementData
    {
        public readonly Point point;
        public readonly int group;
        public MapSpawnGroupPoint(Point point, int group)
        {
            this.point = point;
            this.group = group;
        }
    }
    /// <summary>
    /// Marker Component to signal if an entity is rendering a specific MapLayer. BASE is ignored, as it's handled by the MapRootRenderLayer.
    /// </summary>
    public struct MapRenderLayer : IComponentData
    {
        public MapLayer layer;
    }
    /// <summary>
    /// Marker component to signal which entity is responsible for rendering the base layer. 
    /// </summary>
    public struct MapRootRenderLayer : IComponentData
    {
        public int lastVersion;
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
    }
    /// <summary>
    /// Component to represent tile effects on the map. Point refers to the location of the tile.
    /// </summary>
    public struct MapTileEffect : IBufferElementData
    {
        public Point point;
    }


}