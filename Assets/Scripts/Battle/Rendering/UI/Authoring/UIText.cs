using TMPro;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Reactics.UI.Authoring
{
    [RequireComponent(typeof(ConvertGameObjectUIToEntity))]
    public class UIText : MonoBehaviour
    {
        [MenuItem("GameObject/Entities UI/Elements/Text", false, 20)]
        public static void CreateEntitiesUIRegion(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Text", typeof(UIText), typeof(ConvertGameObjectUIToEntity));
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }
        [SerializeField]
        private string text;

        [SerializeField]
        private TMP_FontAsset font;



        private void Awake()
        {
            GetComponent<ConvertGameObjectUIToEntity>().converter = Convert;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            dstManager.World.GetOrCreateSystem<UISystemGroup>().Elements.TextBuilder(entity,text,font,parent);
            
        }
    }
}