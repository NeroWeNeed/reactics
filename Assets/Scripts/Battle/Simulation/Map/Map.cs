using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle
{
    public interface IMap<Tile, SpawnGroup> where Tile : IMapTile where SpawnGroup : IMapSpawnGroup
    {


        ushort Width { get; }

        ushort Length { get; }

        int Elevation { get; }

        ref Tile this[Point point]
        {
            get;
        }
        ref Tile this[int x, int y]
        {
            get;
        }
        ref Tile this[ushort x, ushort y]
        {
            get;
        }

        bool GetTile(Point point, out Tile mapTile);

        int TileCount { get; }

        ref SpawnGroup this[int index] { get; }
        int SpawnGroupCount { get; }

    }

    public interface IMapTile
    {
        int Elevation { get; }
        bool Inaccessible { get; }
    }

    public interface IMapSpawnGroup
    {
        Point this[int index] { get; }

        int Count { get; }
    }

    // Asset


    [CreateAssetMenu(fileName = "Map", menuName = "Reactics/Map", order = 0)]
    public class Map : ScriptableObject, IEnumerable, ISerializationCallbackReceiver, IMap<Tile, SpawnGroup>
    {
        [SerializeField]
        private string _name = "Untitled Map";


        public string Name => _name;


        [SerializeField]
        private ushort _width = 8;


        public ushort Width
        {

            get => _width;
            private set
            {
                SetSize(value, _length);
            }
        }

        [SerializeField]
        private ushort _length = 8;

        public ushort Length
        {
            get => _length;
            private set
            {
                SetSize(_width, value);
            }
        }


        [SerializeField]
        private Tile[] _tiles;

        public Tile[] tiles { get => _tiles; set => _tiles = value; }

        [SerializeField]
        private int _elevation;
        public int Elevation
        {
            get => _elevation;
        }

        [SerializeField]
        private SpawnGroup[] _spawnGroups;

        public SpawnGroup[] spawnGroups { get => _spawnGroups; private set => _spawnGroups = value; }

        public int TileCount => tiles.Length;

        public int SpawnGroupCount => spawnGroups.Length;

        public ref SpawnGroup this[int index] => ref spawnGroups[index];


        private void Awake()
        {
            if (tiles == null || tiles.Length == 0)
                tiles = new Tile[Length * Width];
            else
            {
                SetSize(Width, Length, true);
            }

        }

        /// <summary>
        /// Calculates the index associated with the x and y coordinate provided.
        /// </summary>
        /// <param name="x">The x Coordinate</param>
        /// <param name="y">The y Coordinate</param>
        /// <return> The Index associated with the provided coordinates </return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(ushort x, ushort y)
        {
            return (y * Width) + x;
        }
        /// <summary>
        /// Calculates the index associated with the point provided.
        /// </summary>
        /// <param name="point">The Tile referencing the Point</param>
        /// <return> The Index associated with the provided coordinates</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(Point point)
        {
            return (point.y * Width) + point.x;
        }
        /// <summary>
        /// Searches for the index the tile provided is located at.
        /// </summary>
        /// <param name="tile">The Tile to search</param>
        /// <return> The Index of the tile provided. -1 if the tile doesn't exist in this Map.</return>
        public int IndexOf(Tile tile)
        {
            return Array.IndexOf(tiles, tile);
        }
        /// <summary>
        /// Calculates the Point from the index provided.
        /// </summary>
        /// <param name="index">The index of the Point</param>
        /// <return> The point the index is associated with.</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point PointOf(int index)
        {
            return new Point(XPointOf(index), YPointOf(index));
        }
        /// <summary>
        /// Calculates the X coordinate of the index provided.
        /// </summary>
        /// <param name="index">The index of the Point</param>
        /// <return> The x coordinate of the Point the index is associated with.</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort XPointOf(int index)
        {
            return (ushort)(index % Width);
        }
        /// <summary>
        /// Calculates the Y coordinate of the index provided.
        /// </summary>
        /// <param name="index">The index of the Point</param>
        /// <return> The y coordinate of the Point the index is associated with.</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort YPointOf(int index)
        {
            return (ushort)(index / Width);
        }


        public IEnumerator GetEnumerator()
        {
            return tiles.GetEnumerator();
        }

        public ref Tile this[ushort x, ushort y] => ref tiles[IndexOf(x, y)];
        public ref Tile this[int x, int y] => ref tiles[IndexOf((ushort)x, (ushort)y)];
        public ref Tile this[Point point] => ref tiles[IndexOf(point)];
        private void SetSize(ushort newWidth, ushort newLength, bool force = false)
        {
            if (newWidth <= 0 || newLength <= 0)
                throw new UnityException("Map Width and Length must be larger than 0");

            if (Width == newWidth && newLength == Length && !force)
                return;
            Tile[] newTiles = new Tile[newWidth * newLength];
            int span = newLength > Length ? Length : newLength;
            Array.Copy(tiles, newTiles, tiles.Length > newTiles.Length ? newTiles.Length : tiles.Length);

            tiles = newTiles;
            Width = newWidth;
            Length = newLength;
        }



        public MapData CreateComponent() => new MapData { map = CreateBlob() };
        public BlobAssetReference<MapBlob> CreateBlob()
        {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref MapBlob mapBlob = ref builder.ConstructRoot<MapBlob>();
            builder.AllocateString(ref mapBlob.name, name);
            mapBlob.width = Width;
            mapBlob.length = Length;
            mapBlob.elevation = Elevation;
            BlobBuilderArray<BlittableTile> tiles = builder.Allocate(ref mapBlob.tiles, Width * Length);
            int index;
            for (ushort x = 0; x < Width; x++)
            {
                for (ushort y = 0; y < Length; y++)
                {
                    index = IndexOf(x, y);
                    tiles[index] = new BlittableTile
                    {
                        elevation = this.tiles[index].elevation,
                        inaccessible = this.tiles[index].inaccessible
                    };
                }
            }
/*             BlobBuilderArray<MapBlobSpawnGroup> spawnGroups = builder.Allocate(ref mapBlob.spawnGroups, this.spawnGroups.Length);

            for (int i = 0; i < spawnGroups.Length; i++)
            {
                if (this.spawnGroups[i].points.Length > 0)
                {
                    BlobBuilderArray<Point> spawnGroupPoints = builder.Allocate(ref mapBlob.spawnGroups[i].points, this.spawnGroups[i].points.Length);

                    for (int j = 0; j < spawnGroupPoints.Length; j++)
                    {

                        spawnGroupPoints[j] = this.spawnGroups[i].points[j];
                    }
                }
            } */
            BlobAssetReference<MapBlob> blob = builder.CreateBlobAssetReference<MapBlob>(Allocator.Persistent);
            builder.Dispose();
            return blob;

        }

        public void OnBeforeSerialize()
        {
            if (tiles.Length != Width * Length)
            {
                SetSize(Width, Length, true);
            }

        }

        public void OnAfterDeserialize()
        {

        }

        public Tile GetTile(ushort x, ushort y)
        {
            return tiles[IndexOf(x, y)];
        }

        public Tile GetTile(Point point)
        {
            return tiles[IndexOf(point)];
        }

        public SpawnGroup GetSpawnGroup(int index)
        {
            return spawnGroups[index];
        }

        public bool GetTile(Point point, out Tile mapTile)
        {
            int index = (point.y * Width) + point.x;
            if (index < 0 || index >= tiles.Length)
            {
                mapTile = default;
                return false;
            }
            else
            {
                mapTile = tiles[(point.y * Width) + point.x];
                return true;
            }
        }
    }

    // Blobs

    public struct MapBlob : IMap<BlittableTile, MapBlobSpawnGroup>
    {
        public BlobString name;

        public ushort width;

        public ushort length;

        public int elevation;

        public BlobArray<BlittableTile> tiles;

        public BlobArray<MapBlobSpawnGroup> spawnGroups;

        public ref MapBlobSpawnGroup this[int index] => ref spawnGroups[index];

        public ref BlittableTile this[int x, int y] => ref tiles[(y * width) + x];

        public ref BlittableTile this[ushort x, ushort y] => ref tiles[(y * width) + x];

        public ref BlittableTile this[Point point] => ref tiles[(point.y * width) + point.x];

        public ushort Width => width;

        public ushort Length => length;

        public int Elevation => elevation;

        public int TileCount => tiles.Length;

        public int SpawnGroupCount => spawnGroups.Length;

        public bool GetTile(Point point, out BlittableTile mapTile)
        {
            int index = (point.y * width) + point.x;
            if (index < 0 || index >= tiles.Length)
            {
                mapTile = default;
                return false;
            }
            else
            {
                mapTile = tiles[(point.y * width) + point.x];
                return true;
            }

        }
    }

    public struct MapBlobSpawnGroup : IMapSpawnGroup
    {
        public BlobArray<Point> points;

        public Point this[int index] => points[index];

        public int Count => points.Length;
    }

}