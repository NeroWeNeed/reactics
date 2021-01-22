using Unity.Entities;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public class UISystemGroup : ComponentSystemGroup { }
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public class UIInitializationSystemGroup : ComponentSystemGroup { }
}