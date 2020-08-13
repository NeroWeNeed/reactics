using Unity.Entities;

namespace Reactics.Core.Input {
    /// <summary>
    /// Entity Tag for inputs that should only be processed locally, and don't require syncing with other clients. Ideally Client Inputs should not affect entities with the GameState component.
    /// </summary>
    public struct ClientInput : IComponentData {

    }
}