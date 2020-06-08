using Unity.Entities;
using Unity.Rendering;

namespace Reactics.Battle
{
    /// <summary>
    /// Responsible for generating output after simulation. Since Simulation runs at a different than target frame rate in most cases,  Systems should be ready to deal with SubFrames.
    /// </summary>

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    //[UpdateBefore(typeof(HybridRendererSystem))]
    public class RenderingSystemGroup : ComponentSystemGroup
    {

    }
}