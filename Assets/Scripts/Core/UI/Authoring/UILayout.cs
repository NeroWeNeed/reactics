using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.UI.Author {

    public class UILayout : MonoBehaviour, IConvertGameObjectToEntity {

        public Layout layout;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new UI.UILayout
            {
                value = layout
            });

        }
    }
}