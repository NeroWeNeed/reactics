using System.Collections;
using UnityEngine;

namespace Reactics.Battle
{
    public static class MapUtil
    {
        public static Mesh GenerateMesh(Mesh mesh, ushort width, ushort length, float tileSize)
        {
            int vertexCount = (width + 1) * (length + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[width * length * 6];
            int x, y, index;
            for (y = 0; y <= length; y++)
            {
                for (x = 0; x <= width; x++)
                {
                    index = y * (width + 1) + x;
                    vertices[index] = new Vector3(x * tileSize, 0, y * tileSize);
                    uv[index] = new Vector2((float)x / (width), (float)y / (length));
                    normals[index] = Vector3.up;
                }
            }
            for (y = 0; y < length; y++)
            {
                for (x = 0; x < width; x++)
                {
                    index = (y * width + x) * 6;
                    triangles[index] = y * (width + 1) + x;
                    triangles[index + 1] = y * (width + 1) + x + width + 1;
                    triangles[index + 2] = y * (width + 1) + x + width + 2;
                    triangles[index + 3] = y * (width + 1) + x;
                    triangles[index + 4] = y * (width + 1) + x + width + 2;
                    triangles[index + 5] = y * (width + 1) + x + 1;
                }
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }




    }


}