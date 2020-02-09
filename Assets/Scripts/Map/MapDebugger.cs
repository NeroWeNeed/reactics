namespace Reactics.Debugger
{
    using Reactics.Battle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;

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