using System;
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
        public static void WriteRenderBox(UIPassState* state, UIConfig* config, IntPtr vertexPtr, UIContext* context) {
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 0, new UIVertexData
            {
                background = new float3(config->background.image.value.x, config->background.image.value.y - config->background.image.value.w, 0f),
                color = config->background.color,
                position = new float3(state->globalBox.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.top.Normalize(*context), config->border.width.left.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(*context), config->border.width.top.Normalize(*context))), config->border.radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 1, new UIVertexData
            {
                background = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y - config->background.image.value.w, 0f),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.top.Normalize(*context), config->border.width.right.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(*context), config->border.width.top.Normalize(*context))), config->border.radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 2, new UIVertexData
            {
                background = new float3(config->background.image.value.x, config->background.image.value.y, 0f),
                color = config->background.color,
                position = new float3(state->globalBox.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(*context), config->border.width.left.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(*context), config->border.width.bottom.Normalize(*context))), config->border.radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 3, new UIVertexData
            {
                background = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y, 0f),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(*context), config->border.width.right.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(*context), config->border.width.bottom.Normalize(*context))), config->border.radius.bottomRight.Normalize(*context)),
            });
        }
        [BurstCompile]
        public static void WriteCharRenderBoxes(UIConfig* config, UIPassState* state, TextConfig* textConfigPtr, IntPtr vertexPtr, UIContext* context) {
            var height = config->font.size.Normalize(*context);
            var fontSizeScale = (height / textConfigPtr->fontInfo.lineHeight) * textConfigPtr->fontInfo.scale;
            float2 initialOffset = new float2(state->globalBox.x, state->globalBox.y - (textConfigPtr->fontInfo.descentLine * fontSizeScale));
            //()
            float offsetX = initialOffset.x;
            float offsetY = initialOffset.y;
            var nullBg = new float3(float.NaN, float.NaN, float.NaN);
            float width = 0f;
            for (int i = 1; i < textConfigPtr->text.length + 1; i++) {
                var charInfo = textConfigPtr->GetCharInfo(config, i - 1);
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), i * 4, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y, charInfo.index),
                    color = config->font.color

                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 1, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y, charInfo.index),
                    color = config->font.color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 2, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = config->font.color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr.ToPointer(), (i * 4) + 3, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * fontSizeScale), 0),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = config->font.color
                });
                offsetX += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                if (i + 1 < textConfigPtr->text.length + 1) {
                    width += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                }
                else {
                    width += ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale);
                }
            }
            //Write BG Box based on box defined by chars
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 0, new UIVertexData
            {
                background = new float3(config->background.image.value.x, config->background.image.value.y - config->background.image.value.w, 0f),
                color = config->background.color,
                position = new float3(initialOffset.x, initialOffset.y + (textConfigPtr->fontInfo.descentLine * fontSizeScale), 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.top.Normalize(*context), config->border.width.left.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(*context), config->border.width.top.Normalize(*context))), config->border.radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 1, new UIVertexData
            {
                background = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y - config->background.image.value.w, 0f),
                color = config->background.color,
                position = new float3(initialOffset.x + width, initialOffset.y + (textConfigPtr->fontInfo.descentLine * fontSizeScale), 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.top.Normalize(*context), config->border.width.right.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(*context), config->border.width.top.Normalize(*context))), config->border.radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 2, new UIVertexData
            {
                background = new float3(config->background.image.value.x, config->background.image.value.y, 0f),
                color = config->background.color,
                position = new float3(initialOffset.x, initialOffset.y + height + (textConfigPtr->fontInfo.descentLine * fontSizeScale) + state->globalBox.w, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(*context), config->border.width.left.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(*context), config->border.width.bottom.Normalize(*context))), config->border.radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr.ToPointer(), 3, new UIVertexData
            {
                background = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y, 0f),
                color = config->background.color,
                position = new float3(initialOffset.x + width, initialOffset.y + height + (textConfigPtr->fontInfo.descentLine * fontSizeScale) + state->globalBox.w, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(*context), config->border.width.right.Normalize(*context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(*context), config->border.width.bottom.Normalize(*context))), config->border.radius.bottomRight.Normalize(*context)),
            });
        }
    }
    [BurstCompile]
    public unsafe static class UILayouts {
        [BurstCompile]
        public static int TextElementRenderBoxHandler(IntPtr configPtr, int configOffset, int configLength) {
            var offset = configOffset + UnsafeUtility.SizeOf<UIConfig>();
            TextConfig* config = (TextConfig*)((configPtr) + offset).ToPointer();
            return config->text.length + 1;
        }
        [BurstCompile]
        [UIDotsElement("TextElement", typeof(TextConfig))]
        [UIDotsRenderBoxHandler(nameof(TextElementRenderBoxHandler))]
        public static void TextElement(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
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
            UIConfig* config = (UIConfig*)(configPtr + configOffset).ToPointer();
            TextConfig* textConfig = (TextConfig*)(configPtr + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
            UIContext* context = (UIContext*)contextPtr.ToPointer();
            switch ((UIPassType)type) {
                case UIPassType.Size:
                    var totalWidth = 0f;
                    var height = config->font.size.Normalize(*context);
                    var fontSizeScale = (height / textConfig->fontInfo.lineHeight) * textConfig->fontInfo.scale;
                    for (int i = 0; i < textConfig->text.length; i++) {
                        var charInfo = textConfig->GetCharInfo(config, i);
                        if (i + 1 < textConfig->text.length) {
                            totalWidth += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
                        }
                        else {
                            totalWidth += ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale);
                        }
                    }
                    selfPtr->size = new float2(totalWidth + selfPtr->localBox.x + selfPtr->localBox.z, height + selfPtr->localBox.y + selfPtr->localBox.w);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteCharRenderBoxes(config, selfPtr, textConfig, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;

            }
        }


        [BurstCompile]
        [UIDotsElement("HBox", typeof(BoxConfig))]
        public static void HBox(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
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
            UIConfig* config = (UIConfig*)(configPtr + configOffset).ToPointer();

            BoxConfig* boxConfig = (BoxConfig*)(configPtr + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
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
                    float totalWidth = config->padding.X.Normalize(*context);
                    var yPadding = config->padding.Y.Normalize(*context);
                    var wPadding = config->padding.W.Normalize(*context);
                    //float totalWidth = 0f;
                    float maxHeight = 0f;
                    for (int i = 0; i < stateChildCount; i++) {
                        var childState = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i)))).ToPointer();

                        childState->localBox.x += totalWidth;

                        totalWidth += childState->size.x;
                        maxHeight = math.max(maxHeight, childState->size.y + childState->localBox.y + childState->localBox.w);
                        childState->localBox.y += yPadding;
                        if (i + 1 < stateChildCount)
                            totalWidth += boxConfig->spacing.Normalize(*context);
                    }

                    var size = new float2(
math.clamp(totalWidth - config->padding.X.Normalize(*context), config->size.minWidth.Normalize(*context), config->size.maxWidth.Normalize(*context)),
math.clamp(maxHeight, config->size.minHeight.Normalize(*context), config->size.maxHeight.Normalize(*context)) + config->padding.Y.Normalize(*context)
);
                    UIJobUtility.AdjustPosition(size, boxConfig, selfPtr, statePtr, stateChildCount, stateChildren);
                    selfPtr->size += new float2(config->padding.X.Normalize(*context) + config->padding.Z.Normalize(*context), config->padding.Y.Normalize(*context) + config->padding.W.Normalize(*context));
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, config, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;
            }
        }
        [BurstCompile]
        [UIDotsElement("VBox", typeof(BoxConfig))]
        public static void VBox(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
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
            UIConfig* config = (UIConfig*)(((IntPtr)configPtr) + configOffset).ToPointer();
            BoxConfig* boxConfig = (BoxConfig*)(((IntPtr)configPtr) + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
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
                    float totalHeight = config->padding.Y.Normalize(*context);
                    float maxWidth = 0f;
                    var xPadding = config->padding.X.Normalize(*context);
                    var zPadding = config->padding.Z.Normalize(*context);
                    for (int i = 0; i < stateChildCount; i++) {
                        var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i)))).ToPointer();
                        childState->localBox.y += totalHeight;
                        totalHeight += childState->size.y;
                        maxWidth = math.max(maxWidth, childState->size.x + childState->localBox.x + childState->localBox.z);
                        childState->localBox.x += xPadding;
                        if (i + 1 < stateChildCount)
                            totalHeight += boxConfig->spacing.Normalize(*context);
                    }
                    var size = new float2(
math.clamp(maxWidth, config->size.minWidth.Normalize(*context), config->size.maxWidth.Normalize(*context)),
math.clamp(totalHeight - config->padding.Y.Normalize(*context), config->size.minHeight.Normalize(*context), config->size.maxHeight.Normalize(*context))
);
                    UIJobUtility.AdjustPosition(size, boxConfig, selfPtr, statePtr, stateChildCount, stateChildren);
                    selfPtr->size += new float2(config->padding.X.Normalize(*context) + config->padding.Z.Normalize(*context), config->padding.Y.Normalize(*context) + config->padding.W.Normalize(*context));
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, config, vertexDataPtr + vertexDataOffset, context);
                    break;
                default:
                    break;
            }
        }
    }
}