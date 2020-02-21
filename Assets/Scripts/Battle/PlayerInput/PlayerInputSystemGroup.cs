using Unity.Entities;

namespace Reactics.Battle
{

    /// <summary>
    /// System Group for storing Systems for determining player input. The Processing of player inputs do not go here, but go in the Player Input Processing Group. 
    /// 
    /// </summary>
    [UpdateBefore(typeof(PlayerInputProcessingSystemGroup))]
    [UpdateInGroup(typeof(BattleSystemGroup))]
    public class PlayerInputSystemGroup : ComponentSystemGroup
    {

    }
}