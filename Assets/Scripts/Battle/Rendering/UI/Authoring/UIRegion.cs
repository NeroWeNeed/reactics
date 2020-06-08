using Unity.Entities;
using UnityEditor;
using UnityEngine;
namespace Reactics.UI.Authoring
{

    [RequiresEntityConversion]
    [RequireComponent(typeof(ConvertGameObjectUIToEntity),typeof(ConvertToEntity))]
    [ConverterVersion("Nero",1)]
    public class UIRegion : MonoBehaviour, IConvertGameObjectToEntity
    {
        [MenuItem("GameObject/Entities UI/Region", false, 0)]
        public static void CreateEntitiesUIRegion(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Region", typeof(UIRegion), typeof(ConvertGameObjectUIToEntity));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
        [SerializeField]
        private ValueRef x;
        [SerializeField]
        private ValueRef y;
        [SerializeField]
        private ValueRef width;
        [SerializeField]
        private ValueRef height;

        private ConvertGameObjectUIToEntity convertGameObjectUIToEntity;
        private void Awake()
        {
            convertGameObjectUIToEntity = GetComponent<ConvertGameObjectUIToEntity>();
            convertGameObjectUIToEntity.converter = Convert;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            convertGameObjectUIToEntity.Convert(entity, dstManager, conversionSystem, Entity.Null);

        }
        private void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            dstManager.World.GetOrCreateSystem<UISystemGroup>().Layouts.RegionBuilder(entity, x, y, width, height).Build();
        }
    }
}