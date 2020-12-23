using System.Collections.Generic;
using System.Linq;
using NeroWeNeed.Commons;
using NeroWeNeed.UIDots;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[assembly: RegisterGenericComponentType(typeof(GameObjectComponentData<UICamera>))]
namespace NeroWeNeed.UIDots {

    [RequireComponent(typeof(Camera))]
    public class UICamera : MonoBehaviour, IConvertGameObjectToEntity {
        private static int uiLayer = -1;

        public static int UILayer { get => uiLayer == -1 ? LayerMask.NameToLayer("UI") : uiLayer; }
        public static readonly string UI_CAMERA_TAG = "UICamera";

        [SerializeField, HideInInspector]
        private GameObject uiCameraGO;
        [SerializeField, HideInInspector]
        private Camera uiCamera;
        [SerializeField, HideInInspector]
        private Camera mainCamera;

        public Camera UILayerCamera { get => uiCamera; }
        public GameObject UILayerCameraObject { get => uiCameraGO; }

        public Camera MainCamera { get => mainCamera; }

        private void OnValidate() {
            mainCamera = this.GetComponent<Camera>();
            var uiCameraObjects = this.GetComponentsInChildren<Camera>().Where(camera => camera.tag == UI_CAMERA_TAG).ToArray();

/*             if (uiCameraObjects.Length == 0) {
                uiCameraGO = CreateUICamera(mainCamera, out uiCamera);
            }
            else if (uiCameraObjects.Length > 1) {
                Debug.LogError($"Multiple UI Cameras found on {name}, please delete them.");
                return;
            }
            else {
                uiCamera = uiCameraObjects[0];
                uiCameraGO = uiCamera.gameObject;
            }
 */
        }
        public UIContext CreateContext() => new UIContext
        {
            dpi = Screen.dpi,
            size = new Unity.Mathematics.float2(uiCamera.orthographicSize * uiCamera.aspect * 2, uiCamera.orthographicSize * 2)
        };
/*         private GameObject CreateUICamera(Camera mainCamera, out Camera uiCamera) {
            var uiCameraObject = new GameObject("UI Camera", typeof(Camera));
            uiCamera = uiCameraObject.GetComponent<Camera>();
            uiCameraObject.AddComponent<UICameraLayer>();
            mainCamera.cullingMask = (mainCamera.cullingMask & int.MaxValue) - (1 << UILayer);
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
            return uiCameraObject;
        } */
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            /*             var uiCameraEntity = conversionSystem.GetPrimaryEntity(uiCameraGO);
                        dstManager.AddComponentObject(entity, this);
                        dstManager.AddComponentObject(entity, this.GetComponent<Camera>());
                        //dstManager.AddSharedComponentData(entity, new GameObjectComponentData<UICamera>(this.gameObject));
                        dstManager.AddComponentData(entity, new GameObjectUICameraData(uiCameraEntity));
                        dstManager.AddComponent<UIContextData>(entity); */
        }

        /*     public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
                referencedPrefabs.Add(uiCameraGO);
            } */
    }
}