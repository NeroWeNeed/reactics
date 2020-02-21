using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;


namespace Reactics.Battle
{

    
    [UpdateInGroup(typeof(BattleSimulationSystemGroup))]
    public class MapSystemGroup : ComponentSystemGroup
    {


    }
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    public class MapRenderSystemGroup : ComponentSystemGroup
    {

    }


}