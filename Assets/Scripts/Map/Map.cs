using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Collections;

namespace Reactics.Battle.Map
{
    [CreateAssetMenu(fileName = "Map", menuName = "Reactics/Map", order = 0)]
    public class Map : ScriptableObject, IEnumerable
    {
        [SerializeField]
        private new string name;

        public string Name => name;

        [SerializeField]
        private int width;

        public int Width => width;

        [SerializeField]
        private int height;

        public int Height => height;

        [SerializeField]
        private Tile[] tiles;

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


    }
}
