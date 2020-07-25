using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    /// <summary>
    /// Simple UI Layout System created from mixing parts of Flutter's layout algorithm and Flex's layout algorithm.
    /// </summary>
/*     public class SimpleUILayoutSystem : SystemBase {
        public EntityQuery rootQuery;
        protected override void OnCreate() {
            rootQuery = GetEntityQuery(ComponentType.Exclude<UIParent>(), ComponentType.ReadOnly<UIChild>(), ComponentType.ReadWrite<UIElement>(), ComponentType.ReadOnly<SimpleUILayoutProperties>(), ComponentType.ReadOnly<UIWidth>(), ComponentType.ReadOnly<UIHeight>());
        }
        protected override void OnUpdate() {
            throw new System.NotImplementedException();
        }

        public struct LayoutJob : IJobChunk {

            public ComponentTypeHandle<UIParent> parentHandle;
            public BufferTypeHandle<UIChild> childHandle;
            public ComponentTypeHandle<UIElement> elementHandle;
            public ComponentTypeHandle<SimpleUILayoutProperties> propertiesHandle;
            public EntityTypeHandle entityHandle;
            public ComponentTypeHandle<UIWidth> widthHandle;
            public ComponentTypeHandle<UIHeight> heightHandle;
            public ComponentDataFromEntity<UIWidth> widthData;
            public ComponentDataFromEntity<UIHeight> heightData;
            public ComponentDataFromEntity<SimpleUILayoutProperties> layoutPropertyData;
            public BufferFromEntity<UIChild> childData;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
                NativeArray<SimpleUILayoutProperties> properties = chunk.GetNativeArray(propertiesHandle);
                BufferAccessor<UIChild> children = chunk.GetBufferAccessor(childHandle);
                NativeArray<UIParent> parents = chunk.GetNativeArray(parentHandle);
                NativeArray<UIWidth> widths = chunk.GetNativeArray(widthHandle);
                NativeArray<UIHeight> heights = chunk.GetNativeArray(heightHandle);

                var sizes = new NativeHashMap<Entity, float2>(8, Allocator.Temp);
                var positions = new NativeHashMap<Entity, float2>(8, Allocator.Temp);
                var availableSpaces = new NativeHashMap<Entity, float4>(8, Allocator.Temp);
                SimpleUIValueProperties valueProperties;

                for (int i = 0; i < entities.Length; i++) {

                    var constraints = new UISize
                    {
                        width = widths[i],
                        height = heights[i]
                    };
                    float4 availableSpace = constraints.RealValue<SimpleUIValueProperties>(default);
                    sizes[entities[i]] = new float2(availableSpace.z, availableSpace.w);
                    availableSpaces[entities[i]] = availableSpace;
                    valueProperties = new SimpleUIValueProperties
                    {
                        containerSize = sizes[entities[i]],
                        FontSize = 12
                    };
                    for (int j = 0; j < children[i].Length; j++) {
                        DoLayout(children[i][j], entities[i], valueProperties, sizes, positions, availableSpaces);
                    }
                }
            }
            private void DoLayout(Entity target, Entity parent, SimpleUIValueProperties parentProperties, NativeHashMap<Entity, float2> sizes, NativeHashMap<Entity, float2> positions, NativeHashMap<Entity, float4> availableSpaces) {
                var constraints = new UISize
                {
                    width = UIWidth.Unbound,
                    height = UIHeight.Unbound
                };
                if (widthData.HasComponent(target)) {
                    constraints.width = widthData[target];
                }
                if (heightData.HasComponent(target)) {
                    constraints.height = heightData[target];
                }
                var availableSpace = GetAvailableSpace(constraints, parentProperties, availableSpaces[parent]);
                availableSpaces[target] = availableSpace;

                var properties = new SimpleUIValueProperties
                {
                    containerSize = new float2(availableSpace.z, availableSpace.w),
                    FontSize = parentProperties.FontSize
                };
                float2 size = new float2(0, 0);

                if (childData.HasComponent(target)) {
                    var priorities = new NativeMultiHashMap<int, PriorityEntry>(8, Allocator.Temp);
                    foreach (var entity in childData[target]) {
                        DoLayout(entity, target, properties, sizes, positions, availableSpaces);
                        size.x += sizes[entity].x;
                        size.y += sizes[entity].y;
                        if (GetPriorityEntry(availableSpaces[entity], entity, out PriorityEntry result)) {
                            priorities.Add(layoutPropertyData.HasComponent(target) ? layoutPropertyData[target].priority : 0, result);

                        }
                    }
                    if (size.x > availableSpace.y) {
                        var keys = priorities.GetKeyArray(Allocator.Temp);
                        keys.Sort();
                        var overflow = size.x - availableSpace.y;

                        foreach (var key in keys) {


                        }
                    }
                    priorities.Dispose();
                }
                else {
                    size = new float2(availableSpace.y, availableSpace.z);
                    sizes[target] = size;
                    return;
                }

            }
            public float4 GetAvailableSpace(UISize size, SimpleUIValueProperties parentProperties, float4 parentAvailableSpace) {
                parentProperties.horizontal = true;
                var w = Bound(size.RealWidthValue(parentProperties), new float2(parentAvailableSpace.x, parentAvailableSpace.z));
                parentProperties.horizontal = false;
                var h = Bound(size.RealWidthValue(parentProperties), new float2(parentAvailableSpace.y, parentAvailableSpace.w));
                return new float4(w.x, h.x, w.y, h.y);
            }
            public float2 Bound(float2 target, float2 constraint) => target.x == target.y ? target : new float2(math.max(target.x, constraint.x), math.min(target.x, constraint.y));

            public bool GetPriorityEntry(float4 space, Entity entity, out PriorityEntry result) {
                var w = space.x != space.z;
                var h = space.y != space.w;
                if (w || h) {
                    result = new PriorityEntry
                    {
                        entity = entity,
                        width = w,
                        height = h
                    };
                    return true;
                }
                else {
                    result = default;
                    return false;

                }
            }
        }
        public struct PriorityEntry {
            public Entity entity;
            public bool width;
            public bool height;
        }
        public struct SimpleUIValueProperties : IValueProperties {
            public bool horizontal;
            public float2 containerSize;
            public float ContainerLength { get => horizontal ? containerSize.x : containerSize.y; }
            public float FontSize { get; set; }
        }
    } */
}