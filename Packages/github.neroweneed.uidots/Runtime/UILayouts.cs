using System;
using System.Runtime.CompilerServices;
using System.Text;
using AOT;
using NeroWeNeed.BehaviourGraph;
using NeroWeNeed.Commons;
using TMPro;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
/* 
[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots {

    [BurstCompile]
    [UIDotsElement("Button", UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.FontConfig, UIConfigLayoutTable.BoxConfig, UIConfigLayoutTable.TextConfig, UIConfigLayoutTable.SizeConfig, UIConfigLayoutTable.SelectableConfig,RenderBoxCounter = nameof(TextElementRenderBoxHandler))]
    public unsafe static class UILayouts {
        [BurstCompile]
        public static int TextElementRenderBoxHandler(IntPtr configPtr, NodeInfo* nodeInfo) {
            TextConfig* config = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configPtr + nodeInfo->configOffset).ToPointer();
            return config->text.length + 1;
        }
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SizeText(UIPassState* selfPtr, IntPtr configSource, TextConfig* textConfig, FontConfig* fontConfig, UIContext* context) {
            var totalWidth = 0f;
            var height = fontConfig->size.Normalize(*context);
            var fontSizeScale = (height / textConfig->fontInfo.lineHeight) * textConfig->fontInfo.scale;
            for (int i = 0; i < textConfig->text.length; i++) {
                var charInfo = textConfig->GetCharInfo(configSource, i);
                if (i + 1 < textConfig->text.length) {
                    totalWidth += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                }
                else {
                    totalWidth += ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale);
                }
            }
            selfPtr->size = new float2(totalWidth + selfPtr->localBox.x + selfPtr->localBox.z, height + selfPtr->localBox.y + selfPtr->localBox.w);
        }
        [BurstCompile]
        [UIDotsElement("Button",nameof(TextElementRenderBoxHandler), UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.FontConfig, UIConfigLayoutTable.BoxConfig, UIConfigLayoutTable.TextConfig, UIConfigLayoutTable.SizeConfig,UIConfigLayoutTable.SelectableConfig)]
        [MonoPInvokeCallback(typeof(UILayoutPass))]
        public static void ButtonElement(
        byte type,
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        IntPtr statePtr,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            TextConfig* textConfig = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            FontConfig* fontConfig = (FontConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.FontConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            switch ((UIPassType)type) {
                case UIPassType.Size:
                    SizeText(selfPtr, configSource, textConfig, fontConfig, context);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteCharRenderBoxes(selfPtr, configSource, backgroundConfig, borderConfig, fontConfig, textConfig, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;

            }
        }
        [BurstCompile]
        [UIDotsElement("TextElement",nameof(TextElementRenderBoxHandler), UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.FontConfig, UIConfigLayoutTable.BoxConfig, UIConfigLayoutTable.TextConfig, UIConfigLayoutTable.SizeConfig)]
        [MonoPInvokeCallback(typeof(UILayoutPass))]
        public static void TextElement(
        byte type,
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        IntPtr statePtr,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            TextConfig* textConfig = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            FontConfig* fontConfig = (FontConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.FontConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            switch ((UIPassType)type) {
                case UIPassType.Size:
                    SizeText(selfPtr, configSource, textConfig, fontConfig, context);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteCharRenderBoxes(selfPtr, configSource, backgroundConfig, borderConfig, fontConfig, textConfig, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;
            }
        }

        [BurstCompile]
        [UIDotsElement("HBox", UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.SequentialLayoutConfig, UIConfigLayoutTable.BoxConfig, UIConfigLayoutTable.SizeConfig)]
        public static void HBox(
        byte type,
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        IntPtr statePtr,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
            ) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            BoxConfig* boxConfig = (BoxConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();
            SequentialLayoutConfig* sequentialLayoutConfig = (SequentialLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SequentialLayoutConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
            switch ((UIPassType)type) {
                case UIPassType.Constrain:
                                                            for (int i = 0; i < stateChildCount; i++) {
                                                                var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i)))).ToPointer();
                                                                childState->localBox.y += config->padding.Y.Normalize(*context);
                                                                childState->localBox.w += config->padding.W.Normalize(*context);
                                                            }
                    break;
                case UIPassType.Size:
                    float totalWidth = boxConfig->padding.X.Normalize(*context);
                    var yPadding = boxConfig->padding.Y.Normalize(*context);
                    var wPadding = boxConfig->padding.W.Normalize(*context);
                    //float totalWidth = 0f;
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
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, backgroundConfig, borderConfig, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;
            }
        }
        [BurstCompile]
        [UIDotsElement("VBox", UIConfigLayoutTable.NameConfig, UIConfigLayoutTable.BackgroundConfig, UIConfigLayoutTable.BorderConfig, UIConfigLayoutTable.SequentialLayoutConfig, UIConfigLayoutTable.BoxConfig, UIConfigLayoutTable.SizeConfig)]
        [MonoPInvokeCallback(typeof(UILayoutPass))]
        public static void VBox(
        byte type,
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        IntPtr statePtr,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            BoxConfig* boxConfig = (BoxConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BoxConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SizeConfig, configSource).ToPointer();
            SequentialLayoutConfig* sequentialLayoutConfig = (SequentialLayoutConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.SequentialLayoutConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            HeaderConfig* headerConfig = (HeaderConfig*)(configPtr + nodeInfo->nodeOffset);
            switch ((UIPassType)type) {
                case UIPassType.Constrain:
                                        for (int i = 0; i < stateChildCount; i++) {
                                            var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i)))).ToPointer();
                                            if (i == 0)
                                                childState->localBox.y += config->padding.Y.Normalize(*context);
                                            if (i + 1 >= stateChildCount)
                                                childState->localBox.w += config->padding.W.Normalize(*context);
                                            childState->localBox.x += config->padding.X.Normalize(*context);
                                            childState->localBox.z += config->padding.Z.Normalize(*context);
                                        }


                    break;
                case UIPassType.Size:
                    float totalHeight = boxConfig->padding.Y.Normalize(*context);

                    float maxWidth = 0f;
                    var xPadding = boxConfig->padding.X.Normalize(*context);
                    var zPadding = boxConfig->padding.Z.Normalize(*context);
                    for (int i = 0; i < headerConfig->childCount; i++) {
                        var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * UnsafeUtility.ReadArrayElement<int>((configPtr+nodeInfo->childrenOffset).ToPointer(), headerConfig->childCount - 1 - i))).ToPointer();
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
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, backgroundConfig, borderConfig, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;
            }
        }
    }
} */