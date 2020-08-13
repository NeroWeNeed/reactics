using Reactics.Core.Commons;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.UI.Author {

    public class UIText : MonoBehaviour, IConvertGameObjectToEntity {

        [SerializeField]
        private string text;
        public string Text { get => text; set => text = value; }
        [SerializeField]
        private TMP_FontAsset font;
        public TMP_FontAsset Font { get => font; set => font = value; }
        [SerializeField]
        private UILength fontSize;

        public UILength FontSize { get => fontSize; set => fontSize = value; }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddSharedComponentData(entity, new UIFont { value = font, size = fontSize });

            dstManager.AddComponentData(entity, new UI.UIText(text));
            dstManager.AddComponent<UISize>(entity);
            dstManager.AddComponent<UITextVersion>(entity);

        }
    }
}