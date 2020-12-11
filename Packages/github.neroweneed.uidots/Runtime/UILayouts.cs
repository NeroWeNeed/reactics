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
        public static void WriteRenderBox(UIPassState* state, UIConfig* config, UIVertexData* vertexPtr, int vertexOffset, UILengthContext context) {
            UnsafeUtility.WriteArrayElement<UIVertexData>((((IntPtr)vertexPtr) + vertexOffset).ToPointer(), 0, new UIVertexData
            {
                uv = new float3(config->background.image.value.x, config->background.image.value.y - config->background.image.value.w, 0),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->localOffset.x, state->globalBox.y + state->localOffset.y, 0f),
                normals = new float3(0, 0, 1),
                border = new float3(math.atan2(config->border.width.top.Normalize(context), config->border.width.left.Normalize(context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(context), config->border.width.top.Normalize(context))), config->border.radius.topLeft.Normalize(context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>((((IntPtr)vertexPtr) + vertexOffset).ToPointer(), 1, new UIVertexData
            {
                uv = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y - config->background.image.value.w, 0),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->size.x + state->globalBox.z + state->localOffset.x, state->globalBox.y + state->localOffset.y, 0f),
                normals = new float3(0, 0, 1),
                border = new float3(math.atan2(config->border.width.top.Normalize(context), config->border.width.right.Normalize(context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(context), config->border.width.top.Normalize(context))), config->border.radius.topRight.Normalize(context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>((((IntPtr)vertexPtr) + vertexOffset).ToPointer(), 2, new UIVertexData
            {
                uv = new float3(config->background.image.value.x, config->background.image.value.y, 0),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->localOffset.x, state->globalBox.y + state->size.y + state->globalBox.w + state->localOffset.y, 0f),
                normals = new float3(0, 0, 1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(context), config->border.width.left.Normalize(context)), math.distance(float2.zero, new float2(config->border.width.left.Normalize(context), config->border.width.bottom.Normalize(context))), config->border.radius.bottomLeft.Normalize(context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>((((IntPtr)vertexPtr) + vertexOffset).ToPointer(), 3, new UIVertexData
            {
                uv = new float3(config->background.image.value.x + config->background.image.value.z, config->background.image.value.y, 0),
                color = config->background.color,
                position = new float3(state->globalBox.x + state->localOffset.x + state->size.x + state->globalBox.z, state->globalBox.y + state->size.y + state->globalBox.w + state->localOffset.y, 0f),
                normals = new float3(0, 0, 1),
                border = new float3(math.atan2(config->border.width.bottom.Normalize(context), config->border.width.right.Normalize(context)), math.distance(float2.zero, new float2(config->border.width.right.Normalize(context), config->border.width.bottom.Normalize(context))), config->border.radius.bottomRight.Normalize(context)),
            });
        }
        [BurstCompile]
        public static unsafe void WriteCharRenderBoxes(UIConfig* configPtr, UIPassState* state, TextConfig* textConfigPtr, void* vertexDataPtr, UILengthContext* context) {
            float offsetX = state->globalBox.x + state->localOffset.x;
            float offsetY = state->globalBox.y + state->localOffset.y;
            Debug.Log(new float2(offsetX, offsetY));
            var height = configPtr->font.size.Normalize(*context);
            var fontSizeScale = height / textConfigPtr->fontInfo.lineHeight;

            for (int i = 0; i < textConfigPtr->text.length; i++) {
                var charInfo = textConfigPtr->GetCharInfo(configPtr, i);
                UnsafeUtility.WriteArrayElement(vertexDataPtr, i * 4, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * textConfigPtr->fontInfo.scale * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * textConfigPtr->fontInfo.scale * fontSizeScale), 0f),
                    normals = new float3(0, 0, 1),
                    uv = new float3(charInfo.uvs.x, charInfo.uvs.y, 1f)
                });
                UnsafeUtility.WriteArrayElement(vertexDataPtr, (i * 4) + 1, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * textConfigPtr->fontInfo.scale * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY))) * textConfigPtr->fontInfo.scale * fontSizeScale), 0f),
                    normals = new float3(0, 0, 1),
                    uv = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y, 1f)
                });
                UnsafeUtility.WriteArrayElement(vertexDataPtr, (i * 4) + 2, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * textConfigPtr->fontInfo.scale * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * textConfigPtr->fontInfo.scale * fontSizeScale), 0f),
                    normals = new float3(0, 0, 1),
                    uv = new float3(charInfo.uvs.x, charInfo.uvs.y + charInfo.uvs.w, 1f)
                });
                UnsafeUtility.WriteArrayElement(vertexDataPtr, (i * 4) + 3, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * textConfigPtr->fontInfo.scale * fontSizeScale), offsetY + (((height - (charInfo.metrics.height - charInfo.metrics.horizontalBearingY) + charInfo.metrics.height)) * textConfigPtr->fontInfo.scale * fontSizeScale), 0f),
                    normals = new float3(0, 0, 1),
                    uv = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y + charInfo.uvs.w, 1f)
                });

                offsetX += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * textConfigPtr->fontInfo.scale * fontSizeScale;
            }
        }
    }
    [BurstCompile]
    public unsafe static class UILayouts {
        [BurstCompile]
        public static int TextElementRenderBoxHandler(void* configPtr, int configOffset, int configLength) {
            TextConfig* config = (TextConfig*)(((IntPtr)configPtr) + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
            return config->text.length;
        }
        [BurstCompile]
        [UIDotsElement("TextElement", typeof(TextConfig))]
        [UIDotsRenderBoxHandler(nameof(TextElementRenderBoxHandler))]
        public static void TextElement(
        byte type,
        void* configPtr,
        int configOffset,
        int configLength,
        void* statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        void* vertexDataPtr,
        int vertexDataOffset,
        void* contextPtr
        ) {
            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            UIConfig* config = (UIConfig*)(((IntPtr)configPtr) + configOffset).ToPointer();
            TextConfig* textConfig = (TextConfig*)(((IntPtr)configPtr) + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
            UILengthContext* context = (UILengthContext*)contextPtr;
            switch ((UIPassType)type) {
                case UIPassType.LayoutSelf:
                    break;
                case UIPassType.SizeSelf:
                    var totalWidth = 0f;
                    var height = config->font.size.Normalize(*context);
                    var fontSizeScale = height / textConfig->fontInfo.lineHeight;
                    for (int i = 0; i < textConfig->text.length; i++) {
                        var charInfo = textConfig->GetCharInfo(config, i);
                        totalWidth += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale * textConfig->fontInfo.scale;
                        Debug.Log(totalWidth);
                    }
                    selfPtr->size = new float2(totalWidth, height);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteCharRenderBoxes(config, selfPtr, textConfig, (((IntPtr)vertexDataPtr) + vertexDataOffset).ToPointer(), context);
                    break;
                default:
                    break;

            }
        }


        [BurstCompile]
        [UIDotsElement("HBox", typeof(BoxConfig))]
        public static void HBox(
            byte type,
            void* configPtr,
            int configOffset,
            int configLength,
            void* statePtr,
            int* stateChildren,
            int stateIndex,
            int stateChildLocalIndex,
            int stateChildCount,
            void* vertexDataPtr,
            int vertexDataOffset,
            void* contextPtr
            ) {

            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            UIConfig* config = (UIConfig*)(((IntPtr)configPtr) + configOffset).ToPointer();
            BoxConfig* boxConfig = (BoxConfig*)(((IntPtr)configPtr) + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
            UILengthContext* context = (UILengthContext*)contextPtr;
            switch ((UIPassType)type) {
                case UIPassType.LayoutChild:
                    break;
                case UIPassType.SizeChild:
                    break;
                case UIPassType.LayoutSelf:
                    break;
                case UIPassType.SizeSelf:
                    float totalWidth = 0f;
                    float maxHeight = 0f;
                    for (int i = 0; i < stateChildCount; i++) {
                        var childPassData = UnsafeUtility.ReadArrayElement<UIPassState>(statePtr, UnsafeUtility.ReadArrayElement<int>(stateChildren, i) + 1);
                        var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i) + 1))).ToPointer();
                        childState->localOffset.x += totalWidth;
                        childState->localOffset.y = selfPtr->localOffset.y;
                        totalWidth += childPassData.size.x + childPassData.margin.x + childPassData.margin.z + childPassData.padding.x + childPassData.padding.z;
                        maxHeight = math.max(maxHeight, childPassData.size.y + childPassData.margin.y + childPassData.padding.y + childPassData.margin.w + childPassData.padding.w);
                        if (i + 1 < stateChildCount)
                            totalWidth += boxConfig->spacing.Normalize(*context);
                    }
                    selfPtr->size = new float2(totalWidth, maxHeight);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, config, (UIVertexData*)vertexDataPtr, vertexDataOffset, *context);
                    break;
                default:
                    break;
            }
        }
        [BurstCompile]
        [UIDotsElement("VBox", typeof(BoxConfig))]
        public static void VBox(
            byte type,
            void* configPtr,
            int configOffset,
            int configLength,
            void* statePtr,
            int* stateChildren,
            int stateIndex,
            int stateChildLocalIndex,
            int stateChildCount,
            void* vertexDataPtr,
            int vertexDataOffset,
            void* contextPtr
        ) {

            UIPassState* selfPtr = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * stateIndex)).ToPointer();
            UIConfig* config = (UIConfig*)(((IntPtr)configPtr) + configOffset).ToPointer();
            BoxConfig* boxConfig = (BoxConfig*)(((IntPtr)configPtr) + configOffset + UnsafeUtility.SizeOf<UIConfig>()).ToPointer();
            UILengthContext* context = (UILengthContext*)contextPtr;
            switch ((UIPassType)type) {
                case UIPassType.LayoutChild:
                    break;
                case UIPassType.SizeChild:
                    break;
                case UIPassType.LayoutSelf:
                    break;
                case UIPassType.SizeSelf:
                    float totalHeight = 0f;
                    float maxWidth = 0f;
                    for (int i = 0; i < stateChildCount; i++) {
                        var childPassData = UnsafeUtility.ReadArrayElement<UIPassState>(statePtr, UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i) + 1);
                        var childState = (UIPassState*)(((IntPtr)statePtr) + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, stateChildCount - 1 - i) + 1))).ToPointer();
                        totalHeight += childPassData.size.y + childPassData.margin.y + childPassData.margin.w + childPassData.padding.y + childPassData.padding.w;
                        childState->localOffset.y += totalHeight;
                        childState->localOffset.x = selfPtr->localOffset.x;
                        maxWidth = math.max(maxWidth, childPassData.size.x + childPassData.margin.x + childPassData.padding.x + childPassData.margin.z + childPassData.padding.x);
                        if (i + 1 < stateChildCount)
                            totalHeight += boxConfig->spacing.Normalize(*context);
                    }
                    selfPtr->size = new float2(maxWidth, totalHeight);
                    break;
                case UIPassType.Render:
                    UIRenderBoxWriters.WriteRenderBox(selfPtr, config, (UIVertexData*)vertexDataPtr, vertexDataOffset, *context);
                    break;
                default:
                    break;
            }
        }
    }
}