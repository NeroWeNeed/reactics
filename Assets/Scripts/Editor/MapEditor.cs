using UnityEngine;
using UnityEditor;
using Reactics.Battle.Map;
namespace Reactics.Editor
{


    [CustomEditor(typeof(Map))]
    public class MapEditor : UnityEditor.Editor
    {
        private PreviewRenderUtility previewRenderUtility;
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();

        }
    }

}