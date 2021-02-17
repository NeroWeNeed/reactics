using System;
using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots.Layouts {
    public unsafe static class UIRenderBoxWriters {
        [BurstCompile]
        public static void WriteRenderBox(UIPassState* state, BackgroundConfig* backgroundConfig, BorderConfig* borderConfig, UIVertexData* vertexPtr, UIContextData* context) {
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 0, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 1, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 2, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 3, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + state->size.x, state->globalBox.y + state->size.y, 0),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomRight.Normalize(*context)),
            });
        }
        [BurstCompile]
        public static void WriteCharRenderBoxes(UIPassState* state, IntPtr config, BackgroundConfig* backgroundConfig, BorderConfig* borderConfig, FontConfig* fontConfig, TextConfig* textConfig, UIVertexData* vertexPtr, UIContextData* context) {
            var height = fontConfig->size.Normalize(*context);
            var fontSizeScale = (height / textConfig->fontInfo.lineHeight) * textConfig->fontInfo.scale;
            float offsetX = state->globalBox.x;
            float offsetY = state->globalBox.y;
            var nullBg = new float3(float.NaN, float.NaN, float.NaN);

            float width = 0f;
            for (int i = 1; i < textConfig->text.length + 1; i++) {
                var charInfo = textConfig->GetCharInfo(config, i - 1);
                UnsafeUtility.WriteArrayElement(vertexPtr, i * 4, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (-textConfig->fontInfo.descentLine * fontSizeScale) + ((charInfo.metrics.horizontalBearingY - charInfo.metrics.height) * fontSizeScale), -0.002f),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y, charInfo.index),
                    color = fontConfig->color

                });
                UnsafeUtility.WriteArrayElement(vertexPtr, (i * 4) + 1, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (-textConfig->fontInfo.descentLine * fontSizeScale) + ((charInfo.metrics.horizontalBearingY - charInfo.metrics.height) * fontSizeScale), -0.002f),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y, charInfo.index),
                    color = fontConfig->color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr, (i * 4) + 2, new UIVertexData
                {
                    position = new float3(offsetX + (charInfo.metrics.horizontalBearingX * fontSizeScale), offsetY + (-textConfig->fontInfo.descentLine * fontSizeScale) + ((charInfo.metrics.horizontalBearingY - charInfo.metrics.height) * fontSizeScale) + (charInfo.metrics.height * fontSizeScale), -0.002f),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = fontConfig->color
                });
                UnsafeUtility.WriteArrayElement(vertexPtr, (i * 4) + 3, new UIVertexData
                {
                    position = new float3(offsetX + ((charInfo.metrics.horizontalBearingX + charInfo.metrics.width) * fontSizeScale), offsetY + (-textConfig->fontInfo.descentLine * fontSizeScale) + ((charInfo.metrics.horizontalBearingY - charInfo.metrics.height) * fontSizeScale) + (charInfo.metrics.height * fontSizeScale), -0.002f),
                    normals = new float3(0, 0, -1),
                    background = nullBg,
                    foreground = new float3(charInfo.uvs.x + charInfo.uvs.z, charInfo.uvs.y + charInfo.uvs.w, charInfo.index),
                    color = fontConfig->color
                });
                offsetX += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;

                width += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;

            }
            //Write BG Box based on box defined by chars
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 0, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y, -0.001f),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 1, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y - backgroundConfig->image.value.w, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + width, state->globalBox.y, -0.001f),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.top.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.top.Normalize(*context))), borderConfig->radius.topRight.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 2, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x, state->globalBox.y + height, -0.001f),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.left.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.left.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomLeft.Normalize(*context)),
            });
            UnsafeUtility.WriteArrayElement<UIVertexData>(vertexPtr, 3, new UIVertexData
            {
                background = new float3(backgroundConfig->image.value.x + backgroundConfig->image.value.z, backgroundConfig->image.value.y, 0f),
                color = backgroundConfig->color,
                position = new float3(state->globalBox.x + width, state->globalBox.y + height, -0.001f),
                normals = new float3(0, 0, -1),
                border = new float3(math.atan2(borderConfig->width.bottom.Normalize(*context), borderConfig->width.right.Normalize(*context)), math.distance(float2.zero, new float2(borderConfig->width.right.Normalize(*context), borderConfig->width.bottom.Normalize(*context))), borderConfig->radius.bottomRight.Normalize(*context)),
            });
        }
    }
}