using Unity.Entities;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIHierarchyManagementSystem))]
    [UpdateAfter(typeof(UIScreenInfoSystem))]
    [UpdateAfter(typeof(UISizeProviderSystemGroup))]
    public class UILayoutSystemGroup : ComponentSystemGroup { }
}