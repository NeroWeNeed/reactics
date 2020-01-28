using UnityEngine;
using UnityEditor;
using Reactics.Battle;


namespace Reactics.Editors
{


    using UnityEngine;
    using UnityEditor;

    public class MapEditor : EditorWindow
    {
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject as Map != null)
            {
                ShowWindow();
                return true;
            }
            return false;
        }

        [MenuItem("Reactics/Map")]
        private static void ShowWindow()
        {
            var window = GetWindow<MapEditor>();
            window.Init();
            window.Show();
        }
        private void Init()
        {

        }

        private void OnGUI()
        {

        }
    }
}