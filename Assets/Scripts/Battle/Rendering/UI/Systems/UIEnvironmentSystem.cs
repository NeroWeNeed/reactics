using System;
using Reactics.UI;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Reactics.UI
{
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateBefore(typeof(UILayoutSystemGroup))]
    [DisableAutoCreation]
    public class UIEnvironmentSystem : SystemBase
    {

        public Camera MainCamera { get; private set; }
        public Camera UICamera { get; private set; }
        public static readonly int UI_LAYER = LayerMask.NameToLayer("UI");

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<UIEnvironmentData>();
        }
        protected override void OnStartRunning()
        {
            if (MainCamera == null || UICamera == null)
            {
                Camera mainCamera = null;
                Camera uiCamera = null;
                foreach (Camera camera in UnityEngine.Object.FindObjectsOfType(typeof(Camera)))
                {
                    if (camera.tag == "MainCamera")
                    {
                        mainCamera = camera;
                        break;
                    }
                    else if (camera.tag == "UICamera")
                    {
                        uiCamera = camera;
                        break;
                    }
                }
                if (mainCamera == null)
                {
                    return;
                    //throw new UnityException("Missing Main Camera in Scene");
                }
                if (uiCamera == null)
                {
                    uiCamera = CreateUICamera(mainCamera);
                }
                MainCamera = mainCamera;
                UICamera = uiCamera;
            }
        }
        protected override void OnUpdate()
        {

            var oldEnvironmentData = GetSingleton<UIEnvironmentData>();
            var newEnvironmentData = new UIEnvironmentData
            {
                window = new float2(Screen.width, Screen.height),
                screen = new float2(Screen.currentResolution.width, Screen.currentResolution.height)
            };
            if (!oldEnvironmentData.Equals(newEnvironmentData))
            {
                ValueConverterReferences.UpdateReferenceValues(UICamera);
                SetSingleton(newEnvironmentData);
                Entities.ForEach((ref UIElement element) =>
                {
                    element.updateCount += 1;
                }).Run();

            }

        }
        private static Camera CreateUICamera(Camera mainCamera)
        {
            Camera uiCamera = new GameObject("UI Camera", typeof(Camera)).GetComponent<Camera>();
            uiCamera.tag = "UICamera";
            uiCamera.depth = 0;
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = Screen.currentResolution.height / 2f;
            uiCamera.cullingMask = UI_LAYER;
            uiCamera.gameObject.layer = UI_LAYER;
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.transform.SetParent(mainCamera.transform, false);
            mainCamera.cullingMask = (mainCamera.cullingMask & int.MaxValue) - (1 << UI_LAYER);
            uiCamera.cullingMask = 1 << UI_LAYER;
            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
            return uiCamera;
        }
    }

}