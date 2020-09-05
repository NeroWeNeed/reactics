using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
namespace Reactics.Core.Effects {


    [CreateAssetMenu(fileName = "Effect", menuName = "Reactics/Effect", order = 0)]
    public abstract class EffectAsset : ScriptableObject {
        [SerializeReference, HideInInspector]
        public IEffect[] components;
        [SerializeField, HideInInspector]
        public int[] roots;
        [SerializeField, HideInInspector]
        public VariableOperationSequence[] operations;
        [SerializeField, HideInInspector]
        public Variable[] variables;


        public int EffectCount { get => components.Length; }
        public int RootCount { get => roots.Length; }
        private void OnValidate() {
            if (operations == null)
                operations = Array.Empty<VariableOperationSequence>();
            if (roots == null)
                roots = Array.Empty<int>();
            if (components == null)
                components = Array.Empty<IEffect>();
            if (variables == null)
                variables = Array.Empty<Variable>();
        }

        public BlobPtr<byte> Allocate() {
            var size = 0;
            foreach (var c in components) {
                size += UnsafeUtility.SizeOf(c.GetType());
            }
            UnsafeUtility.Malloc(size)
        }
    }
    public unsafe abstract class EffectAsset<TTarget> : EffectAsset where TTarget : struct {


        public bool ScheduleJob(
                    JobHandle input,
                    EntityManager entityManager,
                    ComponentDataFromEntity<EffectIndex> indexData,
                    ComponentDataFromEntity<MapBody> mapBodyData,
                    ComponentDataFromEntity<MapData> mapData,
                    EntityCommandBuffer ecb,
                    Entity entity,
                    Effect reference,
                    EffectTarget<TTarget> target,
                    EffectSource source,
                    MapElement mapElement,
                    out JobHandle jobHandle) {
            var payload = new EffectPayload<TTarget>
            {
                sourceEntity = source.value,
                source = mapBodyData[source.value],
                mapEntity = mapElement.value,
                map = mapData[mapElement.value],
                target = target.value
            };


            //TODO: Might have to copy, unsure


            NativeArray<Variable> variables = new NativeArray<Variable>(this.variables, Allocator.Temp);

            NativeHashMap<BlittableGuid, IntPtr> sources = new NativeHashMap<BlittableGuid, IntPtr>(8, Allocator.Temp)
            {
                [typeof(TTarget).GUID] = (IntPtr)UnsafeUtility.AddressOf(ref payload.target)
            };
            //variables.Apply(gcHandle.AddrOfPinnedObject(), sources);
            foreach (var operation in operations) {
                operation.Invoke(sources, variables,)
            }
            sources.Dispose();




            if (indexData.HasComponent(entity)) {
                var index = indexData[entity].value;
                if (index >= 0 && index < EffectCount) {
                    var component = (IEffect<TTarget>)components[index];
                    var job = component.ScheduleJob(input, entityManager, this, reference.value, index, payload, ecb);
                    if (!job.Equals(input)) {
                        jobHandle = job;
                        return true;
                    }
                }
            }
            else {
                NativeList<JobHandle> jobs = new NativeList<JobHandle>(RootCount, Allocator.Temp);
                for (int r = 0; r < RootCount; r++) {
                    var rootIndex = roots[r];
                    var component = (IEffect<TTarget>)components[rootIndex];
                    var job = component.ScheduleJob(input, entityManager, this, reference.value, rootIndex, payload, ecb);
                    if (!job.Equals(input))
                        jobs.Add(job);
                }
                if (jobs.Length > 0) {
                    jobHandle = JobHandle.CombineDependencies(jobs);
                    return true;
                }
            }
            jobHandle = default;
            return false;
        }

        private NativeArray<IntPtr> GetComponentPointers() {
            var output = new NativeArray<BlobP>(components.Length, Allocator.Temp);
            BlobBuilder blob = new BlobBuilder(Allocator.Temp);
            blob.Allocate()
        }
    }
    public class PointEffectAsset : EffectAsset<PointTarget> { }
    public class MapBodyEffectAsset : EffectAsset<MapBodyTarget> { }
    public class DirectionEffectAsset : EffectAsset<MapBodyDirection> { }

}