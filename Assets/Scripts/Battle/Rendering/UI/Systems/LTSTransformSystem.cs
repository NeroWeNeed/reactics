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
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIPaintSystemGroup))]

    public class LTSTransformSystem : SystemBase
    {


        public UIEnvironmentSystem uiEnvironmentSystem;
        protected override void OnCreate()
        {
            uiEnvironmentSystem = World.GetOrCreateSystem<UIEnvironmentSystem>();
            RequireSingletonForUpdate<UIEnvironmentData>();
        }

        protected override void OnUpdate()
        {
            
            Entities.WithNone<LocalToWorld>().WithAll<LocalToScreen>().ForEach((Entity entity) =>
            {
                EntityManager.AddComponent<LocalToWorld>(entity);
            }).WithStructuralChanges().WithoutBurst().Run();
            float3 cameraForward = uiEnvironmentSystem.UICamera.transform.forward;
            float3 cameraPosition = uiEnvironmentSystem.UICamera.transform.position;
            quaternion cameraRotation = uiEnvironmentSystem.UICamera.transform.rotation;
            float3 cameraUp = uiEnvironmentSystem.UICamera.transform.up;
            float3 cameraRight = uiEnvironmentSystem.UICamera.transform.right;
            float elapsedTime = (float)Time.ElapsedTime;
            float drawDistance = uiEnvironmentSystem.UICamera.nearClipPlane;
            float2 cameraSize = new float2(uiEnvironmentSystem.UICamera.orthographicSize * uiEnvironmentSystem.UICamera.aspect, uiEnvironmentSystem.UICamera.orthographicSize);

            float2 screenSize = new float2(uiEnvironmentSystem.UICamera.scaledPixelWidth, uiEnvironmentSystem.UICamera.scaledPixelHeight);
            Entities.WithChangeFilter<LocalToScreen>().ForEach((ref LocalToWorld ltw, in LocalToScreen lts) =>
            {
                ltw.Value = float4x4.TRS(cameraPosition + (cameraForward * (1 + drawDistance)) + (cameraRight * lts.screenAnchor.X(cameraSize.x)) + (cameraUp * lts.screenAnchor.Y(cameraSize.y)) + GetTranslation(lts.location, cameraUp, cameraRight, cameraSize, screenSize, lts.extents, lts.localAnchor), cameraRotation, 1);
            }).ScheduleParallel();
            Entities.WithNone<Rotation>().WithAll<LocalToWorld, FaceScreen>().ForEach((Entity entity) =>
            {
                EntityManager.AddComponent<Rotation>(entity);
            }).WithStructuralChanges().WithoutBurst().Run();
            Entities.WithAll<LocalToWorld,FaceScreen>().ForEach((Entity entity, ref Rotation rotation) =>
            {
                rotation.Value = cameraRotation;
            }).ScheduleParallel();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetTranslation(float2 location, float3 up, float3 right, float2 cameraSize, float2 nativeSize, float2 meshSize, UIAnchor meshAnchor)
        {

            return (right * (location.x - meshAnchor.X(meshSize.x))) + (up * (location.y - meshAnchor.Y(meshSize.y)));
        }


    }


}