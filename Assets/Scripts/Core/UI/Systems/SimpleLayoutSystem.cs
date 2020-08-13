using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UILayoutSystemGroup))]

    public class SimpleLayoutSystem : SystemBase {
        private EntityQuery query;
        private EntityCommandBufferSystem entityCommandBufferSystem;
        public JobHandle sizeProviderJobHandle;
        protected override void OnCreate() {

            query = GetEntityQuery(ComponentType.ReadOnly<UILayoutVersion>(), ComponentType.ReadWrite<UIMeshVersion>(), ComponentType.Exclude<UIParent>());
            query.SetChangedVersionFilter(typeof(UILayoutVersion));
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            new LayoutJob
            {
                entityTypeHandle = GetEntityTypeHandle(),
                childData = GetBufferFromEntity<UIChild>(true),
                resolvedBox = GetComponentDataFromEntity<UIResolvedBox>(true),
                sizeData = GetComponentDataFromEntity<UISize>(true),
                layoutData = GetComponentDataFromEntity<UILayout>(true),
                elementDetailsData = GetComponentDataFromEntity<UIElementDetails>(true),
                entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(query, Dependency).Complete();
            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        [BurstCompile]
        public struct LayoutJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityTypeHandle;
            [ReadOnly]
            public BufferFromEntity<UIChild> childData;
            [ReadOnly]
            public ComponentDataFromEntity<UIResolvedBox> resolvedBox;
            [ReadOnly]
            public ComponentDataFromEntity<UISize> sizeData;
            [ReadOnly]
            public ComponentDataFromEntity<UILayout> layoutData;
            [ReadOnly]
            public ComponentDataFromEntity<UIElementDetails> elementDetailsData;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var boxes = new NativeHashMap<Entity, BoxModel>(8, Allocator.Temp)
                {
                    [Entity.Null] = new BoxModel
                    {
                        margin = float4.zero,
                        borderWidth = float4.zero,
                        padding = float4.zero,
                        content = new float2(float.PositiveInfinity, float.PositiveInfinity)
                    }
                };
                var positions = new NativeHashMap<Entity, float2>(8, Allocator.Temp)
                {
                    [Entity.Null] = new float2(0, 0)
                };
                var entities = chunk.GetNativeArray(entityTypeHandle);
                for (int i = 0; i < entities.Length; i++) {
                    boxes[entities[i]] = Process(entities[i], Entity.Null, UI.Layout.Horizontal, boxes, positions);
                    positions[entities[i]] = float2.zero;
                    ResolveBox(chunkIndex, entities[i], boxes, positions);
                }
                boxes.Dispose();
                positions.Dispose();
            }
            public void ResolveBox(int chunkIndex, Entity entity, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                entityCommandBuffer.SetComponent(chunkIndex, entity, new UIResolvedBox
                {
                    value = new float4(positions[entity], positions[entity] + boxes[entity].Size)
                });

                if (childData.HasComponent(entity)) {
                    foreach (var child in childData[entity]) {
                        ResolveBox(chunkIndex, child, boxes, positions);
                    }
                }
            }
            public BoxModel Process(Entity target, Entity parent, Layout parentLayout, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                var props = new ValueProperties
                {
                    parentInnerBox = boxes[parent].content,
                    FontSize = 12,
                    RootFontSize = 12
                };
                var box = GetBoxModel(target, boxes[parent].content, props);
                var constraints = sizeData.HasComponent(target) ? sizeData[target].RealValues(props) : UISize.Unbounded.RealValues(props);
                box.content = Constrain(boxes[parent].content, constraints);
                if (childData.HasComponent(target)) {
                    var layout = layoutData.HasComponent(target) ? layoutData[target].value : parentLayout;
                    var children = childData[target];
                    boxes[target] = box;
                    foreach (var child in children) {
                        boxes[child] = Process(child, target, layout, boxes, positions);
                    }
                    box.content = Constrain(Layout(target, constraints, children, layout, boxes, positions), constraints);
                }
                return box;

            }

            public float2 Constrain(float2 size, float4 constraints) {
                return new float2(math.clamp(size.x, constraints.x, constraints.y), math.clamp(size.y, constraints.z, constraints.w));
            }
            public BoxModel GetBoxModel<TProperties>(Entity target, float2 space, TProperties properties) where TProperties : struct, IValueProperties {
                UIElementDetails layoutVersion = elementDetailsData.HasComponent(target) ? elementDetailsData[target] : default;
                return BoxModel.CreateFromSpace(space,
                layoutVersion.Margin.RealValues(properties),
                layoutVersion.BorderWidth.RealValues(properties),
                layoutVersion.Padding.RealValues(properties)
                );
            }
            public void GetLayoutDetails(Entity target, float span, out Spacing spacing, out Alignment alignChildren, out bool wrap) {
                if (elementDetailsData.HasComponent(target)) {
                    var details = elementDetailsData[target];
                    spacing = details.Spacing;
                    alignChildren = details.AlignChildren;
                    wrap = details.Wrap && span.IsDefinite();
                }
                else {
                    spacing = Spacing.Start;
                    alignChildren = Alignment.Start;
                    wrap = false;
                }
            }
            //Layouts Because I couldn't think of a better way
            public float2 Layout(Entity container, float4 containerConstraints, DynamicBuffer<UIChild> items, Layout layoutType, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                switch (layoutType) {
                    case UI.Layout.Horizontal:
                        return LayoutHorizontal(container, containerConstraints, items, boxes, positions);
                    case UI.Layout.Vertical:
                        return LayoutVertical(container, containerConstraints, items, boxes, positions);
                    default:
                        return boxes[container].content;
                }
            }
            public float2 LayoutHorizontal(Entity container, float4 containerConstraints, DynamicBuffer<UIChild> items, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                float2 totalSize = new float2(0, 0);
                var lineOffset = 0f;
                var containerBox = boxes[container];
                var containerSize = containerBox.Size;
                var lineItems = new NativeList<LineItem>(8, Allocator.Temp);
                var crossHeight = 0f;
                GetLayoutDetails(container, containerSize.x, out Spacing spacing, out Alignment align, out bool wrap);
                var usedSpace = 0f;
                for (int i = 0; i < items.Length; i++) {
                    var s = boxes[items[i]].Size;
                    var w = s.x;
                    var h = s.y;
                    lineItems.Add(new LineItem
                    {
                        length = w,
                        cross = h,
                        item = items[i]
                    });
                    usedSpace += w;
                    if (h.IsDefinite()) {
                        crossHeight = math.max(crossHeight, h);
                    }
                    else if (containerSize.y.IsDefinite()) {
                        crossHeight = math.max(crossHeight, containerSize.y);
                    }
                    if (i + 1 >= items.Length || (wrap && boxes[items[i + 1]].Size.x + usedSpace > containerSize.x)) {
                        var space = spacing.GetSpacing<LineItem, NativeList<LineItem>>(containerSize.x, lineItems);

                        var spacingOffset = space[0];
                        for (int j = 0; j < lineItems.Length; j++) {
                            var a = elementDetailsData.HasComponent(lineItems[j].item) ? elementDetailsData[lineItems[j].item].AlignSelf : align;

                            positions[lineItems[j].item] = new float2(spacingOffset, a.GetOffset(lineItems[j].cross, lineOffset + crossHeight));
                            spacingOffset += boxes[lineItems[j].item].Width + space[j + 1];
                        }
                        totalSize.x = math.max(totalSize.x, usedSpace);
                        totalSize.y += crossHeight;
                        lineOffset += crossHeight;
                        crossHeight = 0f;
                        usedSpace = 0f;
                        lineItems.Clear();
                        space.Dispose();
                    }
                }
                lineItems.Dispose();
                return Constrain(totalSize, containerConstraints);
            }

            public float2 LayoutVertical(Entity container, float4 containerConstraints, DynamicBuffer<UIChild> items, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                float2 totalSize = new float2(0, 0);
                var lineOffset = 0f;
                var containerBox = boxes[container];
                var containerSize = containerBox.Size;
                var lineItems = new NativeList<LineItem>(8, Allocator.Temp);
                var crossSize = 0f;
                GetLayoutDetails(container, containerSize.y, out Spacing spacing, out Alignment align, out bool wrap);
                var usedSpace = 0f;
                for (int i = 0; i < items.Length; i++) {
                    var s = boxes[items[i]].Size;
                    var w = s.x;
                    var h = s.y;
                    lineItems.Add(new LineItem
                    {
                        length = h,
                        cross = w,
                        item = items[i]
                    });
                    usedSpace += h;
                    if (h.IsDefinite()) {
                        crossSize = math.max(crossSize, w);
                    }
                    else if (containerSize.y.IsDefinite()) {
                        crossSize = math.max(crossSize, containerSize.x);
                    }
                    if (i + 1 >= items.Length || (wrap && boxes[items[i + 1]].Size.y + usedSpace > containerSize.y)) {
                        var space = spacing.GetSpacing<LineItem, NativeList<LineItem>>(containerSize.y, lineItems);

                        var spacingOffset = space[0];
                        for (int j = 0; j < lineItems.Length; j++) {
                            var a = elementDetailsData.HasComponent(lineItems[j].item) ? elementDetailsData[lineItems[j].item].AlignSelf : align;

                            positions[lineItems[j].item] = new float2(a.GetOffset(lineItems[j].cross, lineOffset + crossSize), spacingOffset);
                            spacingOffset += boxes[lineItems[j].item].Height + space[j + 1];
                        }
                        totalSize.y = math.max(totalSize.y, usedSpace);
                        totalSize.x += crossSize;
                        lineOffset += crossSize;
                        crossSize = 0f;
                        usedSpace = 0f;
                        lineItems.Clear();
                        space.Dispose();
                    }
                }
                lineItems.Dispose();
                return Constrain(totalSize, containerConstraints);
            }

        }

        public struct ValueProperties : IValueProperties {

            public float2 parentInnerBox;
            public float FontSize { get; set; }

            public float RootFontSize { get; set; }

            public float CalculatePercentage(float percent, UILength.Hints hints) {
                return ((hints & UILength.Hints.Vertical) != 0 ? parentInnerBox.y : parentInnerBox.x) * percent;
            }
        }
        public struct BoxModel {
            public float4 margin, borderWidth, padding;
            public float2 content;
            public static BoxModel CreateFromSpace(float2 space, float4 margin, float4 borderWidth, float4 padding) {
                return new BoxModel
                {
                    margin = margin,
                    borderWidth = borderWidth,
                    padding = padding,
                    content = new float2(space.x - (margin.y + margin.w) - (borderWidth.y + borderWidth.w) - (padding.y + padding.w),
                    space.y - (margin.x + margin.z) - (borderWidth.x + borderWidth.z) - (padding.x + padding.z)
                    )

                };
            }
            public float4 Edges
            {
                get => margin + borderWidth + padding;
            }
            public float2 Size
            {
                get => new float2(Width, Height);
            }
            public float Width => content.x + (margin.y + margin.w) + (padding.y + padding.w) + (borderWidth.y + borderWidth.w);
            public float Height => content.y + (margin.x + margin.z) + (padding.x + padding.z) + (borderWidth.x + borderWidth.z);

        }

        public struct LineItem : ISpan {
            public float length;
            public float cross;
            public Entity item;
            public float Length => length;
        }
    }
}