using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Reactics.Battle.Map
{
    /// <summary>
    /// MapBlob Asset component for ease of access to map properties.
    /// </summary>
    public struct MapData : IMap<MapBlobTile, MapBlobSpawnGroup>, IComponentData
    {
        public BlobAssetReference<MapBlob> value;

        public MapData(BlobAssetReference<MapBlob> value)
        {
            this.value = value;
        }

        public string Name => value.Value.Name;

        public ushort Width => value.Value.Width;

        public ushort Length => value.Value.Length;

        public int TileCount => value.Value.TileCount;

        public int SpawnGroupCount => value.Value.SpawnGroupCount;

        public int Elevation => value.Value.Elevation;

        public Point GetSpawnGroupPoint(int spawnGroup, int index) => GetSpawnGroupPoint(spawnGroup, index);

        public int GetSpawnGroupPointCount(int spawnGroup) => value.Value.GetSpawnGroupPointCount(spawnGroup);

        public MapBlobTile GetTile(Point point) => value.Value.GetTile(point);

        public MapBlobTile GetTile(ushort x, ushort y) => value.Value.GetTile(x, y);

        public MapBlobTile GetTile(int x, int y) => value.Value.GetTile(x, y);
    }

    public struct MapElement : IComponentData
    {
        /// <summary>
        /// Points to target map entity
        /// </summary>
        public Entity value;
    }

    public struct MapLayerRenderer : IBufferElementData
    {
        /// <summary>
        /// Points to target renderer entity
        /// </summary>
        public Entity entity;
        public MapLayer layer;
    }


    public struct MapRenderInfo : IComponentData
    {
        public uint baseIndexCount;
        public float tileSize;
        public float elevationStep;
    }
	
	public struct MapTileEffect : IBufferElementData
    {
        public Point point;
        public Effect effect;
    }

}