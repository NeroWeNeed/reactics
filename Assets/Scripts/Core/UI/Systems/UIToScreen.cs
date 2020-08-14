using System.Runtime.CompilerServices;
using Reactics.Core.Camera;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {

    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIMeshBuilderSystem))]
    public class UIToScreenSystem : SystemBase {
        private UIScreenInfoSystem screenInfoSystem;
        private EntityQuery resolvedBoxChangeQuery;
        private EntityQuery cameraChangeQuery;
        private EntityQuery cameraQuery;

        protected override void OnCreate() {
            screenInfoSystem = World.GetOrCreateSystem<UIScreenInfoSystem>();
            resolvedBoxChangeQuery = GetEntityQuery(ComponentType.ReadOnly<UIResolvedBox>(), ComponentType.ReadWrite<LocalToWorld>(), ComponentType.ReadOnly<LocalToScreen>());
            resolvedBoxChangeQuery.SetChangedVersionFilter(ComponentType.ReadOnly<UIResolvedBox>());
            cameraChangeQuery = GetEntityQuery(ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadOnly<CameraReference>(), ComponentType.ReadOnly<CameraTag>());
            cameraChangeQuery.SetChangedVersionFilter(ComponentType.ReadOnly<LocalToWorld>());
            cameraChangeQuery.SetSharedComponentFilter(new CameraTag { Value = UIScreenInfoSystem.UI_CAMERA_TAG });
            cameraQuery = GetEntityQuery(ComponentType.ReadOnly<CameraReference>(), ComponentType.ReadOnly<CameraTag>());
            cameraQuery.SetSharedComponentFilter(new CameraTag { Value = UIScreenInfoSystem.UI_CAMERA_TAG });
        }

        protected override void OnUpdate() {

            new Layoutjob
            {
                forward = screenInfoSystem.UICamera.transform.forward,
                position = screenInfoSystem.UICamera.transform.position,
                rotation = screenInfoSystem.UICamera.transform.rotation,
                up = screenInfoSystem.UICamera.transform.up,
                right = screenInfoSystem.UICamera.transform.right,
                drawDistance = screenInfoSystem.UICamera.nearClipPlane,
                cameraSize = new float2(screenInfoSystem.UICamera.orthographicSize * screenInfoSystem.UICamera.aspect, screenInfoSystem.UICamera.orthographicSize),
                localToWorldHandle = GetComponentTypeHandle<LocalToWorld>(false),
                resolvedBoxHandle = GetComponentTypeHandle<UIResolvedBox>(true),
                lastSystemVersion = cameraChangeQuery.CalculateChunkCount() > 0 ? 0 : LastSystemVersion
            }.Schedule(resolvedBoxChangeQuery, Dependency).Complete();
        }
        public struct Layoutjob : IJobChunk {
            public float3 forward;
            public float3 position;
            public quaternion rotation;
            public float3 up;
            public float3 right;
            public float drawDistance;
            public float2 cameraSize;
            public ComponentTypeHandle<LocalToWorld> localToWorldHandle;

            [ReadOnly]
            public ComponentTypeHandle<UIResolvedBox> resolvedBoxHandle;
            public uint lastSystemVersion;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                if (!chunk.DidChange(resolvedBoxHandle, lastSystemVersion))
                    return;
                var ltw = chunk.GetNativeArray(localToWorldHandle);
                var rbh = chunk.GetNativeArray(resolvedBoxHandle);
                for (int i = 0; i < ltw.Length; i++) {
                    ltw[i] = new LocalToWorld
                    {
                        Value = float4x4.TRS(
                            position +
                            (forward * (1 + drawDistance)) +
                            (right * UIAnchor.TOP_LEFT.X(cameraSize.x)) +
                            (up * UIAnchor.TOP_LEFT.Y(cameraSize.y)) +
                            GetTranslation(rbh[i].Position, up, right, rbh[i].Size, UIAnchor.TOP_LEFT), rotation, 1)
                    };
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float3 GetTranslation(float2 location, float3 up, float3 right, float2 meshSize, UIAnchor meshAnchor) {
                return (right * location.x) + (up * -(location.y + meshAnchor.Y(meshSize.y)));
            }

        }

    }



}
