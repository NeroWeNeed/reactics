using System;
using UnityEngine;

namespace Reactics.Battle.Map
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class MapRenderer : MonoBehaviour
    {
        [SerializeField]
        private Map map;

        public Map Map => map;

        [SerializeField]
        private float tileSize;

        public float TileSize => tileSize;
        private Mesh mesh;
        private void Start()
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
            if (mesh == null)
            {
                mesh = new Mesh();
                UpdateMesh(mesh);
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }
            else if (force)
            {
                UpdateMesh(mesh);
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }
        private void UpdateMesh(Mesh mesh)
        {
            int vertexCount = (map.Width + 1) * (map.Height + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[map.Width * map.Height * 6];
            int x, y, index;
            for (y = 0; y <= map.Height; y++)
            {
                for (x = 0; x <= map.Width; x++)
                {
                    index = y * (map.Width + 1) + x;
                    vertices[index] = new Vector3(x * TileSize, 0, y * TileSize);
                    uv[index] = new Vector2((float)x / (map.Width), (float)y / (map.Height));
                    normals[index] = Vector3.up;
                }
            }
            for (y = 0; y < map.Height; y++)
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
        public Vector2Int? CoordinateFromWorldPoint(float x, float z)
        {
            if (x < transform.position.x || x >= (map.Width * tileSize) + transform.position.x || z < transform.position.z || z >= (map.Height * tileSize) + transform.position.z)
                return null;
            return new Vector2Int((int)((x - transform.position.x) / tileSize), (int)((z - transform.position.z) / tileSize));
        }
    }
}