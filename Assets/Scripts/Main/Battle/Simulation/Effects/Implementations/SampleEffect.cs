using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Battle {
    public struct SampleEffect : IEffect<Point>, IEffectBehaviour<Point> {
        public MapLayer layer;
        public float value;
        public bool otherValue;
        [SerializeField]
        public BlittableAssetReference64 item;
        public void Invoke(Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer) {
            throw new System.NotImplementedException();
        }

        public void Invoke(EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) {
            throw new System.NotImplementedException();
        }

        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) {
            throw new System.NotImplementedException();
        }

        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) {
            throw new System.NotImplementedException();
        }
    }
}