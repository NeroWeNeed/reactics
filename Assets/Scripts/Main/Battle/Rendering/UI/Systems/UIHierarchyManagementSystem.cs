using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Core.UI {

    [UpdateInGroup(typeof(UISystemGroup))]
    public class UIHierarchyManagementSystem : SystemBase {
        protected override void OnUpdate() {
            var parentData = GetComponentDataFromEntity<UIParent>(false);
            var parentSystemData = GetComponentDataFromEntity<UISystemStateParent>(false);
            var childData = GetBufferFromEntity<UIChild>(false);
            var childSystemData = GetBufferFromEntity<UISystemStateChild>(false);
            Entities.WithChangeFilter<UIChild>().ForEach((Entity entity, DynamicBuffer<UIChild> children, DynamicBuffer<UISystemStateChild> systemStateChildren) =>
            {
                EntityCommons.SymmetricDifference(children.AsNativeArray(), systemStateChildren.AsNativeArray(), out NativeArray<UIChild> addedChildren, out NativeArray<UISystemStateChild> removedChildren);
                for (int i = 0; i < addedChildren.Length; i++) {
                    if (parentData.HasComponent(addedChildren[i].value))
                        parentData[addedChildren[i].value] = entity;
                    if (parentSystemData.HasComponent(addedChildren[i].value))
                        parentSystemData[addedChildren[i].value] = entity;
                }
                for (int i = 0; i < removedChildren.Length; i++) {
                    if (parentData.HasComponent(removedChildren[i].value)) {
                        var oldParentData = parentData[removedChildren[i].value];
                        if (oldParentData.value == entity)
                            parentData[removedChildren[i].value] = Entity.Null;
                    }
                    if (parentSystemData.HasComponent(removedChildren[i].value)) {
                        var oldParentSystemData = parentSystemData[removedChildren[i].value];
                        if (oldParentSystemData.value == entity)
                            parentSystemData[removedChildren[i].value] = Entity.Null;
                    }
                }
                systemStateChildren.Clear();
                systemStateChildren.CopyFrom(children.Reinterpret<UISystemStateChild>());
            }).ScheduleParallel();
            Entities.WithChangeFilter<UIParent>().ForEach((Entity entity, ref UISystemStateParent systemStateParent, in UIParent parent) =>
            {
                if (!parent.Equals(systemStateParent)) {
                    if (childData.HasComponent(parent.value)) {
                        var childDataBuffer = childData[parent.value];
                        if (!childDataBuffer.Contains(entity))
                            childDataBuffer.Add(entity);
                    }
                    if (childSystemData.HasComponent(parent.value)) {
                        var childSystemDataBuffer = childSystemData[parent.value];
                        if (!childSystemDataBuffer.Contains(entity))
                            childSystemDataBuffer.Add(entity);
                    }
                    if (childData.HasComponent(systemStateParent.value)) {
                        var childDataBuffer = childData[systemStateParent.value];
                        childDataBuffer.Remove(entity);
                    }
                    if (childSystemData.HasComponent(systemStateParent.value)) {
                        var childSystemDataBuffer = childSystemData[systemStateParent.value];
                        childSystemDataBuffer.Remove(entity);
                    }
                    systemStateParent = parent;
                }
            }).ScheduleParallel();
        }
    }
}