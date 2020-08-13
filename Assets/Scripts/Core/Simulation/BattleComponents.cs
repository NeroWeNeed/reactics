using Unity.Entities;

namespace Reactics.Core.Battle {
    /// <summary>
    /// Attach to any entity that's considered a player input and would affect the simulation
    /// </summary>
    public struct PlayerInput : IComponentData { }
}