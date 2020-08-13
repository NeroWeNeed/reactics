using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
namespace Reactics.Core.UI.Author {

    public class UIRectangle : MonoBehaviour, IConvertGameObjectToEntity {

        public UILength width;
        public UILength height;
        [ColorUsage(false)]
        public Color color;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new UI.UIRectangle
            {
                width = width,
                height = height
            });
            dstManager.AddComponentData(entity, new MaterialColor { Value = new float4(color.r, color.g, color.b, 1) });
            dstManager.AddComponent<UISize>(entity);
        }
    }
}