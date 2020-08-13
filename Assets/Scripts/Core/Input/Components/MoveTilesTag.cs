using Unity.Entities;
namespace Reactics.Core.Input {
    public struct MoveTilesTag : IComponentData {
        //sorta works for highlighting tiles but y'know
        public bool toggle;
    }
}