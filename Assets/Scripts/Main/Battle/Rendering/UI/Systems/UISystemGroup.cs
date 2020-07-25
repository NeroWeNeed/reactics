using Unity.Entities;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class UISystemGroup : ComponentSystemGroup { }
}