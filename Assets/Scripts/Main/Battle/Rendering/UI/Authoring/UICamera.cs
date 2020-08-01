using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reactics.Core.UI.Author {

    [RequireComponent(typeof(Camera))]
    public class UICamera : MonoBehaviour, IConvertGameObjectToEntity {
        private void OnValidate() {
            EnsureUICameraExists();
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<ScreenInfo>(entity);
        }
        private void EnsureUICameraExists() {
            foreach (var camera in GetComponentsInChildren<Camera>(true)) {
                if (camera.tag == UIScreenInfoSystem.UI_CAMERA_TAG) {
                    return;
                }
            }
            CreateUICamera();
        }
        private GameObject CreateUICamera() {
            var uiCameraLayerGO = new GameObject("UI Camera")
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
            };
            var uiCameraLayer = uiCameraLayerGO.AddComponent<Camera>();
            var mainCamera = this.GetComponent<Camera>();
            uiCameraLayer.tag = "UICamera";
            uiCameraLayer.depth = 0;
            uiCameraLayer.orthographic = true;
            uiCameraLayer.orthographicSize = Screen.currentResolution.height / 2f;
            uiCameraLayer.cullingMask = UIScreenInfoSystem.UI_LAYER;
            uiCameraLayer.gameObject.layer = UIScreenInfoSystem.UI_LAYER;
            uiCameraLayer.clearFlags = CameraClearFlags.Depth;
            uiCameraLayer.transform.SetParent(mainCamera.transform, false);
            uiCameraLayer.cullingMask = (mainCamera.cullingMask & int.MaxValue) - (1 << UIScreenInfoSystem.UI_LAYER);
            uiCameraLayer.cullingMask = 1 << UIScreenInfoSystem.UI_LAYER;
            uiCameraLayer.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(uiCameraLayer);
            return uiCameraLayerGO;
        }

    }
}