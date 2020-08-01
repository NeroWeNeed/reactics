using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UISystemGroup))]
    public class SimpleLayoutSystem : SystemBase {
        private EntityQuery query;
        protected override void OnCreate() {
            query = GetEntityQuery(ComponentType.ReadWrite<UIElement>(), ComponentType.Exclude<UIParent>());
            query.SetChangedVersionFilter(typeof(UIElement));
            RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            new LayoutJob
            {
                entityTypeHandle = GetEntityTypeHandle(),
                childData = GetBufferFromEntity<UIChild>(true),
                resolvedBox = GetComponentDataFromEntity<UIResolvedBox>(false),
                sizeData = GetComponentDataFromEntity<UISize>(true),
                layoutData = GetComponentDataFromEntity<UILayout>(true),
                paddingData = GetComponentDataFromEntity<UIPadding>(true),
                marginData = GetComponentDataFromEntity<UIMargin>(true),
                borderWidthData = GetComponentDataFromEntity<UIBorderWidth>(true),
                spacingData = GetComponentDataFromEntity<UISpacing>(true),
                wrapData = GetComponentDataFromEntity<UIWrap>(true),
                alignChildrenData = GetComponentDataFromEntity<UIAlignChildren>(true),
                alignSelfData = GetComponentDataFromEntity<UIAlignSelf>(true)
            }.ScheduleParallel(query);
        }
        public struct LayoutJob : IJobChunk {
            public EntityTypeHandle entityTypeHandle;
            public BufferFromEntity<UIChild> childData;
            public ComponentDataFromEntity<UIResolvedBox> resolvedBox;
            public ComponentDataFromEntity<UISize> sizeData;
            public ComponentDataFromEntity<UILayout> layoutData;
            public ComponentDataFromEntity<UIPadding> paddingData;
            public ComponentDataFromEntity<UIMargin> marginData;
            public ComponentDataFromEntity<UIBorderWidth> borderWidthData;
            public ComponentDataFromEntity<UISpacing> spacingData;
            public ComponentDataFromEntity<UIWrap> wrapData;
            public ComponentDataFromEntity<UIAlignChildren> alignChildrenData;
            public ComponentDataFromEntity<UIAlignSelf> alignSelfData;
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
                foreach (var root in chunk.GetNativeArray(entityTypeHandle)) {
                    boxes[root] = Process(root, Entity.Null, UI.Layout.Horizontal, boxes, positions);
                    ResolveBox(root, boxes, positions);
                }
                boxes.Dispose();
                positions.Dispose();
            }
            public void ResolveBox(Entity entity, NativeHashMap<Entity, BoxModel> boxes, NativeHashMap<Entity, float2> positions) {
                resolvedBox[entity] = new UIResolvedBox
                {
                    value = new float4(positions[entity], positions[entity] + boxes[entity].Size)
                };
                if (childData.HasComponent(entity)) {
                    foreach (var child in childData[entity]) {
                        ResolveBox(child, boxes, positions);
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
                        var space = spacing.GetSpacing(containerSize.x, lineItems);
                        var spacingOffset = space[0];
                        for (int j = 0; j < lineItems.Length; j++) {
                            var a = alignSelfData.HasComponent(lineItems[j].item) ? alignSelfData[lineItems[j].item].value : align;
                            positions[lineItems[j].item] = new float2(spacingOffset, a.GetOffset(lineItems[j].cross, lineOffset + lineHeight));
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
                get => new float2(
                    content.x + (margin.y + margin.w) + (padding.y + padding.w) + (borderWidth.y + borderWidth.w),
                content.y + (margin.x + margin.z) + (padding.x + padding.z) + (borderWidth.x + borderWidth.z)
                );
            }

        }

        public struct LineItem : ISpan {
            public float length;
            public float cross;
            public Entity item;
            public float Length => length;
        }
    }
}