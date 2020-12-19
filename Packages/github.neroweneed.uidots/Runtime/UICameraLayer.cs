using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NeroWeNeed.UIDots {
    [RequireComponent(typeof(Camera))]
    public class UICameraLayer : MonoBehaviour, IConvertGameObjectToEntity {
        private void Start() {
            var uiCamera = this.GetComponent<Camera>();
            var mainCamera = this.GetComponentInParent<Camera>();
            if (mainCamera == null || uiCamera == null)
                return;
            uiCamera.tag = "UICamera";
            uiCamera.depth = 0;
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = mainCamera.pixelHeight / 2f;
            uiCamera.cullingMask = UICamera.UILayer;
            uiCamera.gameObject.layer = UICamera.UILayer;
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.transform.SetParent(mainCamera.transform, false);
            uiCamera.cullingMask = 1 << UICamera.UILayer;
            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            /*             GameObjectEntity.AddToEntity(dstManager, this.gameObject, entity);

                        dstManager.AddComponentObject(entity, GetComponent<Camera>()); */
            dstManager.AddComponent<UIContextProvider>(entity);
            dstManager.AddComponent<UICameraLayer>(entity);
        }
    }
}
