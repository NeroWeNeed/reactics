using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Util
{
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToScreen : IComponentData
    {
        public int cameraId;

        public LocalToScreen(Camera camera)
        {
            cameraId = camera.GetInstanceID();

        }

    }

    public class TRSToLocalToScreenSystem : JobComponentSystem
    {
        private NativeHashMap<int, float4x4> cameras;

        protected override void OnCreate()
        {

        }
        protected override void OnStartRunning()
        {
            if (cameras.IsCreated)
                cameras.Dispose();
            cameras = new NativeHashMap<int, float4x4>(4, Allocator.Persistent);
        }
        protected override void OnStopRunning()
        {
            if (cameras.IsCreated)
                cameras.Dispose();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CollectViewMatrices();
            var job = new LocalToScreenTransform
            {
                translation = GetComponentDataFromEntity<Translation>(true),
                rotation = GetComponentDataFromEntity<Rotation>(true),
                scale = GetComponentDataFromEntity<Scale>(true),
                cameras = cameras
            };

            return job.Schedule(this, inputDeps);

        }

        private void CollectViewMatrices()
        {
            cameras.Clear();
            foreach (var item in Camera.allCameras)
            {
                cameras[item.GetInstanceID()] = (item.projectionMatrix*item.worldToCameraMatrix).inverse;
            }
            
        }

        public struct LocalToScreenTransform : IJobForEachWithEntity<LocalToWorld, LocalToScreen>
        {
            [ReadOnly]
            public ComponentDataFromEntity<Translation> translation;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> rotation;
            [ReadOnly]
            public ComponentDataFromEntity<Scale> scale;

            [ReadOnly]
            
            public NativeHashMap<int, float4x4> cameras;

            public void Execute(Entity entity, int index, ref LocalToWorld ltw, ref LocalToScreen lts)
            {

                if (!cameras.ContainsKey(lts.cameraId))
                    return;




                float4x4 value = (float4x4)cameras[lts.cameraId]; ;
                if (translation.Exists(entity))
                    value *= (float4x4)Matrix4x4.Translate(translation[entity].Value);
                if (rotation.Exists(entity))
                    value *= (float4x4)Matrix4x4.Rotate(rotation[entity].Value);
                if (scale.Exists(entity))
                    value *= scale[entity].Value;
                ltw.Value = value;
            }
        }
    }
}