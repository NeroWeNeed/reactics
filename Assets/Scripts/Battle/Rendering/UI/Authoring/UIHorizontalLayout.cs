using Unity.Entities;
using UnityEditor;
using UnityEngine;
namespace Reactics.UI.Authoring
{

    [RequireComponent(typeof(ConvertGameObjectUIToEntity))]
    public class UIHorizontalLayout : MonoBehaviour
    {
        [MenuItem("GameObject/Entities UI/Layouts/Horizontal Layout", false, 10)]
        public static void CreateEntitiesUIRegion(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Horizontal Layout", typeof(UIHorizontalLayout),typeof(ConvertGameObjectUIToEntity));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
        [SerializeField]
        ValueRef spacing;


        private void Start()
        {
            GetComponent<ConvertGameObjectUIToEntity>().converter = Convert;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            dstManager.World.GetOrCreateSystem<UISystemGroup>().Layouts.HorizontalLayoutBuilder(entity, spacing, parent).Build();
        }
    }
}