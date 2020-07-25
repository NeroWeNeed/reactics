using Reactics.Battle.Map;
using Reactics.Battle.Map.Authoring;
using UnityEditor;
using UnityEngine;

namespace Reactics.Editor {
    public static class Menus {
        [MenuItem("GameObject/Map", false, 10)]
        public static void CreateMap(MenuCommand menuCommand) {
            var gameObject = new GameObject("Map", typeof(Map));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
            var map = gameObject.GetComponent<Map>();
            map.layerColors = MapLayers.CreateDefaultColorMap();

        }
    }
}