using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Collections;

namespace Reactics.Battle
{
    [CreateAssetMenu(fileName = "Map", menuName = "Reactics/Map", order = 0)]
    public class Map : ScriptableObject, IEnumerable
    {
        [SerializeField]
        private new string name = "Untitled Map";

        public string Name => name;

        [SerializeField]
        private int width = 8;

        public int Width
        {
            get => width;
            private set
            {
                SetSize(value, length);
            }
        }

        [SerializeField]
        private int length = 8;

        public int Length
        {
            get => length;
            private set
            {
                SetSize(width, value);
            }
        }


        [SerializeField]
        private Tile[] tiles;

        private void Awake()
        {
            SetSize(width, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int x, int y)
        {
            return (y * Width) + x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(Vector2Int coordinates)
        {
            return (coordinates.y * Width) + coordinates.x;
        }
        public int IndexOf(Tile tile)
        {
            return Array.IndexOf(tiles, tile);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2Int CoordinatesOf(int index)
        {
            return new Vector2Int(XCoordinateOf(index), YCoordinateOf(index));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int XCoordinateOf(int index)
        {
            return index % Width;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int YCoordinateOf(int index)
        {
            return index / Width;
        }


        public IEnumerator GetEnumerator()
        {
            return tiles.GetEnumerator();
        }

        public Tile this[int x, int y] => tiles[IndexOf(x, y)];
        public Tile this[Vector2Int coordinates] => tiles[IndexOf(coordinates)];
        private void SetSize(int newWidth, int newLength)
        {
            if (newWidth <= 0 || newLength <= 0)
                throw new UnityException("Map Width and Length must be larger than 0");

            if (width == newWidth && newLength == length)
                return;
            Tile[] newTiles = new Tile[newWidth * newLength];
            int span = newLength > length ? length : newLength;
            for (int i = 0; i < span; i++)
            {
                Array.Copy(tiles, i * width, newTiles, i * newWidth, span);
            }
            tiles = newTiles;
            width = newWidth;
            length = newLength;
        }
        public bool Validate() {
            return width > 0 && length > 0;
        }

    }
}
