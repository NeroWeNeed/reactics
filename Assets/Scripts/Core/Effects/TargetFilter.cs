using Reactics.Core.Map;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Reactics.Core.Effects {
    public interface ITargetFilter {

    }
    public interface ITargetFilter<TTarget> : ITargetFilter where TTarget : struct {
        void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<TTarget> targets);
    }

}