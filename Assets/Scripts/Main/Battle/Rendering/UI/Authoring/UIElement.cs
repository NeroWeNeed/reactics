using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Core.UI.Author {

    [RequiresEntityConversion]
    [ConverterVersion("Nero", 2)]
    public class UIElement : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            UIElement component = null;
            if (transform.parent?.TryGetComponent(out component) == true) {
                dstManager.AddComponentData(entity, new UIParent
                {
                    value = conversionSystem.GetPrimaryEntity(component)
                });
            }
            dstManager.AddComponentData(entity, new Reactics.Core.UI.UIElement());
            dstManager.AddComponent<UIResolvedBox>(entity);

            var children = new NativeList<UIChild>(Allocator.Temp);
            foreach (Transform child in transform) {
                if (child.TryGetComponent(out component)) {
                    children.Add(conversionSystem.GetPrimaryEntity(component));
                }
            }
            var buffer = dstManager.AddBuffer<UIChild>(entity);
            buffer.AddRange(children.AsArray());
        }
    }

}