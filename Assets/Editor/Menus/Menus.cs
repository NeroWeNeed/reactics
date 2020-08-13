using Reactics.Core.Map;
using Reactics.Core.Map.Authoring;
using UnityEditor;
using UnityEngine;
namespace Reactics.Core.Editor {
    public static class Menus {
        [MenuItem("GameObject/Map", false, 10)]
        public static void CreateMap(MenuCommand menuCommand) {
            var gameObject = new GameObject("Map", typeof(Map.Authoring.Map));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
            var map = gameObject.GetComponent<Map.Authoring.Map>();
            map.layerColors = MapLayers.CreateDefaultColorMap();

        }
    }
}