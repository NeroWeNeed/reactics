using Unity.Entities;

namespace Reactics.Battle
{
    /// <summary>
    /// Entity Tag for inputs that should only be processed locally, and don't require syncing with other clients. Ideally Client Inputs should not affect entities with the GameState component.
    /// </summary>
    public struct ClientInput : IComponentData
    {

    }

    /// <summary>
    /// Entity Tag for inputs that should be processed in the simulation. These inputs are synced with other clients. Ideally Simulation Inputs should only affect entities with the GameState component.
    /// </summary>
    public struct SimulationInput : IComponentData
    {
        public ulong frame;
    }
}