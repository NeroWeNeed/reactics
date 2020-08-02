using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Reactics.Core.UI.Authoring {

    public class UIRectangle : MonoBehaviour, IConvertGameObjectToEntity {

        public UILength width = new UILength(40, UILengthUnit.Px);
        public UILength height = new UILength(40, UILengthUnit.Px);
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new UI.UIRectangle
            {
                width = width,
                height = height
            });
            dstManager.AddComponent<UISize>(entity);
        }
    }
}