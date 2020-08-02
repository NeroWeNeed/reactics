using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UILayoutSystemGroup))]

    public class SimpleLayoutSystem : SystemBase {
        private EntityQuery query;
        private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate() {
            query = GetEntityQuery(ComponentType.ReadWrite<UIElement>(), ComponentType.Exclude<UIParent>());
            query.SetChangedVersionFilter(typeof(UIElement));
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(query);
        }
        protected override void OnUpdate() {

            var entityTypeHandle = GetEntityTypeHandle();
            var childData = GetBufferFromEntity<UIChild>(true);
            var resolvedBox = GetComponentDataFromEntity<UIResolvedBox>(true);
            var sizeData = GetComponentDataFromEntity<UISize>(true);
            var layoutData = GetComponentDataFromEntity<UILayout>(true);
            var paddingData = GetComponentDataFromEntity<UIPadding>(true);
            var marginData = GetComponentDataFromEntity<UIMargin>(true);
            var borderWidthData = GetComponentDataFromEntity<UIBorderWidth>(true);
            var spacingData = GetComponentDataFromEntity<UISpacing>(true);
            var wrapData = GetComponentDataFromEntity<UIWrap>(true);
            var alignChildrenData = GetComponentDataFromEntity<UIAlignChildren>(true);
            var alignSelfData = GetComponentDataFromEntity<UIAlignSelf>(true);
            var elementTypeHandle = GetComponentTypeHandle<UIElement>(true);
            var entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var job = new LayoutJob
            {
                entityTypeHandle = entityTypeHandle,
                childData = childData,
                resolvedBox = resolvedBox,
                sizeData = sizeData,
                layoutData = layoutData,
                paddingData = paddingData,
                marginData = marginData,
                borderWidthData = borderWidthData,
                spacingData = spacingData,
                wrapData = wrapData,
                alignChildrenData = alignChildrenData,
                alignSelfData = alignSelfData,
                elementTypeHandle = elementTypeHandle,
                entityCommandBuffer = entityCommandBuffer
            }.Schedule(query, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer(job);
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
            public ComponentDataFromEntity<UIPadding> paddingData;
            [ReadOnly]
            public ComponentDataFromEntity<UIMargin> marginData;
            [ReadOnly]
            public ComponentDataFromEntity<UIBorderWidth> borderWidthData;
            [ReadOnly]
            public ComponentDataFromEntity<UISpacing> spacingData;
            [ReadOnly]
            public ComponentDataFromEntity<UIWrap> wrapData;
            [ReadOnly]
            public ComponentDataFromEntity<UIAlignChildren> alignChildrenData;
            [ReadOnly]
            public ComponentDataFromEntity<UIAlignSelf> alignSelfData;
            [ReadOnly]
            public ComponentTypeHandle<UIElement> elementTypeHandle;
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
                var elements = chunk.GetNativeArray(elementTypeHandle);
                for (int i = 0; i < entities.Length; i++) {
                    boxes[entities[i]] = Process(entities[i], Entity.Null, UI.Layout.Horizontal, boxes, positions);
                    positions[entities[i]] = float2.zero;
                    ResolveBox(chunkIndex, entities[i], boxes, positions);

                    entityCommandBuffer.SetComponent(chunkIndex, entities[i], new UIElement { Version = elements[i].Version + 1 });

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
                return BoxModel.CreateFromSpace(space,
                marginData.HasComponent(target) ? marginData[target].GetRealValues(properties) : float4.zero,
                borderWidthData.HasComponent(target) ? borderWidthData[target].GetRealValues(properties) : float4.zero,
                paddingData.HasComponent(target) ? paddingData[target].GetRealValues(properties) : float4.zero
                );
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
                var lineHeight = 0f;
                var spacing = spacingData.HasComponent(container) ? spacingData[container].value : Spacing.Start;
                var align = alignChildrenData.HasComponent(container) ? alignChildrenData[container].value : Alignment.Start;
                bool doWrap = wrapData.HasComponent(container) && containerSize.x.IsDefinite();
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
                        lineHeight = math.max(lineHeight, h);
                    }
                    else if (containerSize.y.IsDefinite()) {
                        lineHeight = math.max(lineHeight, containerSize.y);
                    }
                    if (i + 1 >= items.Length || (doWrap && boxes[items[i + 1]].Size.x + usedSpace > containerSize.x)) {
                        var space = spacing.GetSpacing<LineItem, NativeList<LineItem>>(containerSize.x, lineItems);

                        var spacingOffset = space[0];
                        for (int j = 0; j < lineItems.Length; j++) {
                            var a = alignSelfData.HasComponent(lineItems[j].item) ? alignSelfData[lineItems[j].item].value : align;

                            positions[lineItems[j].item] = new float2(spacingOffset, a.GetOffset(lineItems[j].cross, lineOffset + lineHeight));
                            spacingOffset += boxes[lineItems[j].item].Width + space[j + 1];
                        }
                        totalSize.x = math.max(totalSize.x, usedSpace);
                        totalSize.y += lineHeight;
                        lineOffset += lineHeight;
                        lineHeight = 0f;
                        usedSpace = 0f;
                        lineItems.Clear();
                        space.Dispose();
                    }
                }
                lineItems.Dispose();
                return Constrain(totalSize, containerConstraints);
            }

            public float2 LayoutVertical(Entity container, float4 containerConstraints, DynamicBuffer<UIChild> items, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                float offset = 0;
                float width = 0;
                /*                foreach (var item in items) {
                                   positions[item] = new float2(0, offset);
                                   offset += sizes[item].y;
                                   width = math.max(width, sizes[item].x);
                               } */
                return new float2(width, offset);
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