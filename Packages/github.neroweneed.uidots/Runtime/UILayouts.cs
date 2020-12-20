using System;
using System.Runtime.CompilerServices;
using System.Text;
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
using UnityEngine.AddressableAssets;

[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots {
    public unsafe static class UIRenderBoxWriters {
        [BurstCompile]
        public static void WriteRenderBox(UIPassState* state, BackgroundConfig* backgroundConfig, BorderConfig* borderConfig, IntPtr vertexPtr, UIContext* context) {
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 0, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 1, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 2, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 3, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomRight.Normalize(*context)),
            });
        }
        [BurstCompile]
        public static void WriteCharRenderBoxes(UIPassState* state, IntPtr config, BackgroundConfig* backgroundConfig, BorderConfig* borderConfig, FontConfig* fontConfig, TextConfig* textConfig, IntPtr vertexPtr, UIContext* context) {
            var height = fontConfig->size.Normalize(*context);
            var fontSizeScale = (height / textConfig->fontInfo.lineHeight) * textConfig->fontInfo.scale;
            float2 initialOffset = new float2(state->globalBox.x, state->globalBox.y - (textConfig->fontInfo.descentLine * fontSizeScale));
            //()
            float offsetX = initialOffset.x;
            float offsetY = initialOffset.y;
            var nullBg = new float3(float.NaN, float.NaN, float.NaN);

            float width = 0f;
            for (int i = 1; i < textConfig->text.length + 1; i++) {
                var charInfo = textConfig->GetCharInfo(config, i - 1);
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), i * 4, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y, charInfo.index),
                    color = fontConfig->color

                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 1, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y, charInfo.index),
                    color = fontConfig->color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 2, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = fontConfig->color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 3, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = fontConfig->color
                });
                offsetX += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                if (i + 1 < textConfig->text.length + 1) {
                    width += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                }
                else {
                    width += ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale);
                }
            }
            //Write BG Box based on box defined by chars
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 0, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(initialOffset.x, initialOffset.y + (textConfig->fontInfo.descentLine * fontSizeScale), 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 1, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(initialOffset.x + width, initialOffset.y + (textConfig->fontInfo.descentLine * fontSizeScale), 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 2, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(initialOffset.x, initialOffset.y + height + (textConfig->fontInfo.descentLine * fontSizeScale) + state->globalBox.w, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 3, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(initialOffset.x + width, initialOffset.y + height + (textConfig->fontInfo.descentLine * fontSizeScale) + state->globalBox.w, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomRight.Normalize(*context)),
            });
        }
    }
    [BurstCompile]
    public unsafe static class UILayouts {
        [BurstCompile]
        public static int TextElementRenderBoxHandler(IntPtr configPtr, int configOffset, int configLength, ulong configurationMask) {
            TextConfig* config = (TextConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.TextConfig, configPtr + configOffset).ToPointer();
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
        [UIDotsElement("Button", UIConfigLayout.NameConfig, UIConfigLayout.BackgroundConfig, UIConfigLayout.BorderConfig, UIConfigLayout.FontConfig, UIConfigLayout.BoxConfig, UIConfigLayout.TextConfig, UIConfigLayout.SizeConfig,UIConfigLayout.SelectableConfig)]
        [UIDotsRenderBoxHandler(nameof(TextElementRenderBoxHandler))]
        public static void ButtonElement(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
        ulong configurationMask,
        IntPtr statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            IntPtr configSource = configPtr + configOffset;
            TextConfig* textConfig = (TextConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.TextConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BackgroundConfig, configSource).ToPointer();
            FontConfig* fontConfig = (FontConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.FontConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BorderConfig, configSource).ToPointer();
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
        [UIDotsElement("TextElement", UIConfigLayout.NameConfig, UIConfigLayout.BackgroundConfig, UIConfigLayout.BorderConfig, UIConfigLayout.FontConfig, UIConfigLayout.BoxConfig, UIConfigLayout.TextConfig, UIConfigLayout.SizeConfig)]
        [UIDotsRenderBoxHandler(nameof(TextElementRenderBoxHandler))]
        public static void TextElement(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
        ulong configurationMask,
        IntPtr statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            IntPtr configSource = configPtr + configOffset;
            TextConfig* textConfig = (TextConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.TextConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BackgroundConfig, configSource).ToPointer();
            FontConfig* fontConfig = (FontConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.FontConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BorderConfig, configSource).ToPointer();
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
        [UIDotsElement("HBox", UIConfigLayout.NameConfig, UIConfigLayout.BackgroundConfig, UIConfigLayout.BorderConfig, UIConfigLayout.SequentialLayoutConfig, UIConfigLayout.BoxConfig, UIConfigLayout.SizeConfig)]
        public static void HBox(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
        ulong configurationMask,
        IntPtr statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
            ) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            IntPtr configSource = configPtr + configOffset;
            BoxConfig* boxConfig = (BoxConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BoxConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.SizeConfig, configSource).ToPointer();
            SequentialLayoutConfig* sequentialLayoutConfig = (SequentialLayoutConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.SequentialLayoutConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            switch ((UIPassType)type) {
                case UIPassType.Constrain:
                    /*                                         for (int i = 0; i < stateChildCount; i++) {
                                                                var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i)))).ToPointer();
                                                                childState->localBox.y += config->padding.Y.Normalize(*context);
                                                                childState->localBox.w += config->padding.W.Normalize(*context);
                                                            } */
                    break;
                case UIPassType.Size:
                    float totalWidth = boxConfig->padding.X.Normalize(*context);
                    var yPadding = boxConfig->padding.Y.Normalize(*context);
                    var wPadding = boxConfig->padding.W.Normalize(*context);
                    //float totalWidth = 0f;
                    float maxHeight = 0f;
                    for (int i = 0; i < stateChildCount; i++) {
                        var childState = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i)))).ToPointer();

                        childState->localBox.x += totalWidth;

                        totalWidth += childState->size.x;
                        maxHeight = math.max(maxHeight, childState->size.y + childState->localBox.y + childState->localBox.w);
                        childState->localBox.y += yPadding;
                        if (i + 1 < stateChildCount)
                            totalWidth += sequentialLayoutConfig->spacing.Normalize(*context);
                    }

                    var size = new float2(
math.clamp(totalWidth - boxConfig->padding.X.Normalize(*context), sizeConfig->minWidth.Normalize(*context), sizeConfig->maxWidth.Normalize(*context)),
math.clamp(maxHeight, sizeConfig->minHeight.Normalize(*context), sizeConfig->maxHeight.Normalize(*context)) + boxConfig->padding.Y.Normalize(*context)
);
                    UIJobUtility.AdjustPosition(size, sequentialLayoutConfig, selfPtr, statePtr, stateChildCount, stateChildren);
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
        [UIDotsElement("VBox", UIConfigLayout.NameConfig, UIConfigLayout.BackgroundConfig, UIConfigLayout.BorderConfig, UIConfigLayout.SequentialLayoutConfig, UIConfigLayout.BoxConfig, UIConfigLayout.SizeConfig)]
        public static void VBox(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
        ulong configurationMask,
        IntPtr statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            IntPtr configSource = configPtr + configOffset;
            BoxConfig* boxConfig = (BoxConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BoxConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BackgroundConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.BorderConfig, configSource).ToPointer();
            SizeConfig* sizeConfig = (SizeConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.SizeConfig, configSource).ToPointer();
            SequentialLayoutConfig* sequentialLayoutConfig = (SequentialLayoutConfig*)UIConfigLayout.GetConfig(configurationMask, UIConfigLayout.SequentialLayoutConfig, configSource).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            switch ((UIPassType)type) {
                case UIPassType.Constrain:
                    /*                     for (int i = 0; i < stateChildCount; i++) {
                                            var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i)))).ToPointer();
                                            if (i == 0)
                                                childState->localBox.y += config->padding.Y.Normalize(*context);
                                            if (i + 1 >= stateChildCount)
                                                childState->localBox.w += config->padding.W.Normalize(*context);
                                            childState->localBox.x += config->padding.X.Normalize(*context);
                                            childState->localBox.z += config->padding.Z.Normalize(*context);
                                        } */


                    break;
                case UIPassType.Size:
                    float totalHeight = boxConfig->padding.Y.Normalize(*context);

                    float maxWidth = 0f;
                    var xPadding = boxConfig->padding.X.Normalize(*context);
                    var zPadding = boxConfig->padding.Z.Normalize(*context);
                    for (int i = 0; i < stateChildCount; i++) {
                        var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i)))).ToPointer();
                        childState->localBox.y += totalHeight;
                        totalHeight += childState->size.y;
                        maxWidth = math.max(maxWidth, childState->size.x + childState->localBox.x + childState->localBox.z);
                        childState->localBox.x += xPadding;
                        if (i + 1 < stateChildCount)
                            totalHeight += sequentialLayoutConfig->spacing.Normalize(*context);
                    }
                    var size = new float2(
                                    math.clamp(maxWidth, sizeConfig->minWidth.Normalize(*context), sizeConfig->maxWidth.Normalize(*context)),
                                    math.clamp(totalHeight - boxConfig->padding.Y.Normalize(*context), sizeConfig->minHeight.Normalize(*context), sizeConfig->maxHeight.Normalize(*context))
                                );
                    UIJobUtility.AdjustPosition(size, sequentialLayoutConfig, selfPtr, statePtr, stateChildCount, stateChildren);
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
}