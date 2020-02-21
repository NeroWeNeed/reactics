using Unity.Entities;

namespace Reactics.Battle {
    /// <summary>
    /// Responsible for generating output after simulation. Since Simulation runs at a different than target frame rate in most cases,  Systems should be ready to deal with SubFrames.
    /// </summary>
    [UpdateAfter(typeof(Unity.Entities.SimulationSystemGroup))]
    [UpdateInGroup(typeof(BattleSystemGroup))]
    public class RenderingSystemGroup : ComponentSystemGroup {

    }
}