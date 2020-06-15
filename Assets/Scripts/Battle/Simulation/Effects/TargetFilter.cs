using Reactics.Battle.Map;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Reactics.Battle
{
    public interface ITargetFilter
    {
       
    }
    public interface ITargetFilter<TTarget> : ITargetFilter where TTarget : unmanaged
    {
        void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<TTarget> targets);
    }

}