using System;
using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Layouts {
    [BurstCompile]
    [UIDotsElement("HBox", UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.BoxLayoutConfig, UIConfigLayoutTable.BoxModelConfig, UIConfigLayoutTable.SizeConfig)]
    public unsafe static class HBoxElement {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UIRenderPass))]
        public static void Render(IntPtr configPtr, NodeInfo* nodeInfo, UIPassState* statePtr, UIVertexData* vertexDataPtr, UIContext* context) {
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
        public static void Layout(int childIndex, IntPtr configPtr, NodeInfo* nodeInfo, IntPtr statePtr, UIContext* context) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            BoxModelConfig* boxConfig = (BoxModelConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxModelConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();
            BoxLayoutConfig* sequentialLayoutConfig = (BoxLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxLayoutConfig, configSource).ToPointer();
            HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
            if (childIndex < 0) {
                float totalWidth = boxConfig->padding.X.Normalize(*context);
                var yPadding = boxConfig->padding.Y.Normalize(*context);
                var wPadding = boxConfig->padding.W.Normalize(*context);
                float maxHeight = 0f;
                for (int i = 0; i < headerConfig->childCount; i++) {
                    var childState = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), i)))).ToPointer();
                    childState->localBox.x += totalWidth;
                    totalWidth += childState->size.x;
                    maxHeight = math.max(maxHeight, childState->size.y + childState->localBox.y + childState->localBox.w);
                    childState->localBox.y += yPadding;
                    if (i + 1 < headerConfig->childCount)
                        totalWidth += sequentialLayoutConfig->spacing.Normalize(*context);
                }
                var size = new float2(
                                math.clamp(totalWidth - boxConfig->padding.X.Normalize(*context), sizeConfig->minWidth.Normalize(*context), sizeConfig->maxWidth.Normalize(*context)),
                                math.clamp(maxHeight, sizeConfig->minHeight.Normalize(*context), sizeConfig->maxHeight.Normalize(*context)) + boxConfig->padding.Y.Normalize(*context)
                                );
                UIJobUtility.AdjustPosition(size, sequentialLayoutConfig, selfPtr, statePtr, headerConfig->childCount, (configPtr + nodeInfo->childrenOffset).ToPointer());
                selfPtr->size += new float2(boxConfig->padding.X.Normalize(*context) + boxConfig->padding.Z.Normalize(*context), boxConfig->padding.Y.Normalize(*context) + boxConfig->padding.W.Normalize(*context));
            }
        }
    }
    [BurstCompile]
    [UIDotsElement("VBox", UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.BoxLayoutConfig, UIConfigLayoutTable.BoxModelConfig, UIConfigLayoutTable.SizeConfig)]
    public unsafe static class VBoxElement {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UIRenderPass))]
        public static void Render(IntPtr configPtr, NodeInfo* nodeInfo, UIPassState* statePtr, UIVertexData* vertexDataPtr, UIContext* context) {
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
        public static void Layout(int childIndex, IntPtr configPtr, NodeInfo* nodeInfo, IntPtr statePtr, UIContext* context) {
            if (childIndex < 0) {
                UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
                IntPtr configSource = configPtr + nodeInfo->configOffset;
                BoxModelConfig* boxConfig = (BoxModelConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxModelConfig, configSource).ToPointer();
                BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
                BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
                SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();
                BoxLayoutConfig* sequentialLayoutConfig = (BoxLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxLayoutConfig, configSource).ToPointer();
                HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
                float totalHeight = boxConfig->padding.Y.Normalize(*context);
                float maxWidth = 0f;
                var xPadding = boxConfig->padding.X.Normalize(*context);
                var zPadding = boxConfig->padding.Z.Normalize(*context);
                for (int i = 0; i < headerConfig->childCount; i++) {
                    var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * UnsafeUtility.ReadArrayElement<int>((configPtr + nodeInfo->childrenOffset).ToPointer(), headerConfig->childCount - 1 - i))).ToPointer();
                    childState->localBox.y += totalHeight;
                    totalHeight += childState->size.y;
                    maxWidth = math.max(maxWidth, childState->size.x + childState->localBox.x + childState->localBox.z);
                    childState->localBox.x += xPadding;
                    if (i + 1 < headerConfig->childCount)
                        totalHeight += sequentialLayoutConfig->spacing.Normalize(*context);
                }
                var size = new float2(
                                math.clamp(maxWidth, sizeConfig->minWidth.Normalize(*context), sizeConfig->maxWidth.Normalize(*context)),
                                math.clamp(totalHeight - boxConfig->padding.Y.Normalize(*context), sizeConfig->minHeight.Normalize(*context), sizeConfig->maxHeight.Normalize(*context))
                            );
                UIJobUtility.AdjustPosition(size, sequentialLayoutConfig, selfPtr, statePtr, headerConfig->childCount, (configPtr + nodeInfo->childrenOffset).ToPointer());
                selfPtr->size += new float2(boxConfig->padding.X.Normalize(*context) + boxConfig->padding.Z.Normalize(*context), boxConfig->padding.Y.Normalize(*context) + boxConfig->padding.W.Normalize(*context));
            }
        }

    }

}