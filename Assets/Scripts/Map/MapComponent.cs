using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle
{
    public struct MapComponent : IMap, IComponentData
    {


        [SerializeField]
        private NativeString128 name;
        public string Name => name.ToString();

        [SerializeField]
        private ushort width;
        public ushort Width => width;
        [SerializeField]
        private ushort length;
        public ushort Length => length;
        [SerializeField]
        private int elevation;
        public int Elevation => elevation;

        public int TileCount => throw new NotImplementedException();

        

        //[SerializeField]
        //private NativeArray<Tile> tiles;
        //public int TileCount => tiles.Length;

        [SerializeField]
        private NativeArray<SpawnGroup> spawnGroups;

        public int SpawnGroupCount => spawnGroups.Length;

        public Tile this[Point point] => GetTile(point);

        public Tile this[ushort x, ushort y] => GetTile(x, y);
        public MapComponent(string name, ushort width, ushort length, int elevation, Tile[] tiles, SpawnGroup[] spawnGroups)
        {
            this.name = new NativeString128(name);
            this.width = width;
            this.length = length;
            this.elevation = elevation;

            
            //this.tiles = new NativeArray<Tile>(tiles, Allocator.Persistent);
            this.spawnGroups = new NativeArray<SpawnGroup>(spawnGroups, Allocator.Persistent);

        }
        public SpawnGroup GetSpawnGroup(int index)
        {
            throw new NotImplementedException();
            //return spawnGroups[index];
        }
        public Tile GetTile(ushort x, ushort y)
        {
            throw new NotImplementedException();
            //return tiles[(y * Width) + x];
        }
        public Tile GetTile(Point point)
        {
            throw new NotImplementedException();
            //return tiles[(point.y * Width) + point.x];
        }
    }
}