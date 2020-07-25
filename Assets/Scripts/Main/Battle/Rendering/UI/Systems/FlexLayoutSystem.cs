using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {

    public class FlexLayoutSystem : SystemBase {
        protected override void OnCreate() {

        }
        protected override void OnUpdate() {
            //throw new System.NotImplementedException();
        }
        public struct LayoutJob : IJobChunk {
            public EntityTypeHandle entityHandle;
            public ComponentDataFromEntity<UIParent> parentData;
            public BufferFromEntity<UIChild> childData;
            public ComponentDataFromEntity<Flex> flexData;
            public ComponentDataFromEntity<UIBorderWidth> borderWidthData;
            public ComponentDataFromEntity<UIMargin> marginData;
            public ComponentDataFromEntity<UIPadding> paddingData;
            public ComponentDataFromEntity<UIWidth> widthData;
            public ComponentDataFromEntity<UIHeight> heightData;
            public ComponentDataFromEntity<UIOrder> orderData;
            public ComponentDataFromEntity<AspectRatio> aspectRatioData;


            private NativeHashMap<Entity, float2> availableSpace;
            private NativeHashMap<Entity, float> hypotheticalMainSize;
            private NativeHashMap<Entity, Flex.Basis> flexBases;
            private NativeHashMap<Entity, float4> constraints;
            private NativeHashMap<Entity, Flex> flex;
            private FlexValueProperties valueProperties;
            private NativeMultiHashMap<Entity, FlexItem> flexContainers;
            private NativeHashMap<Entity, float> finalMainSizes;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(entityHandle);
                availableSpace = new NativeHashMap<Entity, float2>(8, Allocator.Temp)
                {
                    [Entity.Null] = new float2(float.PositiveInfinity, float.PositiveInfinity)
                };
                constraints = new NativeHashMap<Entity, float4>(8, Allocator.Temp)
                {
                    [Entity.Null] = new float4(0, 0, float.PositiveInfinity, float.PositiveInfinity)
                };
                flex = new NativeHashMap<Entity, Flex>(8, Allocator.Temp)
                {
                    [Entity.Null] = default
                };
                flexBases = new NativeHashMap<Entity, Flex.Basis>(8, Allocator.Temp)
                {
                    [Entity.Null] = UILength.Auto
                };
                hypotheticalMainSize = new NativeHashMap<Entity, float>(8, Allocator.Temp)
                {
                    [Entity.Null] = float.PositiveInfinity
                };
                flexContainers = new NativeMultiHashMap<Entity, FlexItem>(8, Allocator.Temp);
                finalMainSizes = new NativeHashMap<Entity, float>(8, Allocator.Temp);
                for (int i = 0; i < entities.Length; i++) {
                    ExecuteOnContainer(entities[i], Entity.Null);
                }

            }
            private void ExecuteOnContainer(Entity container, Entity parent) {
                valueProperties = new FlexValueProperties
                {
                    ContainerLength = availableSpace[parent].x,
                    FontSize = 12
                };
                if (flexData.HasComponent(container)) {
                    flex[container] = flexData[container];
                }
                else {
                    flex[container] = Flex.Initial;
                }
                var constraints = this.constraints[parent];
                if (widthData.HasComponent(container)) {
                    var width = widthData[container];
                    if (!width.IsFixed()) {

                        constraints.x = math.clamp(width.Min.RealValue(valueProperties), this.constraints[parent].x, this.constraints[parent].z);
                        constraints.z = math.clamp(width.Max.RealValue(valueProperties), this.constraints[parent].x, this.constraints[parent].z);
                    }
                    else {
                        constraints.x = width.Max.RealValue(valueProperties);
                        constraints.z = width.Max.RealValue(valueProperties);
                    }
                }
                if (heightData.HasComponent(container)) {
                    var height = heightData[container];
                    if (!height.IsFixed()) {
                        constraints.y = math.clamp(height.Min.RealValue(valueProperties), this.constraints[parent].y, this.constraints[parent].w);
                        constraints.w = math.clamp(height.Max.RealValue(valueProperties), this.constraints[parent].y, this.constraints[parent].w);
                    }
                    else {
                        constraints.y = height.Max.RealValue(valueProperties);
                        constraints.w = height.Max.RealValue(valueProperties);
                    }
                }
                this.constraints[container] = constraints;
                availableSpace[container] = flex[container].direction.IsRow() ? new float2(constraints.z, constraints.w) : new float2(constraints.w, constraints.z);
                flexBases[container] = flex[container].basis;
                ResolveHypotheticalMainSizes(container, parent, valueProperties);
                if (childData.HasComponent(container)) {
                    var children = childData[container];
                    foreach (var entity in children) {
                        ExecuteOnContainer(entity, container);
                    }
                    CreateFlexLines(container, children);
                }
                else {

                }


            }
            private void ResolveHypotheticalMainSizes<TProperties>(Entity container, Entity parent, TProperties props) where TProperties : struct, IValueProperties {
                var basis = flexData[container].basis;

                if (basis.TryGetDefiniteValue(props, out float value)) {
                    hypotheticalMainSize[container] = value;
                }
                else if (aspectRatioData.HasComponent(container) && availableSpace[container].y.IsDefinite()) {
                    hypotheticalMainSize[container] = aspectRatioData[container].value * availableSpace[container].y;
                }
                else {

                    hypotheticalMainSize[container] = value;
                }

            }
            private float2 CreateFlexLines(Entity container, DynamicBuffer<UIChild> children) {
                var mainSize = 0f;
                var lineCount = 1;
                if (flexData[container].wrap == Flex.Wrap.NoWrap) {
                    foreach (var child in children) {
                        flexContainers.Add(container, new FlexItem
                        {
                            order = orderData.HasComponent(child) ? orderData[child].value : 0,
                            item = child,

                        });
                        mainSize += hypotheticalMainSize[child];
                    }
                }
                else {
                    var tempMainSize = 0f;
                    var lineLength = hypotheticalMainSize[container];
                    for (int i = 0; i < children.Length; i++) {
                        flexContainers.Add(container, new FlexItem
                        {
                            order = orderData.HasComponent(children[i]) ? orderData[children[i]].value : 0,
                            item = children[i],
                            line = lineCount - 1
                        });
                        tempMainSize += hypotheticalMainSize[children[i]];
                        if (i + 1 < children.Length && (tempMainSize + hypotheticalMainSize[children[i]]) > lineLength) {
                            lineCount++;
                            mainSize = math.max(lineLength, math.max(mainSize, tempMainSize));
                            tempMainSize = 0f;
                        }
                    }
                }
                return new float2(mainSize, lineCount);
            }
            /*             private void ResolveFlexLengths(Entity container, DynamicBuffer<UIChild> children) {
                            var size = CreateFlexLines(container, children);

                            bool shouldGrow = size.x < hypotheticalMainSize[container];
                            var frozen = new NativeList<Entity>(8, Allocator.Temp);
                            for (int i = 0; i < children.Length; i++) {
                                var data = flexData[children[i]];
                                if ((data.grow == 0 && shouldGrow) 
                                || (data.shrink == 0 && !shouldGrow)
                                || (shouldGrow && flexBases[children[i]].)

                                ) {
                                    frozen.Add(children[i]);
                                }
                            }
                        } */


        }
    }

    public struct FlexItem {
        public int order;
        public int line;
        public Entity item;


    }
    public struct FlexValueProperties : IValueProperties {
        public float ContainerLength { get; set; }
        public float FontSize { get; set; }
    }

    /*     public class FlexLayoutSystem : SystemBase {

            private EntityQuery rootQuery;
            protected override void OnCreate() {
                rootQuery = GetEntityQuery(ComponentType.Exclude<UIParent>(), ComponentType.ReadOnly<UIChild>(), ComponentType.ReadWrite<UIElement>(), ComponentType.ReadOnly<FlexProperties>(), ComponentType.ReadOnly<UIFixedWidth>(), ComponentType.ReadOnly<UIFixedHeight>());
            }
            protected override void OnUpdate() {
                var job = new LayoutJob
                {
                    parentData = GetComponentTypeHandle<UIParent>(true),
                    fixedWidthData = GetComponentTypeHandle<UIFixedWidth>(true),
                    fixedHeightData = GetComponentTypeHandle<UIFixedHeight>(true),
                    flexPropertyData = GetComponentTypeHandle<FlexProperties>(true),
                    childData = GetBufferTypeHandle<UIChild>(true),
                    elementData = GetComponentTypeHandle<UIElement>(true),
                    entityData = GetEntityTypeHandle(),
                    fixedWidthComponentData = GetComponentDataFromEntity<UIFixedWidth>(true),
                    fixedHeightComponentData = GetComponentDataFromEntity<UIFixedHeight>(true),
                    constrainedWidthComponentData = GetComponentDataFromEntity<UIConstrainedWidth>(true),
                    constrainedHeightComponentData = GetComponentDataFromEntity<UIConstrainedHeight>(true),
                    flexPropertyComponentData = GetComponentDataFromEntity<FlexProperties>(true),
                    parentComponentData = GetComponentDataFromEntity<UIParent>(true),
                    childComponentData = GetBufferFromEntity<UIChild>(true),
                    borderWidthComponentData = GetComponentDataFromEntity<UIBorderWidth>(true),
                    marginComponentData = GetComponentDataFromEntity<UIMargin>(true),
                    paddingComponentData = GetComponentDataFromEntity<UIPadding>(true),
                    aspectRatioComponentData = GetComponentDataFromEntity<AspectRatio>(true)
                };
            }

            public struct LayoutJob : IJobChunk {

                public ComponentTypeHandle<UIParent> parentData;
                public ComponentTypeHandle<UIFixedWidth> fixedWidthData;
                public ComponentTypeHandle<UIFixedHeight> fixedHeightData;
                public ComponentTypeHandle<FlexProperties> flexPropertyData;
                public BufferTypeHandle<UIChild> childData;
                public ComponentTypeHandle<UIElement> elementData;
                public EntityTypeHandle entityData;


                public ComponentDataFromEntity<UIFixedWidth> fixedWidthComponentData;
                public ComponentDataFromEntity<UIFixedHeight> fixedHeightComponentData;
                public ComponentDataFromEntity<UIConstrainedWidth> constrainedWidthComponentData;
                public ComponentDataFromEntity<UIConstrainedHeight> constrainedHeightComponentData;
                public ComponentDataFromEntity<FlexProperties> flexPropertyComponentData;
                public ComponentDataFromEntity<UIParent> parentComponentData;
                public BufferFromEntity<UIChild> childComponentData;

                //Properties
                public ComponentDataFromEntity<UIBorderWidth> borderWidthComponentData;
                public ComponentDataFromEntity<UIPadding> paddingComponentData;
                public ComponentDataFromEntity<UIMargin> marginComponentData;
                public ComponentDataFromEntity<AspectRatio> aspectRatioComponentData;
                public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                    NativeHashMap<Entity, float2> availableSpace = new NativeHashMap<Entity, float2>(8, Allocator.Temp);
                    NativeHashMap<Entity, FlexProperties> hierarchyFlexProperties = new NativeHashMap<Entity, FlexProperties>(8, Allocator.Temp);
                    NativeHashMap<Entity, FlexValueProperties> hierarchyFlexValueProperties = new NativeHashMap<Entity, FlexValueProperties>(8, Allocator.Temp);
                    NativeQueue<Entity> flexItemQueue = new NativeQueue<Entity>(Allocator.Temp);
                    NativeHashMap<Entity, float> flexBases = new NativeHashMap<Entity, float>(8, Allocator.Temp);
                    var entities = chunk.GetNativeArray(entityData);
                    var elements = chunk.GetNativeArray(elementData);
                    var children = chunk.GetBufferAccessor(childData);
                    var flexProperties = chunk.GetNativeArray(flexPropertyData);
                    var widths = chunk.GetNativeArray(fixedWidthData);
                    var heights = chunk.GetNativeArray(fixedHeightData);
                    var parents = chunk.GetNativeArray(parentData);
                    var flexEntities = new NativeList<Entity>(8, Allocator.Temp);
                    for (int i = 0; i < entities.Length; i++) {
                        availableSpace[entities[i]] = new float2(widths[i].width.RealValue<FlexValueProperties>(), heights[i].height.RealValue<FlexValueProperties>());
                        hierarchyFlexProperties[entities[i]] = flexProperties[i];
                        hierarchyFlexValueProperties[entities[i]] = new FlexValueProperties
                        {
                            ContainerLength = (((int)flexProperties[i].direction & 0x01) != 0) ? heights[i].height.RealValue<FlexValueProperties>() : widths[i].width.RealValue<FlexValueProperties>(),
                            FontSize = 12
                        };
                        for (int j = 0; j < children[i].Length; j++) {
                            flexItemQueue.Enqueue(children[i][j]);
                        }
                        flexEntities.Add(entities[i]);
                        GetAvailableSpace(flexItemQueue, entities[i], availableSpace, hierarchyFlexValueProperties, hierarchyFlexProperties, flexEntities);
                    }
                }

                public float2 GetInnerSize(Entity target, float2 size, FlexValueProperties properties) {
                    var padding = paddingComponentData.HasComponent(target) ? paddingComponentData[target].GetSize(properties) : float2.zero;
                    var border = borderWidthComponentData.HasComponent(target) ? borderWidthComponentData[target].GetSize(properties) : float2.zero;
                    var margin = marginComponentData.HasComponent(target) ? marginComponentData[target].GetSize(properties) : float2.zero;
                    return size - (padding + border + margin);
                }
                public float GetInnerWidth(Entity target, float width, FlexValueProperties properties) {
                    var padding = paddingComponentData.HasComponent(target) ? paddingComponentData[target].GetWidth(properties) : 0f;
                    var border = borderWidthComponentData.HasComponent(target) ? borderWidthComponentData[target].GetWidth(properties) : 0f;
                    var margin = marginComponentData.HasComponent(target) ? marginComponentData[target].GetWidth(properties) : 0f;
                    return width - (padding + border + margin);
                }
                public float GetInnerHeight(Entity target, float height, FlexValueProperties properties) {
                    var padding = paddingComponentData.HasComponent(target) ? paddingComponentData[target].GetHeight(properties) : 0f;
                    var border = borderWidthComponentData.HasComponent(target) ? borderWidthComponentData[target].GetHeight(properties) : 0f;
                    var margin = marginComponentData.HasComponent(target) ? marginComponentData[target].GetHeight(properties) : 0f;
                    return height - (padding + border + margin);
                }
                public void GetAvailableSpace(NativeQueue<Entity> targets, Entity root, NativeHashMap<Entity, float2> availableSpace, NativeHashMap<Entity, FlexValueProperties> flexValueProperties, NativeHashMap<Entity, FlexProperties> flexProperties, NativeList<Entity> flexEntities) {
                    while (targets.Count > 0) {
                        var target = targets.Dequeue();
                        var parent = parentComponentData[target];
                        flexProperties[target] = flexPropertyComponentData.HasComponent(target) ? flexPropertyComponentData[target] : flexProperties[parent];
                        float2 space;
                        float2 size;
                        if (fixedWidthComponentData.HasComponent(parent)) {
                            size.x = fixedWidthComponentData[parent].width.RealValue(flexValueProperties[parent]);
                        }
                        else if (constrainedWidthComponentData.HasComponent(parent)) {
                            size.x = constrainedWidthComponentData[parent].Clamp(availableSpace[parent].x, flexValueProperties[parent]);
                        }
                        else {
                            size.x = GetInnerWidth(target, availableSpace[parent].x, flexValueProperties[parent]);
                        }
                        if (fixedHeightComponentData.HasComponent(parent)) {
                            size.y = fixedHeightComponentData[parent].height.RealValue(flexValueProperties[parent]);
                        }
                        else if (constrainedHeightComponentData.HasComponent(parent)) {
                            size.y = constrainedHeightComponentData[parent].Clamp(availableSpace[parent].x, flexValueProperties[parent]);
                        }
                        else {
                            size.y = GetInnerHeight(target, availableSpace[parent].y, flexValueProperties[parent]);
                        }
                        if (flexProperties[target].direction.IsColumn()) {
                            space = new float2(size.y, size.x);
                        }
                        else {
                            space = new float2(size.x, size.y);
                        }
                        availableSpace[target] = space;
                        flexValueProperties[target] = new FlexValueProperties
                        {
                            ContainerLength = space.x,
                            FontSize = 12
                        };
                        if (childComponentData.HasComponent(target)) {
                            var buffer = childComponentData[target];
                            for (int i = 0; i < buffer.Length; i++)
                                targets.Enqueue(buffer[i]);
                        }
                        flexEntities.Add(target);
                    }
                }
                public void GetFlexBaseSizes(NativeHashMap<Entity, float2> availableSpace, NativeHashMap<Entity, float> flexBases, NativeHashMap<Entity, float> mainSizes, NativeHashMap<Entity, FlexValueProperties> flexValueProperties, NativeHashMap<Entity, FlexProperties> flexProperties, NativeList<Entity> flexEntities) {
                    foreach (var flexEntity in flexEntities) {
                        var basis = flexProperties[flexEntity].basis.RealValue(flexValueProperties[flexEntity]);
                        if (basis.IsDefinite()) {
                            flexBases[flexEntity] = basis;
                            mainSizes[flexEntity] = basis;
                            continue;
                        }
                        if (aspectRatioComponentData.HasComponent(flexEntity) && flexProperties[flexEntity].basis.IsContent && availableSpace[flexEntity].y.IsDefinite()) {
                            flexBases[flexEntity] = aspectRatioComponentData[flexEntity].value * availableSpace[flexEntity].y;
                            mainSizes[flexEntity] = flexBases[flexEntity];
                            continue;
                        }


                                            else if (flexProperties[flexEntity].basis.RequiresAvailableSpace) {
                                                if (flexProperties[flexEntity].direction.IsColumn() && constrainedHeightComponentData.HasComponent(flexEntity)) {
                                                    flexBases[flexEntity] = constrainedHeightComponentData[flexEntity].Clamp(availableSpace[flexEntity].x, flexValueProperties[flexEntity]);
                                                    continue;
                                                }
                                                else if (flexProperties[flexEntity].direction.IsRow() && constrainedWidthComponentData.HasComponent(flexEntity)) {
                                                    flexBases[flexEntity] = constrainedWidthComponentData[flexEntity].Clamp(availableSpace[flexEntity].x, flexValueProperties[flexEntity]);
                                                    continue;
                                                }

                                            }

                    }
                }
                public void CalculateMainSizes(Entity entity, NativeQueue<Entity> flexItemQueue, NativeHashMap<Entity, float> mainSizes) {
                    float size = 0f;
                    if (!childComponentData.HasComponent(entity)) {
                        mainSizes[entity] = 0f;
                        return;
                    }
                    foreach (var child in childComponentData[entity]) {
                        flexItemQueue.Enqueue(child);
                    }
                    while (flexItemQueue.Count > 0){

                    }

                }
            }
            public struct FlexValueProperties : IValueProperties {

                public float ContainerLength { get; set; }
                public float FontSize { get; set; }
            }
        } */
}