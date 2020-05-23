using Unity.Entities;
using UnityEditor;
using UnityEngine;
namespace Reactics.UI.Authoring
{

    [RequireComponent(typeof(ConvertGameObjectUIToEntity))]
    public class UIVerticalLayout : MonoBehaviour
    {
        [MenuItem("GameObject/Entities UI/Layouts/Vertical Layout", false, 10)]
        public static void CreateEntitiesUIRegion(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Vertical Layout", typeof(UIVerticalLayout),typeof(ConvertGameObjectUIToEntity));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
        [SerializeField]
        ValueRef spacing;


        private void Awake()
        {
            GetComponent<ConvertGameObjectUIToEntity>().converter = Convert;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            
            dstManager.World.GetOrCreateSystem<UISystemGroup>().Layouts.VerticalLayoutBuilder(entity, spacing, parent).Build();
        }
    }
}