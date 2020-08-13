using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Map {


    [CreateAssetMenu(fileName = "MapAsset", menuName = "Reactics/Map", order = 0)]
    public class MapAsset : ScriptableObject, IMap<MapAssetTile, MapAssetSpawnGroup>, ISerializationCallbackReceiver {


        [SerializeField]
        private new string name;
        public string Name { get => name; set => name = value; }
        [SerializeField]
        private ushort width;

        public ushort Width
        {
            get => width; set
            {
                UpdateTiles(width, length, value, length);
                width = value;
            }
        }
        [SerializeField]
        private ushort length;
        public ushort Length
        {
            get => length;
            set
            {
                UpdateTiles(width, length, width, value);
                length = value;
            }

        }

        [SerializeField]
        private int elevation;
        public int Elevation { get => elevation; set => elevation = value; }
        [SerializeField]
        private MapAssetTile[] tiles;

        public int TileCount => tiles != null ? tiles.Length : 0;
        [SerializeField]
        private MapAssetSpawnGroup[] spawnGroups;
        public int SpawnGroupCount => spawnGroups != null ? spawnGroups.Length : 0;

        public MapAssetTile GetTile(Point point) => tiles[(point.y * Width) + point.x];

        public MapAssetTile GetTile(ushort x, ushort y) => tiles[(y * Width) + x];

        public MapAssetTile GetTile(int x, int y) => tiles[(y * Width) + x];


        public BlobAssetReference<MapBlob> CreateBlob() {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref MapBlob mapBlob = ref builder.ConstructRoot<MapBlob>();

            builder.AllocateString(ref mapBlob.NameNative, name);
            mapBlob.Width = Width;
            mapBlob.Length = Length;
            mapBlob.Elevation = Elevation;
            BlobBuilderArray<MapBlobTile> tiles = builder.Allocate(ref mapBlob.tiles, Width * Length);
            int index;
            for (ushort x = 0; x < Width; x++) {
                for (ushort y = 0; y < Length; y++) {
                    index = this.IndexOf(x, y);
                    tiles[index] = MapAssetTile.CreateBlobTile(this.tiles[index]);
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

        public Point GetSpawnGroupPoint(int spawnGroup, int index) {
            return spawnGroups[spawnGroup][index];
        }

        public int GetSpawnGroupPointCount(int spawnGroup) {
            return spawnGroups[spawnGroup].Count;
        }

        public void OnBeforeSerialize() {
            UpdateTiles(-1, -1, width, length);

            //this.tiles.FillNull(new MapAssetTile());
        }
        public void OnAfterDeserialize() {
            UpdateTiles(-1, -1, width, length);
            //this.tiles.FillNull(new MapAssetTile());

        }
        public Mesh UpdateMesh(float tileSize = 1f, float elevationStep = 0.25f, int oldWidth = -1, int oldLength = -1, Mesh mesh = null) {
            UpdateTiles(oldWidth, oldLength, width, length);
            this.CreateMesh(tileSize, elevationStep, mesh);
            return mesh;
        }
        private bool UpdateTiles(int oldWidth, int oldLength, ushort newWidth, ushort newLength) {
            if (width * length != tiles.Length) {
                var newTiles = new MapAssetTile[width * length];
                if (oldWidth > -1 && oldLength > -1) {
                    if (oldWidth != width || oldLength != length) {

                        if (tiles != null && newTiles.Length > 0) {
                            //Copy By row to preserve locations
                            var rowSize = width > oldWidth ? oldWidth : width;
                            var rowCount = length > oldLength ? oldLength : length;

                            for (int i = 0; i < rowCount; i++) {
                                var remaining = tiles.Length - oldWidth * i;
                                if (remaining > 0)
                                    Array.Copy(tiles, oldWidth * i, newTiles, width * i, rowSize > remaining ? remaining : rowSize);
                                else
                                    break;
                            }

                        }
                        tiles = newTiles;
                    }
                }
                else
                    Array.Copy(tiles, newTiles, newTiles.Length > tiles.Length ? tiles.Length : newTiles.Length);
                tiles = newTiles;

                return true;
            }
            return false;
        }





    }
    [Serializable]
    public struct MapAssetTile : IMapTile {

        [SerializeField]
        private short elevation;
        public short Elevation { get => elevation; private set => elevation = value; }
        [SerializeField]
        private bool inaccessible;
        public bool Inaccessible { get => inaccessible; set => inaccessible = value; }

        public static MapBlobTile CreateBlobTile(MapAssetTile tile) => new MapBlobTile
        {
            Elevation = tile.Elevation
        };
    }
    [Serializable]
    public class MapAssetSpawnGroup : IMapSpawnGroup {
        private Point[] points;
        public Point this[int index] => points[index];

        public int Count => points.Length;
    }
    public class OriginalStateData {
        public ushort width, length;
    }


}