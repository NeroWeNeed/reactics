using Unity.Entities;
using UnityEngine;
namespace NeroWeNeed.UIDots {


    public class FollowCamera : MonoBehaviour, IConvertGameObjectToEntity {
        public GameObject target;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            if (target != null) {
                var targetEntity = conversionSystem.GetPrimaryEntity(target);
                dstManager.AddComponentData(entity, new FollowCameraData { value = targetEntity });
            }
        }
    }


}