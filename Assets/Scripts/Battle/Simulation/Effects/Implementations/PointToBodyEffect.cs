using System;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Battle {
    [Serializable]
    public struct PointToBodyEffect : IEffect<Point> {
        [SerializeNodeIndex(typeof(IEffect<MapBodyTarget>))]
        [SerializeField]
        public IndexReference onBody;

        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer) {
            throw new NotImplementedException();
        }
        public struct PointToBodyEffectJob : IJob {
            public void Execute() {
                /*                 if (onBody.index != -1) {
                                    var entity = entityCommandBuffer.CreateEntity();
                                    entityCommandBuffer.AddComponent(entity, new EffectIndexData(effectDataEntity, onBody.index));
                                    entityCommandBuffer.AddComponent(entity, new PointToMapBody
                                    {
                                        point = target,
                                        map = mapEntity
                                    });
                                }
                                entityCommandBuffer.DestroyEntity(cursorEntity); */
            }
        }
    }
}