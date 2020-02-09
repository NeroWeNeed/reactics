using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Collections;
using Reactics.Util;

namespace Reactics.Battle
{
    [CreateAssetMenu(fileName = "Map", menuName = "Reactics/Map", order = 0)]
    public class MapAsset : ScriptableObject, IEnumerable, ISerializationCallbackReceiver, IMap
    {
        [SerializeField]
        private string _name = "Untitled Map";
        

        public new string Name => _name;


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
        private GameObject backgroundHandler;

        [SerializeField]
        private SpawnGroup[] _spawnGroups;

        public SpawnGroup[] spawnGroups { get => _spawnGroups; private set => _spawnGroups = value; }

        public int TileCount => tiles.Length;

        public int SpawnGroupCount => spawnGroups.Length;

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

        public Tile this[ushort x, ushort y] => tiles[IndexOf(x, y)];
        public Tile this[Point point] => tiles[IndexOf(point)];
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



        /// <summary>
        /// Creates a Map Component from the Asset.
        /// </summary>
        /// <return> Map Component</return>
        public MapComponent ToComponent() => ToComponent(out _);
        /// <summary>
        /// Creates a Map Component from the Asset.
        /// </summary>
        /// <param name="map">Output Parameter to store output in. Also returns the Map</param>
        /// <returns>Map Component</returns>
        public MapComponent ToComponent(out MapComponent map)
        {
            Tile[] tiles = new Tile[Width * Length];
            SpawnGroup[] spawnGroups = new SpawnGroup[this.spawnGroups.Length];
            Array.Copy(this.tiles, tiles, tiles.Length);
            Array.Copy(this.spawnGroups, spawnGroups, spawnGroups.Length);
            map = new MapComponent(Name, Width, Length, Elevation, tiles, spawnGroups);
            return map;
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
    }
}
