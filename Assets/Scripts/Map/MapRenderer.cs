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

        private void GenerateMesh(Mesh mesh)
        {

        }
    }
}