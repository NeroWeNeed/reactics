using Unity.Entities;
using UnityEditor;
using UnityEngine;
namespace Reactics.UI.Authoring
{
    [RequireComponent(typeof(ConvertGameObjectUIToEntity))]
    public class UIBox : MonoBehaviour
    {
        [MenuItem("GameObject/Entities UI/Elements/Box", false, 20)]
        public static void CreateEntitiesUIRegion(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Box", typeof(UIBox));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
        [SerializeField]
        private ValueRef width;

        [SerializeField]
        private ValueRef height;
        private void Awake()
        {
            GetComponent<ConvertGameObjectUIToEntity>().converter = Convert;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            dstManager.World.GetOrCreateSystem<UISystemGroup>().Elements.BoxBuilder(entity, width, height, parent);

        }
    }
}