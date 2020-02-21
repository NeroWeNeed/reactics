using System;
using Reactics.Util;
using UnityEngine;

namespace Reactics.Battle
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class MapRenderer : MonoBehaviour
    {

        [SerializeField]
        private Map map;
        public Map Map
        {
            get => map; set
            {
                map = value;
                UpdateMesh(map);
            }
        }

        [SerializeField]
        private float tileSize = 1f;

        public float TileSize => tileSize;

        [SerializeField]
        private float tileElevationDelta = 0.5f;

        public float TileElevationDelta => tileElevationDelta;

        [SerializeField]
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material mapMaterial;

        [SerializeField]
        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;
        private Mesh mesh;

        public ushort Width { get; private set; }
        public ushort Length { get; private set; }

        private int[] hoverBuffer;
        private void Awake()
        {
            this.InjectResources();
            if (map != null)
                mesh = GenerateMesh(Map.Width, Map.Length, tileSize);

        }
        private void Start()
        {
            GetComponent<MeshRenderer>().sharedMaterials = new Material[] { mapMaterial, hoverMaterial };
        }

        public bool UpdateMesh(Map map, bool force = false)
        {
            if (UpdateMesh(map?.Width ?? 0, map?.Length ?? 0, 1, force))
            {
                this.map = map;
                return true;
            }
            else
                return false;
        }
        private bool UpdateMesh(Map map, float newTileSize, bool force = false) => UpdateMesh(map?.Width ?? 0, map?.Length ?? 0, newTileSize, force);
        private bool UpdateMesh(ushort newWidth, ushort newLength, float newTileSize, bool force = false)
        {
            if (Width != newWidth || Length != newLength || tileSize != newTileSize || force || mesh == null)
            {
                mesh = GenerateMesh(mesh ?? new Mesh(), newWidth, newLength, newTileSize);
                return true;
            }
            else
                return false;
        }
        private Mesh GenerateMesh(ushort width, ushort length, float tileSize) => GenerateMesh(new Mesh(), width, length, tileSize);
        private Mesh GenerateMesh(Mesh mesh, ushort width, ushort length, float tileSize)
        {
            map.GenerateMesh(mesh, tileSize);
            Width = width;
            Length = length;
            this.tileSize = tileSize;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            return mesh;
        }
        /// <summary>
        /// Tries to approximate a <c>Point</c> from the provided World Point.
        /// </summary>
        /// <param name="worldPoint">The World Point</param>
        /// <param name="pointInfo">The Point to store the results in. Will be default if unable to approximate from the world point.</param>
        /// <return> True if Point could be approximated, and stores the result in the pointInfo parameter, otherwise false and the point parameter will be default.</return>
        public bool GetPoint(Vector3 worldPoint, out Point pointInfo) => GetPoint(worldPoint.x, worldPoint.z, out pointInfo);

        /// <summary>
        /// Tries to approximate a <c>Point</c> from the provided x and z coordinates in world space.
        /// </summary>
        /// <param name="x">The x coordinate in world space.</param>
        /// <param name="x">The y coordinate in world space.</param>
        /// <param name="pointInfo">The Point to store the results in. Will be default if unable to approximate from the world point.</param>
        /// <return> True if Point could be approximated, and stores the result in the pointInfo parameter, otherwise false and the point parameter will be default.</return>
        public bool GetPoint(float x, float z, out Point pointInfo)
        {
            if (map == null || x < transform.position.x || x >= ((map.Width) * tileSize) + transform.position.x || z < transform.position.z || z >= ((map.Length) * tileSize) + transform.position.z)
            {
                pointInfo = default;
                return false;
            }
            pointInfo = new Point((ushort)((x - transform.position.x) / tileSize), (ushort)((z - transform.position.z) / tileSize));
            return true;
        }
        /// <summary>
        /// The Center of the grid in world coordinates.
        /// </summary>
        /// <returns>The center of the grid in world coordinates.</returns>
        public Vector3 GetCenter() => mesh?.bounds.center ?? Vector3.zero;
        /// <summary>
        /// The maximum distance the camera can be from the map, or the diagonal of the map.
        /// </summary>
        /// <returns>The maximum distance the camera can be from the map</returns>
        public float GetMaxCameraDistance() => map == null ? 0f : Mathf.Sqrt(Mathf.Pow(map.Width * tileSize, 2f) + Mathf.Pow(map.Length * tileSize, 2f));
        /// <summary>
        /// Focuses the camera on the center of the grid (in world coordinates). The camera is moved to be within the maximum distance, and a minimum height above the map (10% of the max distance). The direction from the center to the camera remains the same.
        /// </summary>
        public void FocusCamera(Camera camera)
        {
            float maxDistance = GetMaxCameraDistance();
            Vector3 center = GetCenter();
            if (Vector3.Distance(center, camera.transform.position) > maxDistance)
                camera.transform.position = (camera.transform.position - center).normalized * maxDistance;
            if (camera.transform.position.y < center.y + maxDistance * 0.1f)
                camera.transform.position = new Vector3(camera.transform.position.x, center.y + maxDistance * 0.1f, camera.transform.position.z);
            camera.transform.rotation = Quaternion.LookRotation((center - camera.transform.position).normalized, Vector3.up);
        }
        /// <summary>
        /// Checks to see if the provided point is within the map bounds.
        /// </summary>
        /// <returns>True if the map is within bounds, false otherwise.</returns>
        public bool Contains(Point? point)
        {
            return point != null && point.Value.x < map.Width && point.Value.y < Map.Length;
        }
        /// <inheritsdoc>
        public bool Contains(Point point)
        {
            return point.x < map.Width && point.y < Map.Length;
        }
        /// <summary>
        /// Signals the renderer to hover the selected tile. This updates the submesh that uses the hover material.
        /// </summary>
        /// <param name="point">The point to hover</param>
        public void Hover(Point point)
        {
            if (!Contains(point))
            {
                mesh.SetTriangles(Array.Empty<int>(), 1);
            }
            else
            {
                if (hoverBuffer == null)
                    hoverBuffer = new int[6];

                Map.GenerateMeshTile(hoverBuffer, 0, point.x, point.y,map.Width);
                mesh.SetTriangles(hoverBuffer, 1);
            }
        }
        /// <summary>
        /// Clears the hover tile.
        /// </summary>
        public void UnHover()
        {

            mesh.SetTriangles(Array.Empty<int>(), 1);
        }



    }
}