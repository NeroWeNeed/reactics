using Unity.Entities;

namespace Reactics.Battle {
    /// <summary>
    /// Component group for processing player inputs created in the PlayerInputSystemGroup. 
    /// </summary>
    [UpdateAfter(typeof(PlayerInputSystemGroup))]
    [UpdateInGroup(typeof(BattleSystemGroup))]
    public class PlayerInputProcessingSystemGroup : ComponentSystemGroup {

    }
}