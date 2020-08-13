using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public class UISystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIHierarchyManagementSystem))]
    [UpdateAfter(typeof(UIScreenInfoSystem))]
    [UpdateAfter(typeof(UISizeProviderSystemGroup))]
    public class UILayoutSystemGroup : ComponentSystemGroup { }
    [UpdateBefore(typeof(UIMeshBuilderSystem))]
    [UpdateAfter(typeof(UILayoutSystemGroup))]
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UIMeshProviderSystemGroup : ComponentSystemGroup { }
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UISizeProviderSystemGroup : ComponentSystemGroup { }
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UISizeProviderSystemGroup))]
    [UpdateBefore(typeof(UIUpdatePropagationSystemGroup))]
    public class UIUpdateNotifierSystemGroup : ComponentSystemGroup { }
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UISizeProviderSystemGroup))]
    [UpdateBefore(typeof(UILayoutSystemGroup))]
    [UpdateAfter(typeof(UIUpdateNotifierSystemGroup))]
    public class UIUpdatePropagationSystemGroup : ComponentSystemGroup { }
}