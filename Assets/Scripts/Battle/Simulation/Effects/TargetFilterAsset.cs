using System.Collections.Generic;
using Reactics.Battle.Map;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Battle
{
    public class TargetFilterAsset : ScriptableObject
    {
        [SerializeField,HideInInspector]
        public TargetType type;
        [SerializeReference]
        public ITargetFilter[] filter;
    }
    public abstract class BaseTargetFilterAsset : ScriptableObject { }
    public abstract class BaseTargetFilterAsset<T> : BaseTargetFilterAsset where T : unmanaged
    {
        [SerializeReference]
        protected List<ITargetFilter<T>> filter;
        public NativeArray<T> Filter(Entity entitySourceBody, EntityManager entityManager, NativeArray<T> initial, Allocator allocator = Allocator.Temp)
        {
            var sourceBody = entityManager.GetComponentData<MapBody>(entitySourceBody);
            var mapEntity = entityManager.GetComponentData<MapElement>(entitySourceBody).value;
            var map = entityManager.GetComponentData<MapData>(mapEntity);
            var targets = new NativeList<T>(allocator);
            targets.AddRange(initial);
            for (int i = 0; i < filter.Count && targets.Length > 0; i++)
            {
                filter[i].Filter(entitySourceBody, sourceBody, mapEntity, map, targets);
            }
            return targets.AsArray();
        }

    }

}