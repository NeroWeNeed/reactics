using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Collections;
using Unity.Entities;
using System.Linq;

namespace Reactics.Battle
{


    [CreateAssetMenu(fileName = "Map", menuName = "Reactics/Map", order = 0)]
    public class Map : ScriptableObject, IEnumerable, ISerializationCallbackReceiver
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
        public Tile this[int x, int y] => tiles[IndexOf((ushort)x, (ushort)y)];
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
        /// Creates a Map Entity from the Asset.
        /// </summary>
        /// <param name="entityManager">Entity Manager to use for Entity Creation. Defaults to <c>World.DefaultGameObjectInjectionWorld.EntityManager</c></param>
        /// <returns>A Map Entity</returns>
        public Entity CreateEntity(EntityManager entityManager = null)
        {
            EntityManager manager = entityManager ?? World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = manager.CreateEntity(typeof(MapHeader), typeof(MapTile), typeof(MapSpawnGroupPoint));

            manager.SetComponentData(entity, new MapHeader(this.Name, this.Width, this.Length, this.Elevation));
            DynamicBuffer<MapTile> tileElement = manager.GetBuffer<MapTile>(entity);
            tileElement.Capacity = Width * Length;
            tileElement.CopyFrom(this.tiles.Select(x => new MapTile(x)).ToArray());
            DynamicBuffer<MapSpawnGroupPoint> spawnGroupElement = manager.GetBuffer<MapSpawnGroupPoint>(entity);
            for (int i = 0; i < this.spawnGroups.Length; i++)
            {
                foreach (var item in this.spawnGroups[i].points)
                {
                    spawnGroupElement.Add(new MapSpawnGroupPoint(item, i));
                }
            }
            return entity;
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
        public Mesh GenerateMesh(Mesh mesh = null, float tileSize = 1f)
        {
            int vertexCount = (Width) * (Length) * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[Width * Length * 6];
            int x, y, index;
            for (y = 0; y < Length; y++)
            {
                for (x = 0; x < Width; x++)
                {
                    index = (y * (Width) + x) * 4;

                    vertices[index] = new Vector3(x * tileSize, this[x, y].elevation * 0.25f, y * tileSize);
                    uv[index] = new Vector2((float)x / (Width), (float)y / (Length));
                    normals[index] = Vector3.up;

                    vertices[index + 1] = new Vector3((x + 1) * tileSize, this[x, y].elevation * 0.25f, y * tileSize);
                    uv[index + 1] = new Vector2(((float)x + 1) / (Width), (float)y / (Length));
                    normals[index + 1] = Vector3.up;

                    vertices[index + 2] = new Vector3(x * tileSize, this[x, y].elevation * 0.25f, (y + 1) * tileSize);
                    uv[index + 2] = new Vector2((float)x / (Width), ((float)y + 1) / (Length));
                    normals[index + 2] = Vector3.up;

                    vertices[index + 3] = new Vector3((x + 1) * tileSize, this[x, y].elevation * 0.25f, (y + 1) * tileSize);
                    uv[index + 3] = new Vector2(((float)x + 1) / (Width), ((float)y + 1) / (Length));
                    normals[index + 3] = Vector3.up;
                }
            }
            for (y = 0; y < Length; y++)
            {
                for (x = 0; x < Width; x++)
                {
                    GenerateMeshTile(triangles, (y * Width + x) * 6, x, y,Width);
                    /*                     index = (y * Width + x) * 6;
                                        triangles[index] = y * (Width + 1) + x;
                                        triangles[index + 1] = y * (Width + 1) + x + Width + 1;
                                        triangles[index + 2] = y * (Width + 1) + x + Width + 2;
                                        triangles[index + 3] = y * (Width + 1) + x;
                                        triangles[index + 4] = y * (Width + 1) + x + Width + 2;
                                        triangles[index + 5] = y * (Width + 1) + x + 1; */
                }
            }
            if (mesh == null)
                mesh = new Mesh();
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }
        public static void GenerateMeshTile(int[] triangles, int index, int x, int y,int stride)
        {
            triangles[index] = (y * stride + x) * 4;
            triangles[index + 1] = ((y * stride + x) * 4) + 2;
            triangles[index + 2] = ((y * stride + x) * 4) + 1;
            triangles[index + 3] = ((y * stride + x) * 4) + 2;
            triangles[index + 4] = ((y * stride + x) * 4) + 3;
            triangles[index + 5] = ((y * stride + x) * 4) + 1;
        }
        public static void GenerateMeshTiles(int[] triangles, int index,int stride, params Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                GenerateMeshTile(triangles, index + (i * 6), points[i].x, points[i].y,stride);
            }
        }



    }
}
