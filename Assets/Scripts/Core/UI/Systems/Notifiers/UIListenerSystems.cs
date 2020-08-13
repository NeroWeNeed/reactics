using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.UI {

    [UpdateInGroup(typeof(UIUpdateNotifierSystemGroup))]
    public class UISizeVersionNotificationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UISize>().ForEach((ref UILayoutVersion version) => version.Update()).Schedule();
        }
    }

    [UpdateInGroup(typeof(UIUpdateNotifierSystemGroup))]
    public class UIElementDetailsNotificationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIElementDetails>().ForEach((ref UILayoutVersion version) => version.Update()).Schedule();
        }
    }
    [UpdateInGroup(typeof(UIUpdateNotifierSystemGroup))]
    public class UILayoutNotificationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UILayout>().ForEach((ref UILayoutVersion version) => version.Update()).Schedule();
        }
    }
    [UpdateInGroup(typeof(UIUpdateNotifierSystemGroup))]
    public class UITextNotificationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIText>().ForEach((ref UITextVersion version) => version.Update()).Schedule();
        }
    }
    [UpdateInGroup(typeof(UIUpdateNotifierSystemGroup))]
    public class UIFontNotificationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIFont>().ForEach((ref UITextVersion version) => version.Update()).Schedule();
        }
    }

    [UpdateInGroup(typeof(UIUpdatePropagationSystemGroup))]
    public abstract class BaseUIVersionPropagationSystem<TVersion> : SystemBase where TVersion : struct, IVersion, IComponentData {

        protected EntityQuery query;
        protected override void OnCreate() {
            query = CreateQuery();


        }
        protected override void OnUpdate() {
            new VersionPropagationJob
            {
                versionData = GetComponentDataFromEntity<TVersion>(false),
                parentData = GetComponentDataFromEntity<UIParent>(true),
                entityType = GetEntityTypeHandle()
            }.Schedule(query, Dependency).Complete();
        }

        protected abstract EntityQuery CreateQuery();
        private struct VersionPropagationJob : IJobChunk {
            //TODO: Might be dangerous, unsure
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<TVersion> versionData;
            [ReadOnly]
            public ComponentDataFromEntity<UIParent> parentData;
            [ReadOnly]
            public EntityTypeHandle entityType;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                foreach (var entity in chunk.GetNativeArray(entityType)) {
                    var root = entity;
                    while (parentData.HasComponent(root)) {
                        root = parentData[root];
                    }
                    if (versionData.HasComponent(root)) {
                        var oldVersion = versionData[root];
                        oldVersion.Version++;
                        versionData[root] = oldVersion;
                    }
                }
            }
        }
    }

    public class UILayoutVersionPropagationSystem : BaseUIVersionPropagationSystem<UILayoutVersion> {
        protected override EntityQuery CreateQuery() {
            var query = GetEntityQuery(typeof(UILayoutVersion));
            query.SetChangedVersionFilter(typeof(UILayoutVersion));
            return query;
        }
    }
    public class UIMeshVersionPropagationSystem : BaseUIVersionPropagationSystem<UIMeshVersion> {
        protected override EntityQuery CreateQuery() {
            var query = GetEntityQuery(typeof(UIMeshVersion));
            query.SetChangedVersionFilter(typeof(UIMeshVersion));
            return query;
        }
    }



}