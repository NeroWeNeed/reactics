using System;
using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Layouts {

    [BurstCompile]
    [UIDotsElement("Box",
    ConfigBlocks = UIConfigBlock.NameConfig | UIConfigBlock.BackgroundConfig | UIConfigBlock.BorderConfig | UIConfigBlock.BoxLayoutConfig | UIConfigBlock.BoxModelConfig | UIConfigBlock.SizeConfig,
    OptionalConfigBlocks = UIConfigBlock.MaterialConfig
    )]
    public unsafe static class BoxElement {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(UIRenderPass))]
        public static void Render(IntPtr configPtr, NodeInfo* nodeInfo, UIPassState* statePtr, UIVertexData* vertexDataPtr, UIContextData* context) {
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            BoxModelConfig* boxConfig = (BoxModelConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxModelConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();
            BoxLayoutConfig* sequentialLayoutConfig = (BoxLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxLayoutConfig, configSource).ToPointer();
            HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
            UIRenderBoxWriters.WriteRenderBox(statePtr, backgroundConfig, borderConfig, vertexDataPtr, context);
        }
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UILayoutPass))]
        public static void Layout(int childIndex, IntPtr configPtr, NodeInfo* nodeInfo, IntPtr statePtr, UIContextData* context) {
            if (childIndex < 0) {
                UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
                IntPtr configSource = configPtr + nodeInfo->configOffset;
                BoxModelConfig* boxConfig = (BoxModelConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxModelConfig, configSource).ToPointer();
                BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
                BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
                SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();

                BoxLayoutConfig* boxLayoutConfig = (BoxLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxLayoutConfig, configSource).ToPointer();
                HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
                float height = 0f;
                float width = 0f;
                float2 childrenSize = default;
                int4 multiplier = default;
                //float4 padding = boxConfig->padding.Normalize(*context);
                float4 constraints = new float4(
sizeConfig->minWidth.Normalize(*context), sizeConfig->maxWidth.Normalize(*context),
sizeConfig->minHeight.Normalize(*context), sizeConfig->maxHeight.Normalize(*context)
                );
                var spacing = boxLayoutConfig->spacing.Normalize(*context);
                switch (boxLayoutConfig->direction) {
                    case Direction.Left:
                        for (int i = 0; i < headerConfig->childCount; i++) {
                            var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), headerConfig->childCount - 1 - i))).ToPointer();
                            LayoutHorizontal(childState, ref width, ref height);
                            if (i + 1 < headerConfig->childCount)
                                width += spacing;
                        }
                        multiplier = new int4(1, 0, 0, 1);
                        break;
                    case Direction.Right:
                        for (int i = 0; i < headerConfig->childCount; i++) {
                            var childState = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), i)))).ToPointer();
                            LayoutHorizontal(childState, ref width, ref height);
                            if (i + 1 < headerConfig->childCount)
                                width += spacing;
                        }
                        multiplier = new int4(1, 0, 0, 1);
                        break;
                    case Direction.Up:
                        for (int i = 0; i < headerConfig->childCount; i++) {
                            var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), i))).ToPointer();
                            LayoutVertical(childState, ref height, ref width);
                            if (i + 1 < headerConfig->childCount)
                                height += spacing;
                        }
                        multiplier = new int4(0, 1, 1, 0);
                        break;
                    case Direction.Down:
                        for (int i = 0; i < headerConfig->childCount; i++) {
                            var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), headerConfig->childCount - 1 - i))).ToPointer();
                            LayoutVertical(childState, ref height, ref width);
                            if (i + 1 < headerConfig->childCount)
                                height += spacing;
                        }
                        multiplier = new int4(0, 1, 1, 0);
                        break;
                }
                childrenSize = new float2(width, height);
                selfPtr->size = new float2(
                        math.clamp(childrenSize.x, constraints.x, constraints.y),
                        math.clamp(childrenSize.y, constraints.z, constraints.w)
                    ); ;
                UIJobUtility.AdjustPosition(childrenSize, constraints, boxLayoutConfig, selfPtr, statePtr, headerConfig->childCount, (configPtr + nodeInfo->childrenOffset).ToPointer(), multiplier);

                //Debug.Log(selfPtr->size);
            }
        }

        private static void LayoutHorizontal(UIPassState* childState, ref float totalWidth, ref float maxHeight) {
            childState->localBox.x += totalWidth;
            totalWidth += childState->size.x;
            maxHeight = math.max(maxHeight, childState->size.y + childState->localBox.y + childState->localBox.w);
        }
        private static void LayoutVertical(UIPassState* childState, ref float totalHeight, ref float maxWidth) {
            childState->localBox.y += totalHeight;
            totalHeight += childState->size.y;
            maxWidth = math.max(maxWidth, childState->size.x + childState->localBox.x + childState->localBox.z);
        }

    }
}