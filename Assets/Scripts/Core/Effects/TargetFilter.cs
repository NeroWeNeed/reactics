using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Reactics.Core.Effects {
    public interface ITargetFilter {

    }
    [ConcreteTypeColor("#F03F00", typeof(MapBodyTarget))]
    [ConcreteTypeColor("#4257E3", typeof(Point))]
    [ConcreteTypeColor("#76E32B", typeof(MapBodyDirection))]
    public interface ITargetFilter<TTarget> : ITargetFilter where TTarget : struct {
        void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<TTarget> targets);
    }

}