using System;
using System.Collections.Generic;
using Reactics.Core.Camera;
using Reactics.Core.Commons;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.UI {
    /// <summary>
    /// System for updating the status of the screen
    /// </summary>
    [UpdateInGroupFirst(typeof(UISystemGroup))]
    [AlwaysUpdateSystem]
    public class UIScreenInfoSystem : SystemBase {
        private abstract class StateContext {
            private class ScreenWidthKey { }
            private class ScreenHeightKey { }
            private class ScreenDpiKey { }
            private class ScreenOrientationKey { }
            private class ScreenResolutionWidthKey { }
            private class ScreenResolutionHeightKey { }
            public static readonly SharedStatic<int> ScreenWidth = SharedStatic<int>.GetOrCreate<StateContext, ScreenWidthKey>();
            public static readonly SharedStatic<int> ScreenHeight = SharedStatic<int>.GetOrCreate<StateContext, ScreenHeightKey>();
            public static readonly SharedStatic<float> ScreenDpi = SharedStatic<float>.GetOrCreate<StateContext, ScreenDpiKey>();
            public static readonly SharedStatic<ScreenOrientation> ScreenOrientation = SharedStatic<ScreenOrientation>.GetOrCreate<StateContext, ScreenOrientationKey>();
            public static readonly SharedStatic<int> ScreenResolutionWidth = SharedStatic<int>.GetOrCreate<StateContext, ScreenResolutionWidthKey>();
            public static readonly SharedStatic<int> ScreenResolutionHeight = SharedStatic<int>.GetOrCreate<StateContext, ScreenResolutionHeightKey>();
        }
        public static SharedStatic<int> ScreenWidth => StateContext.ScreenWidth;
        public static SharedStatic<int> ScreenHeight => StateContext.ScreenHeight;
        public static SharedStatic<float> ScreenDpi => StateContext.ScreenDpi;
        public static SharedStatic<ScreenOrientation> ScreenOrientation => StateContext.ScreenOrientation;
        public static SharedStatic<int> ScreenResolutionWidth => StateContext.ScreenResolutionWidth;
        public static SharedStatic<int> ScreenResolutionHeight => StateContext.ScreenResolutionHeight;
        public static readonly int UI_LAYER = LayerMask.NameToLayer("UI");
        public const string UI_CAMERA_TAG = "UICamera";
        private readonly Dictionary<string, UnityEngine.Camera> cameras = new Dictionary<string, UnityEngine.Camera>();
        public UnityEngine.Camera MainCamera { get => cameras["MainCamera"]; }
        public UnityEngine.Camera UICamera { get => cameras[UI_CAMERA_TAG]; }
        public bool Dirty { get; set; }
        protected override void OnCreate() {

            ScreenWidth.Data = Screen.width;
            ScreenHeight.Data = Screen.height;
            ScreenResolutionWidth.Data = Screen.currentResolution.width;
            ScreenResolutionHeight.Data = Screen.currentResolution.height;
            ScreenDpi.Data = Screen.dpi;
            ScreenOrientation.Data = Screen.orientation;
            Dirty = true;
        }
        protected override void OnStartRunning() {
            InitCameras();
        }
        private void InitCameras() {

            this.cameras.Clear();
            var cameraData = new List<CameraData>();
            EntityManager.GetAllUniqueSharedComponentData(cameraData);
            foreach (var data in cameraData) {
                if (data.camera == null)
                    continue;
                this.cameras[data.tag] = data.camera;
            }

        }
        protected override void OnUpdate() {
            var newScreenWidth = Screen.width;
            var newScreenHeight = Screen.height;
            var newScreenDpi = Screen.dpi;
            var newScreenOrientation = Screen.orientation;
            var newScreenResolutionWidth = Screen.currentResolution.width;
            var newScreenResolutionHeight = Screen.currentResolution.height;
            Dirty |= ScreenWidth.Data != newScreenWidth;
            Dirty |= ScreenHeight.Data != newScreenHeight;
            Dirty |= ScreenDpi.Data != newScreenDpi;
            Dirty |= ScreenOrientation.Data != newScreenOrientation;
            Dirty |= ScreenResolutionWidth.Data != newScreenResolutionWidth;
            Dirty |= ScreenResolutionHeight.Data != newScreenResolutionHeight;
            ScreenWidth.Data = newScreenWidth;
            ScreenHeight.Data = newScreenHeight;
            ScreenDpi.Data = newScreenDpi;
            ScreenOrientation.Data = newScreenOrientation;
            ScreenResolutionWidth.Data = newScreenResolutionWidth;
            ScreenResolutionHeight.Data = newScreenResolutionHeight;
            if (Dirty) {
                Entities.ForEach((ref ScreenInfo screenInfo) =>
                {
                    screenInfo.screen = new Unity.Mathematics.int2(newScreenWidth, newScreenHeight);
                    screenInfo.resolution = new Unity.Mathematics.int2(newScreenResolutionWidth, newScreenResolutionHeight);
                    screenInfo.dpi = newScreenDpi;
                    screenInfo.orientation = newScreenOrientation;
                }).Run();
                Dirty = false;
            }
        }



    }
}