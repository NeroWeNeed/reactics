using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Map {
    public enum MapBodyAnchor {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        Bottom,
        BottomRight
    }
    public static class MapCommons {

        public static readonly float MAX_CAMERA_DISTANCE_PADDING = 5f;
        public static int IndexOf<T, S>(this IMap<T, S> map, int x, int y) where T : IMapTile where S : IMapSpawnGroup {
            return (y * map.Width) + x;
        }
        public static int IndexOf<T, S>(this IMap<T, S> map, ushort x, ushort y) where T : IMapTile where S : IMapSpawnGroup {
            return (y * map.Width) + x;
        }
        public static int IndexOf<T, S>(this IMap<T, S> map, Point point) where T : IMapTile where S : IMapSpawnGroup {
            return (point.y * map.Width) + point.x;
        }
        public static int IndexOf(Point point, ushort width) {
            return (point.y * width) + point.x;
        }
        public static float3 GetCenterInWorldCoordinates<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup {
            return new float3(map.Width * tileSize / 2f, map.Elevation * elevationStep, map.Length * tileSize / 2f);
        }
        public static float GetMaxDistance<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup {
            return math.sqrt(math.pow(map.Width * tileSize / 2f, 2) + math.pow(map.Length * tileSize / 2f, 2)) + MAX_CAMERA_DISTANCE_PADDING;

        }

    }


    public static class MapMeshCommons {
        public static readonly uint[] TILE_INDICES = new uint[] { 0, 2, 1, 2, 3, 1 };
        public static Mesh CreateMesh<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f, Mesh mesh = null) where T : IMapTile where S : IMapSpawnGroup {

            if (mesh == null)
                mesh = new Mesh();
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];

            meshData.SetIndexBufferParams(map.Width * map.Length * 6, IndexFormat.UInt32);

            meshData.SetVertexBufferParams(map.Width * map.Length * 4, new VertexAttributeDescriptor[]
            {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.Normal,VertexAttributeFormat.Float32,3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2)
            });
            meshData.subMeshCount = MapLayers.Count;
            var vertexData = meshData.GetVertexData<VertexData>();
            var indexData = meshData.GetIndexData<uint>();
            for (ushort y = 0; y < map.Length; y++) {
                for (ushort x = 0; x < map.Width; x++) {
                    for (byte i = 0; i < 4; i++) {
                        vertexData[(y * map.Width + x) * 4 + i] = new VertexData
                        {
                            position = new float3((x + (i & 0b0001)) * tileSize, map.GetTile(x, y).Elevation * elevationStep, (y + ((i >> 1) & 0b0001)) * tileSize),
                            normal = new float3(0, 1, 0),
                            uv = new float2(((float)(x + (i & 0b0001))) / map.Width, ((float)y + ((i >> 1) & 0b0001)) / map.Length)
                        };
                    }
                    for (byte j = 0; j < 6; j++) {
                        indexData[(y * map.Width + x) * 6 + j] = (ushort)(((y * map.Width + x) * 4) + TILE_INDICES[j]);
                    }
                }
            }

            meshData.SetSubMesh(0, new SubMeshDescriptor(0, 6 * map.Width * map.Length)
            {
                firstVertex = 0,
                vertexCount = 4 * map.Width * map.Length
            });

            meshData.subMeshCount = MapLayers.Count;

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static bool GetPoint<T, S>(this IMap<T, S> map, float3 worldCoordinates, out Point point, bool ignoreHeight = true, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup => GetPoint(map, worldCoordinates, float3.zero, out point, ignoreHeight, tileSize, elevationStep);
        public static bool GetPoint<T, S>(this IMap<T, S> map, float3 worldCoordinates, float3 mapOffset, out Point point, bool ignoreHeight = true, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup {
            if (worldCoordinates.x < mapOffset.x || worldCoordinates.x >= mapOffset.x + map.Width * tileSize || worldCoordinates.z < mapOffset.z || worldCoordinates.z >= mapOffset.z + map.Length * tileSize) {
                point = default;
                return false;
            }

            var coords = worldCoordinates - mapOffset;
            var p = new Point((ushort)(coords.x / tileSize), (ushort)(coords.z / tileSize));
            if (!ignoreHeight && map.GetTile(p).Elevation * elevationStep != worldCoordinates.y - mapOffset.y) {
                point = default;
                return false;
            }
            point = p;
            return true;

        }

        public static bool UpdateRenderLayerBuffer(IMapHighlightInfo info, ushort mapWidth, MapLayer layer, List<int> buffer, List<uint> processedBuffer) => UpdateRenderLayerBuffer(info, mapWidth, (ushort)layer, buffer, processedBuffer);
        public static bool UpdateRenderLayerBuffer(IMapHighlightInfo info, ushort mapWidth, ushort layer, List<int> buffer, List<uint> processedBuffer) {
            if ((info.Dirty & layer) == 0)
                return false;
            var enumerator = info.GetPoints(layer);
            processedBuffer.Clear();
            while (enumerator.MoveNext()) {

                uint j = (uint)MapCommons.IndexOf(enumerator.Current, mapWidth);
                if (processedBuffer.Contains(j))
                    continue;
                for (byte k = 0; k < 6; k++)
                    buffer.Add((int)(j * 4 + TILE_INDICES[k]));
                processedBuffer.Add(j);
            }
            return true;
        }

        private struct VertexData {
            public float3 position;
            public float3 normal;
            public float2 uv;
        }
    }

    public interface IMapHighlightInfo {
        ushort Dirty { get; }

        IEnumerator<Point> GetPoints(ushort layer);

        IEnumerator<Point> GetPoints(MapLayer layer);
    }

    public struct MapHighlightInfo : IMapHighlightInfo {

        public ushort Dirty => throw new NotImplementedException();

        public IEnumerator<Point> GetPoints(ushort layer) {
            throw new NotImplementedException();
        }

        public IEnumerator<Point> GetPoints(MapLayer layer) {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct MapBodyTarget {
        public Entity entity;

        public MapBody mapBody;
    }

    [Flags]
    public enum MapLayer {
        Base = 0,
        PlayerMove = 1,
        PlayerAttack = 2,
        PlayerSupport = 4,
        PlayerAll = 8,
        EnemyMove = 16,
        EnemyAttack = 32,
        EnemySupport = 64,
        EnemyAll = 128,
        AllyMove = 256,
        AllyAttack = 512,
        AllySupport = 1024,
        AllyAll = 2048,
        Hover = 4096,
        //Placeholders
        UtilityOne = 8192,
        UtilityTwo = 16384


    }


    public static class MapLayers {

        private static readonly ushort[] Values = { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };


        public static readonly int Count = Values.Length;
        public static ushort Get(int index) => Values[index];
        public static ushort Get(byte index) => Values[index];
        public static ushort Get(ushort index) => Values[index];

        public static int IndexOf(MapLayer layer) => IndexOf((ushort)layer);
        public static int IndexOf(ushort layer) => Array.IndexOf(Values, layer);

        public static EnumDictionary<MapLayer, Color> CreateDefaultColorMap() => new EnumDictionary<MapLayer, Color>() {
            { MapLayer.Base, GeneralCommons.ParseColor("C0C0C0")},
            { MapLayer.PlayerMove, GeneralCommons.ParseColor("41EAD4") },
            { MapLayer.PlayerAttack , GeneralCommons.ParseColor("41EAD4") },
            { MapLayer.PlayerSupport, GeneralCommons.ParseColor("41EAD4") },
            { MapLayer.PlayerAll , GeneralCommons.ParseColor("41EAD4") },
            { MapLayer.EnemyMove , GeneralCommons.ParseColor("F71735") },
            { MapLayer.EnemyAttack, GeneralCommons.ParseColor("F71735") },
            { MapLayer.EnemySupport, GeneralCommons.ParseColor("F71735") },
            { MapLayer.EnemyAll , GeneralCommons.ParseColor("F71735") },
            { MapLayer.AllyMove , GeneralCommons.ParseColor("95E06C") },
            { MapLayer.AllyAttack, GeneralCommons.ParseColor("95E06C") },
            { MapLayer.AllySupport, GeneralCommons.ParseColor("95E06C") },
            { MapLayer.AllyAll , GeneralCommons.ParseColor("95E06C") },
            { MapLayer.Hover ,GeneralCommons.ParseColor("FF9F1C") },
            { MapLayer.UtilityOne , GeneralCommons.ParseColor("FDFFFC") },
            { MapLayer.UtilityTwo, GeneralCommons.ParseColor("011627") }
        };

    }







}