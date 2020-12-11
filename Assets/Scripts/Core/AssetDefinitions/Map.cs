using System;
using Reactics.Core.Commons;
using Unity.Entities;

namespace Reactics.Core.AssetDefinitions {
    [Serializable]
    public struct MapData {
        public ushort id;
        public ushort width;
        public ushort length;
        public sbyte baseHeight;
        public BlobArray<SpawnGroup> spawnGroups;
        public bool IsValid { get => tiles.Length == width * length && width > 0 && length > 0; }
        public BlobArray<TileData> tiles;
        /// <summary>
        /// To minimize on pre-game setup, spawn points are 3x3 grids centered on pre-selected points. Map Spawn Groups are randomly selected from the list to determine which two points to use. 
        /// </summary>
        [Serializable]
        public struct SpawnGroup {
            public Point sideA;
            public Point sideB;
        }
        [Serializable]
        public struct TileData {
            /// <summary>
            /// sbyte.MINVALUE = Hole
            /// sbyte.MAXVALUE = Wall
            /// </summary>
            public float height;
            /// <summary>
            /// Used for generating meshes & texture data.
            /// </summary>
            public ushort textureHint;
            public ushort structureHint;
            public float movementMultiplier;
            public DataReference<TileEffectData> effect;
        }
    }
}