using Unity.Entities;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIHierarchyManagementSystem))]
    public class UILayoutSystemGroup : ComponentSystemGroup { }
}