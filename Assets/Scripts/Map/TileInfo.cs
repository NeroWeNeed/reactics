using System;
using Reactics.Util;

namespace Reactics.Battle
{
    public struct TileInfo : IEquatable<TileInfo> {
        public readonly Point point;
        public readonly Tile tile;
        public TileInfo(Point point, Tile tile)
        {
            this.point = point;
            this.tile = tile;
        }

        bool IEquatable<TileInfo>.Equals(TileInfo other)
        {
            return other.point.Equals(point) && other.tile.Equals(tile);
        }
    }
}