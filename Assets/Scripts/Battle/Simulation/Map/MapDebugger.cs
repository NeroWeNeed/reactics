using Reactics.Battle;
using UnityEngine;
namespace Reactics.Debugger
{


    [RequireComponent(typeof(MapRenderer))]
    [ExecuteInEditMode]

    public class MapDebugger : MonoBehaviour
    {

        private MapRenderer mapRenderer = null;
        public MapRenderer MapRenderer
        {
            get
            {
                if (mapRenderer == null)
                    mapRenderer = GetComponent<MapRenderer>();
                return mapRenderer;
            }

        }
        private void Awake()
        {
            tag = "Debug";

        }


    }
}