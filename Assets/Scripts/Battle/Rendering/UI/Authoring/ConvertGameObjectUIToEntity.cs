using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reactics.UI.Authoring
{

    public class ConvertGameObjectUIToEntity : MonoBehaviour
    {
        public Convert converter;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent)
        {
            converter?.Invoke(entity, dstManager, conversionSystem, parent);
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out ConvertGameObjectUIToEntity obj))
                {
                    var childEntity = dstManager.CreateEntity();
#if UNITY_EDITOR
                    dstManager.SetName(childEntity, child.name);
#endif
                    obj.Convert(childEntity, dstManager, conversionSystem, entity);
                }
            }
            if (this.gameObject.transform.GetComponent(typeof(ConvertToEntity)) == null)
                Destroy(gameObject);
        }
    }
    public delegate void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem, Entity parent);

}