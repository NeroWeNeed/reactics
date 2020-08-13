using Reactics.Core.Camera;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Core.UI.Author {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class EntityCamera : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddSharedComponentData(entity, new CameraData
            {
                camera = GetComponent<UnityEngine.Camera>(),
                tag = this.tag
            });

        }
    }
}