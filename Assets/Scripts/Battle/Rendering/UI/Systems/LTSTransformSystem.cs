using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reactics.UI
{
    [UpdateInGroup(typeof(UIRenderingSystemGroup))]
    public class LTSTransformSystem : JobComponentSystem
    {
        public Camera Camera { get; private set; }
        public Camera UICamera { get; private set; }
        private float2 cameraSize;
        private float2 nativeScreenSize;
        public int distance = 1;
        protected override void OnCreate()
        {

            foreach (Camera t in Object.FindObjectsOfType(typeof(Camera)))
            {
                if (t.tag == "MainCamera")
                {
                    Camera = t;
                    break;
                }
            }
            if (Camera == null)
            {
                throw new UnityException("Missing Main Camera in Scene");
            }

            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(LocalToScreen)), typeof(LocalToWorld)));
        }
        protected override void OnStartRunning()
        {

            if (UICamera != null)
            {
                Camera.GetUniversalAdditionalCameraData().cameraStack.Remove(UICamera);
                Object.Destroy(UICamera.gameObject);
            }
            UICamera = CreateUICamera();
        }
        protected override void OnStopRunning()
        {

            if (UICamera != null)
            {
                Camera.GetUniversalAdditionalCameraData().cameraStack.Remove(UICamera);
                Object.Destroy(UICamera.gameObject);
            }

        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            cameraSize.x = UICamera.orthographicSize * UICamera.aspect;
            cameraSize.y = UICamera.orthographicSize;
            nativeScreenSize.x = UICamera.scaledPixelWidth;
            nativeScreenSize.y = UICamera.scaledPixelHeight;
            var job = new TransformJob
            {
                cameraForward = UICamera.transform.forward,
                cameraPosition = UICamera.transform.position,
                cameraRotation = UICamera.transform.rotation,
                cameraUp = UICamera.transform.up,
                cameraRight = UICamera.transform.right,
                cameraSize = cameraSize,
                screenSize = nativeScreenSize

            };
            return job.Schedule(this, inputDeps);

        }
        protected override void OnDestroy()
        {
            if (UICamera != null)
            {
                Camera.GetUniversalAdditionalCameraData().cameraStack.Remove(UICamera);
                Object.Destroy(UICamera.gameObject);
            }

        }
        [BurstCompile]
        public struct TransformJob : IJobForEach<LocalToScreen, LocalToWorld>
        {
            [ReadOnly]
            public float3 cameraForward;
            [ReadOnly]
            public float3 cameraPosition;
            public float3 cameraUp;
            [ReadOnly]
            public float3 cameraRight;
            [ReadOnly]
            public quaternion cameraRotation;
            [ReadOnly]
            public float2 cameraSize;
            [ReadOnly]
            public float2 screenSize;
            [ReadOnly]
            public int drawDistance;
            public void Execute(ref LocalToScreen lts, ref LocalToWorld ltw)
            {
                ltw.Value = float4x4.TRS(cameraPosition + cameraForward*drawDistance + (cameraRight * lts.screenAnchor.X(cameraSize.x)) + (cameraUp * lts.screenAnchor.Y(cameraSize.y)) + GetTranslation(ref lts.location, ref cameraUp, ref cameraRight, ref cameraSize, ref screenSize, ref lts.extents, ref lts.localAnchor), cameraRotation, 1);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetTranslation(ref float2 location, ref float3 up, ref float3 right, ref float2 cameraSize, ref float2 nativeSize, ref float2 meshSize, ref UIAnchor meshAnchor)
        {
            return (right * ((location.x + meshAnchor.X(meshSize.x)) * (cameraSize.x * 2) / nativeSize.x)) + (up * ((location.y + meshAnchor.Y(meshSize.y)) * (cameraSize.y * 2) / nativeSize.y));
        }


        private Camera CreateUICamera()
        {
            UICamera = new GameObject("UI Camera", typeof(Camera)).GetComponent<Camera>();
            UICamera.depth = 0;
            UICamera.orthographic = true;
            int layer = LayerMask.NameToLayer("UI");
            UICamera.cullingMask = layer;
            UICamera.gameObject.layer = layer;
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.transform.SetParent(Camera.transform, false);
            Camera.cullingMask = (Camera.cullingMask & int.MaxValue) - (1 << layer);
            UICamera.cullingMask = 1 << layer;
            UICamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;

            Camera.GetUniversalAdditionalCameraData().cameraStack.Add(UICamera);
            return UICamera;
        }
    }
}