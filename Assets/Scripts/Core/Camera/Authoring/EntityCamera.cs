using Reactics.Core.Camera;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Core.Camera.Authoring {

    [RequireComponent(typeof(UnityEngine.Camera))]
    public class EntityCamera : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            //dstManager.AddComponentObject(entity, GetComponent<UnityEngine.Camera>());
            dstManager.AddSharedComponentData(entity, new CameraReference { Value = GetComponent<UnityEngine.Camera>() });
            dstManager.AddSharedComponentData(entity, new CameraTag { Value = this.tag });

        }
    }
}