using System;
using System.Linq;
using Reactics.Util;
using UnityEngine;

namespace Reactics.Battle
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider),typeof(MeshRenderer))]
    public class MapRenderer : MonoBehaviour
    {
        [SerializeField]
        private Map map;

        public Map Map => map;

        [SerializeField]
        private float tileSize;

        public float TileSize => tileSize;

        [SerializeField]
        private float tileElevationDelta;

        public float TileElevationDelta => tileElevationDelta;
        private Mesh mesh;





        private void Awake()
        {
            RefreshMesh();
        }
        private void OnDrawGizmosSelected()
        {
            RefreshMesh(true);
            Gizmos.DrawWireMesh(mesh);

        }

        private void RefreshMesh(bool force = false)
        {
            bool shouldUpdate = force || mesh == null;
            if (mesh == null)
                mesh = new Mesh();

            if (shouldUpdate)
            {
                UpdateMesh(mesh);
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }

        }
        private void UpdateMesh(Mesh mesh)
        {
            UpdateMesh(map, TileSize, mesh);
        }
        public static Mesh GenerateMesh(Map map, float tileSize)
        {
            Mesh mesh = new Mesh();
            UpdateMesh(map, tileSize, mesh);
            return mesh;
        }
        public static void UpdateMesh(Map map, float tileSize, Mesh mesh)
        {
            int vertexCount = (map.Width + 1) * (map.Length + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[map.Width * map.Length * 6];
            int x, y, index;
            for (y = 0; y <= map.Length; y++)
            {
                for (x = 0; x <= map.Width; x++)
                {
                    index = y * (map.Width + 1) + x;
                    vertices[index] = new Vector3(x * tileSize, 0, y * tileSize);
                    uv[index] = new Vector2((float)x / (map.Width), (float)y / (map.Length));
                    normals[index] = Vector3.up;

                }
            }

            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    index = (y * map.Width + x) * 6;
                    triangles[index] = y * (map.Width + 1) + x;
                    triangles[index + 1] = y * (map.Width + 1) + x + (map.Width + 1);
                    triangles[index + 2] = y * (map.Width + 1) + x + (map.Width + 2);
                    triangles[index + 3] = y * (map.Width + 1) + x;
                    triangles[index + 4] = y * (map.Width + 1) + x + (map.Width + 2);
                    triangles[index + 5] = y * (map.Width + 1) + x + 1;
                }
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;

            mesh.normals = normals;
        }
        public Vector2Int? CoordinateFromWorldPoint(Vector3 worldPoint) => CoordinateFromWorldPoint(worldPoint.x, worldPoint.z);
        public Vector2Int? CoordinateFromWorldPoint(Vector2 worldPoint) => CoordinateFromWorldPoint(worldPoint.x, worldPoint.y);
        public Vector2Int? CoordinateFromWorldPoint(float x, float z) => CoordinateFromWorldPoint(map, tileSize, transform.position, x, z);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, float tileSize, Vector3 offset, Vector3 worldPoint) => CoordinateFromWorldPoint(map, tileSize, offset, worldPoint.x, worldPoint.z);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, float tileSize, Vector3 offset, Vector2 worldPoint) => CoordinateFromWorldPoint(map, tileSize, offset, worldPoint.x, worldPoint.y);

        public static Vector2Int? CoordinateFromWorldPoint(Map map, float tileSize, Vector3 worldPoint) => CoordinateFromWorldPoint(map, tileSize, Vector3.zero, worldPoint.x, worldPoint.z);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, float tileSize, Vector2 worldPoint) => CoordinateFromWorldPoint(map, tileSize, Vector3.zero, worldPoint.x, worldPoint.y);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, Vector3 offset, Vector3 worldPoint) => CoordinateFromWorldPoint(map, 1f, offset, worldPoint.x, worldPoint.z);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, Vector3 offset, Vector2 worldPoint) => CoordinateFromWorldPoint(map, 1f, offset, worldPoint.x, worldPoint.y);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, Vector3 worldPoint) => CoordinateFromWorldPoint(map, 1f, Vector3.zero, worldPoint.x, worldPoint.z);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, Vector2 worldPoint) => CoordinateFromWorldPoint(map, 1f, Vector3.zero, worldPoint.x, worldPoint.y);
        public static Vector2Int? CoordinateFromWorldPoint(Map map, float tileSize, Vector3 offset, float x, float z)
        {
            if (map == null || x < offset.x || x >= (map.Width * tileSize) + offset.x || z < offset.z || z >= (map.Length * tileSize) + offset.z)
                return null;
            return new Vector2Int((int)((x - offset.x) / tileSize), (int)((z - offset.z) / tileSize));
        }

        public static MapRenderer CreateInstance(Map map)
        {
            
            return new GameObject("Map Renderer").AddComponent<MapRenderer>().Apply(x =>
            {
                x.map = map;
            });
            
        }
    }
}