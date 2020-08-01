using Reactics.Commons;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.UI {
    /// <summary>
    /// System for updating the status of the screen
    /// </summary>
    [UpdateInGroupFirst(typeof(UISystemGroup))]
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
        public static readonly string UI_CAMERA_TAG = "UICamera";
        public bool Dirty { get; set; }
        protected override void OnCreate() {
            Debug.Log(Screen.width);
            ScreenWidth.Data = Screen.width;
            ScreenHeight.Data = Screen.height;
            ScreenResolutionWidth.Data = Screen.currentResolution.width;
            ScreenResolutionHeight.Data = Screen.currentResolution.height;
            ScreenDpi.Data = Screen.dpi;
            ScreenOrientation.Data = Screen.orientation;
            Dirty = true;
            RequireSingletonForUpdate<ScreenInfo>();

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
                SetSingleton(new ScreenInfo
                {
                    screen = new Unity.Mathematics.int2(newScreenWidth, newScreenHeight),
                    resolution = new Unity.Mathematics.int2(newScreenResolutionWidth, newScreenResolutionHeight),
                    dpi = newScreenDpi,
                    orientation = newScreenOrientation
                });
                Dirty = false;
            }
        }



    }
}