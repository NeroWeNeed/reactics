using Unity.Entities;

namespace Reactics.Core.Input {

    /// <summary>
    /// Entity Tag for inputs that should be processed in the simulation. These inputs are synced with other clients. Ideally Simulation Inputs should only affect entities with the GameState component.
    /// </summary>
    public struct SimulationInput : IComponentData {
        public ulong frame;
        //public bool commandQueued;
    }
}