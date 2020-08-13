using Reactics.Core.Commons;
using Unity.Entities;
namespace Reactics.Core.Camera {
    [GenerateAuthoringComponent]
    public struct ControlSchemeData : IComponentData {
        public ControlSchemes currentControlScheme;
    }

}