using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle.Components
{
    public struct MapHeader : IComponentData
    {
        public NativeString128 name;
        public ushort width;
        public ushort length;
        public int elevation;
        public MapHeader(string name, ushort width, ushort length, int elevation)
        {
            this.name = new NativeString128(name);
            this.width = width;
            this.length = length;
            this.elevation = elevation;
        }
    }
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
    public struct MapSpawnGroup : IBufferElementData
    {
        public readonly Point point;
        public readonly int group;
        public MapSpawnGroup(Point point, int group)
        {
            this.point = point;
            this.group = group;
        }
    }
    public struct MapRenderData : IComponentData
    {
        public Entity mapEntity;
        public int lastVersion;
        public BlittableBool isRenderingBase;
    }

    public struct MapLayerRender : IComponentData
    {
        public Entity mapRenderDataEntity;
        public MapLayer layer;
    }

    public struct HighlightTile : IBufferElementData
    {
        public Point point;
        public MapLayer layer;
    }

}