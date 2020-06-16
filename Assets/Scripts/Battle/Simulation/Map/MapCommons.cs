using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Commons;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Battle.Map
{
    public static class MapCommons
    {

        public static readonly float MAX_CAMERA_DISTANCE_PADDING = 5f;
        public static int IndexOf<T, S>(this IMap<T, S> map, int x, int y) where T : IMapTile where S : IMapSpawnGroup
        {
            return (y * map.Width) + x;
        }
        public static int IndexOf<T, S>(this IMap<T, S> map, ushort x, ushort y) where T : IMapTile where S : IMapSpawnGroup
        {
            return (y * map.Width) + x;
        }
        public static int IndexOf<T, S>(this IMap<T, S> map, Point point) where T : IMapTile where S : IMapSpawnGroup
        {
            return (point.y * map.Width) + point.x;
        }
        public static int IndexOf(Point point, ushort width)
        {
            return (point.y * width) + point.x;
        }
        public static float3 GetCenterInWorldCoordinates<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup
        {
            return new float3(map.Width * tileSize / 2f, map.Elevation * elevationStep, map.Length * tileSize / 2f);
        }
        public static float GetMaxDistance<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup
        {
            return math.sqrt(math.pow(map.Width * tileSize / 2f, 2) + math.pow(map.Length * tileSize / 2f, 2)) + MAX_CAMERA_DISTANCE_PADDING;

        }

    }


    public static class MapMeshCommons
    {
        public static readonly uint[] TILE_INDICES = new uint[] { 0, 2, 1, 2, 3, 1 };
        public static Mesh CreateMesh<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f, Mesh mesh = null) where T : IMapTile where S : IMapSpawnGroup
        {

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
            for (ushort y = 0; y < map.Length; y++)
            {
                for (ushort x = 0; x < map.Width; x++)
                {
                    for (byte i = 0; i < 4; i++)
                    {
                        vertexData[(y * map.Width + x) * 4 + i] = new VertexData
                        {
                            position = new float3((x + (i & 0b0001)) * tileSize, map.GetTile(x, y).Elevation * elevationStep, (y + ((i >> 1) & 0b0001)) * tileSize),
                            normal = new float3(0, 1, 0),
                            uv = new float2(((float)(x + (i & 0b0001))) / map.Width, ((float)y + ((i >> 1) & 0b0001)) / map.Length)
                        };
                    }
                    for (byte j = 0; j < 6; j++)
                    {
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
        public static bool GetPoint<T, S>(this IMap<T, S> map, float3 worldCoordinates, float3 mapOffset, out Point point, bool ignoreHeight = true, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup
        {
            if (worldCoordinates.x < mapOffset.x || worldCoordinates.x >= mapOffset.x + map.Width * tileSize || worldCoordinates.z < mapOffset.z || worldCoordinates.z >= mapOffset.z + map.Length * tileSize)
            {
                point = default;
                return false;
            }


            var coords = worldCoordinates - mapOffset;
            var p = new Point((ushort)(coords.x / tileSize), (ushort)(coords.z / tileSize));
            if (!ignoreHeight && map.GetTile(p).Elevation * elevationStep != worldCoordinates.y - mapOffset.y)
            {
                point = default;
                return false;
            }
            point = p;
            return true;

        }

        public static bool UpdateRenderLayerBuffer(IMapHighlightInfo info, ushort mapWidth, MapLayer layer, List<int> buffer, List<uint> processedBuffer) => UpdateRenderLayerBuffer(info, mapWidth, (ushort)layer, buffer, processedBuffer);
        public static bool UpdateRenderLayerBuffer(IMapHighlightInfo info, ushort mapWidth, ushort layer, List<int> buffer, List<uint> processedBuffer)
        {
            if ((info.Dirty & layer) == 0)
                return false;
            var enumerator = info.GetPoints(layer);
            processedBuffer.Clear();
            while (enumerator.MoveNext())
            {

                uint j = (uint)MapCommons.IndexOf(enumerator.Current, mapWidth);
                if (processedBuffer.Contains(j))
                    continue;
                for (byte k = 0; k < 6; k++)
                    buffer.Add((int)(j * 4 + TILE_INDICES[k]));
                processedBuffer.Add(j);
            }
            return true;
        }

        private struct VertexData
        {
            public float3 position;
            public float3 normal;
            public float2 uv;
        }
    }

    public interface IMapHighlightInfo
    {
        ushort Dirty { get; }

        IEnumerator<Point> GetPoints(ushort layer);

        IEnumerator<Point> GetPoints(MapLayer layer);
    }

    public struct MapHighlightInfo : IMapHighlightInfo
    {

        public ushort Dirty => throw new NotImplementedException();

        public IEnumerator<Point> GetPoints(ushort layer)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Point> GetPoints(MapLayer layer)
        {
            throw new NotImplementedException();
        }
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        public static readonly Point zero = new Point(0, 0);

        public ushort x;
        public ushort y;

        public Point(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(uint2 coordinates)
        {

            this.x = (ushort)coordinates.x;
            this.y = (ushort)coordinates.y;
        }
        public Point(int x, int y)
        {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
        }

        public bool ComparePoints(Point comparePoint)
        {
            if (this.x == comparePoint.x && this.y == comparePoint.y)
                return true;
            return false;
        }

        public bool InRange(Point comparePoint, ushort range)
        {
            if (this.Distance(comparePoint) <= range)
                return true;
            else
                return false;
        }
        
        public void GetPathToTargetTile(ref NativeList<Point> fullPathToTargetPoint, Point targetPoint, int range, ushort maxLength, ushort maxWidth, double leniency)
        {
            //idk why this would happen but it could I guess
            if (this.ComparePoints(targetPoint))
                return;

            if (this.x == targetPoint.x)
            {
                if (targetPoint.y > this.y) //2 > 1
                {
                    for (int y = this.y; y <= targetPoint.y; y++)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
                else
                {
                    for (int y = this.y; y >= targetPoint.y; y--)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
            }
            else if (this.y == targetPoint.y)
            {
                if (targetPoint.x > this.x)
                {
                    for (int x = this.x; x <= targetPoint.x; x++)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
                else
                {
                    for (int x = this.x; x >= targetPoint.x; x--)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
            }
            else
            {
                int x0 = this.x;
                int y0 = this.y;
                int x1 = targetPoint.x;
                int y1 = targetPoint.y;
                bool steep = math.abs(targetPoint.y - this.y) > math.abs(targetPoint.x - this.x);

                if (steep)
                {
                    var temp = x0;
                    x0 = y0;
                    y0 = temp;

                    temp = x1;
                    x1 = y1;
                    y1 = temp;
                }
                if (x0 > x1)
                {
                    //Note that this saves the list from target -> origin, instead of origin -> target
                    var temp = x0;
                    x0 = x1;
                    x1 = temp;

                    temp = y0;
                    y0 = y1;
                    y1 = temp;
                }

                //Don't trust this at all. Debug it. gradient probably being converted to some int or something.
                double dx = x1 - x0;
                double dy = y1 - y0;
                double gradient = dy / dx;
                if (dx == 0)
                    gradient = 1.0;

                //First endpoint
                var xEnd = math.floor(x0 + 0.5);
                var yEnd = y0 + gradient * (xEnd - x0);
                var xGap = 1 - ((x0 + 0.5) - math.floor(x0 + 0.5)); 

                var xpxl1 = xEnd;
                var ypxl1 = math.floor(yEnd);
                var firstEndPoint = new Point(0,0);

                if (steep)
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)ypxl1, (int)xpxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)ypxl1 + 1, (int)xpxl1);
                }
                else
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1 + 1);
                }
                var interY = yEnd + gradient;

                //Second endpoint
                xEnd = math.floor(x1 + 0.5);
                yEnd = y1 + gradient * (xEnd - x1);
                xGap = 1 - ((x1 + 0.5) - math.floor(x1 + 0.5));

                var xpxl2 = xEnd;
                var ypxl2 = math.floor(yEnd);
                Point secondEndPoint = new Point (0,0);

                if (steep)
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)ypxl2, (int)xpxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)ypxl2 + 1, (int)xpxl2);
                }
                else
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2 + 1);
                }
                
                //not sure if right
                fullPathToTargetPoint.Add(firstEndPoint);

                //main loop
                if (steep)
                {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++)
                    {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY), x));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY) + 1, x));
                        interY += gradient;
                    }
                }
                else
                {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++)
                    {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY)));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY) + 1));
                        interY += gradient;
                    }
                }

                fullPathToTargetPoint.Add(secondEndPoint);
            }
        }
        
        public Point ShiftX(int amount)
        {
            return new Point(x + amount, y);
        }
        public bool ShiftX(int amount, int max, out Point output)
        {
            int newX = x + amount;
            if (newX < 0 || newX >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, y);
                return true;
            }
        }
        public Point ShiftY(int amount)
        {
            return new Point(x, y + amount);
        }
        public bool ShiftY(int amount, int max, out Point output)
        {
            int newY = y + amount;
            if (newY < 0 || newY >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, newY);
                return true;
            }
        }

        public Point Shift(int xAmount, int yAmount)
        {
            return new Point(x + xAmount, y + yAmount);
        }

        public bool TryShift(int xAmount, int yAmount, int xMax, int yMax, out Point output)
        {
            int newY = y + yAmount;
            int newX = x + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, newY);
                return true;
            }
        }
        public bool TryShift(sbyte xAmount, sbyte yAmount, int xMax, int yMax, out Point output)
        {
            int newY = y + yAmount;
            int newX = x + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, newY);
                return true;
            }
        }

        public float Distance(Point other)
        {
            return math.distance(x - other.x, y - other.y);
        }
        public int ManhattanDistance(Point other)
        {
            return math.abs(x - other.x) + math.abs(y - other.y);
        }
        public static Point FromIndex(int index, ushort width)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            return new Point(index % width, index / width);
        }

        public static bool Create(int x, int y, int maxX, int maxY, out Point output)
        {
            if (x < 0 || x >= maxX || y < 0 || y >= maxY)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, y);
                return true;
            }
        }



        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public override int GetHashCode()
        {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "Point(" + x + "," + y + ")";
        }

        public static Point operator +(Point thisPoint, Point otherPoint) => new Point(thisPoint.x + otherPoint.x, thisPoint.y + otherPoint.y);

        public static Point operator -(Point thisPoint, Point otherPoint) => new Point(thisPoint.x - otherPoint.x, thisPoint.y - otherPoint.y);
        public static implicit operator Vector2Int(Point point) => new Vector2Int(Convert.ToInt32(point.x), Convert.ToInt32(point.y));
        public static explicit operator Point(Vector2Int vector)
        {
            if (vector.x >= 0 && vector.y >= 0)
                if (vector.x == 0 && vector.y == 0)
                    return zero;
                else
                    return new Point((ushort)vector.x, (ushort)vector.y);
            else
                throw new InvalidCastException("Vector must be positive");
        }

        public static NativeArray<Point> CreateMapPointSet(ushort width, ushort length, Allocator allocator = Allocator.Temp)
        {
            var result = new NativeArray<Point>(width * length, allocator);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Point.FromIndex(i, width);
            }
            return result;
        }


    }

    public class PointComparerXAxis : IComparer<Point>
    {
        public int Compare(Point x, Point y)
        {
            var comparison = x.x.CompareTo(y.x);
            return comparison != 0 ? comparison : x.y.CompareTo(y.y);
        }
    }
    public class PointComparerYAxis : IComparer<Point>
    {
        public int Compare(Point x, Point y)
        {
            var comparison = x.y.CompareTo(y.y);
            return comparison != 0 ? comparison : x.x.CompareTo(y.x);
        }
    }
    [Flags]
    public enum MapLayer
    {
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


    public static class MapLayers
    {

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