using Reactics.Core.Commons;
using Unity.Entities;

namespace Reactics.Core.Map {

    public struct MapBlob : IMap<MapBlobTile, MapBlobSpawnGroup> {

        /// <summary>
        /// Provides access to the name in unmanaged context
        /// </summary>
        public BlobString NameNative;
        /// <summary>
        /// Provides access to the name in managed context
        /// </summary>
        public string Name => NameNative.ToString();



        public ushort Width { get; set; }

        public ushort Length { get; set; }

        public int Elevation { get; set; }

        public BlobArray<MapBlobTile> tiles;
        public int TileCount => tiles.Length;

        public BlobArray<MapBlobSpawnGroup> spawnGroups;
        public int SpawnGroupCount => spawnGroups.Length;

        public MapBlobTile GetTile(Point point) => tiles[(point.y * Width) + point.x];

        public MapBlobTile GetTile(ushort x, ushort y) => tiles[(y * Width) + x];

        public MapBlobTile GetTile(int x, int y) => tiles[(y * Width) + x];

        public Point GetSpawnGroupPoint(int spawnGroup, int index) => spawnGroups[spawnGroup][index];

        public int GetSpawnGroupPointCount(int spawnGroup) => spawnGroups[spawnGroup].Count;
    }
    public struct MapBlobTile : IMapTile {
        public short Elevation { get; set; }

        private BlittableBool _inaccessible;
        public bool Inaccessible { get => _inaccessible; set => _inaccessible = value; }
    }

    public struct MapBlobSpawnGroup : IMapSpawnGroup {
        private BlobArray<Point> points;

        public MapBlobSpawnGroup(BlobArray<Point> points) {
            this.points = points;
        }

        public Point this[int index] => points[index];

        public int Count => points.Length;
    }
}