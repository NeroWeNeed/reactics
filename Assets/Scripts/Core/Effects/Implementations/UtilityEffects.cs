using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.Effects {
    public interface IUtilityEffect : IEffect { }
    public interface IUtilityEffect<T> : IUtilityEffect, IEffect<T> where T : struct {
    }

    /// <summary>
    /// Utility Effect for chaining together effects.
    /// </summary>
    public struct LinearEffect : IUtilityEffect<MapBodyTarget>, IUtilityEffect<Point>, IUtilityEffect<MapBodyDirection> {
        public int effect;
        public int next;
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<MapBodyTarget> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyTarget>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<Point>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<MapBodyDirection> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyDirection>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        private JobHandle ScheduleJob<TTarget>(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<TTarget> payload, EntityCommandBuffer entityCommandBuffer) where TTarget : struct {
            var targetEffect = (IEffect<TTarget>)effectAsset.effect[effect];
            var targetJobHandle = targetEffect.ScheduleJob(handle, entityManager, effectAsset, effectResource, effect, payload, entityCommandBuffer);
            if (next >= 0) {
                return ((IEffect<TTarget>)effectAsset.effect[next]).ScheduleJob(targetJobHandle, entityManager, effectAsset, effectResource, next, payload, entityCommandBuffer);
            }
            else {
                return targetJobHandle;
            }
        }
    }
    /// <summary>
    /// Utility effect for assigning runtime variables to effects.
    /// </summary>
    public struct VariableEffect : IUtilityEffect<MapBodyTarget>, IUtilityEffect<Point>, IUtilityEffect<MapBodyDirection> {

        public int effect;
        public int variableDataIndex;
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<MapBodyTarget> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyTarget>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<Point>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<MapBodyDirection> payload, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyDirection>(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);
        public unsafe JobHandle ScheduleJob<TTarget>(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<TTarget> payload, EntityCommandBuffer entityCommandBuffer) where TTarget : struct {

            var variables = effectAsset.variables[variableDataIndex];
            //TODO: Might have to copy, unsure
            var targetEffect = (IEffect<TTarget>)effectAsset.effect[effect];
            var gcHandle = GCHandle.Alloc(targetEffect, GCHandleType.Weak);

            NativeHashMap<BlittableGuid, IntPtr> sources = new NativeHashMap<BlittableGuid, IntPtr>(8, Allocator.Temp)
            {
                [typeof(TTarget).GUID] = (IntPtr)UnsafeUtility.AddressOf(ref payload.target)
            };
            variables.SetVariables(gcHandle.AddrOfPinnedObject(), sources);
            sources.Dispose();
            gcHandle.Free();
            return targetEffect.ScheduleJob(handle, entityManager, effectAsset, effectResource, effectIndex, payload, entityCommandBuffer);

        }
    }


}