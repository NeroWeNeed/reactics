using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Battle
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class MapSystemGroup : ComponentSystemGroup { }
}