using System;
using System.Runtime.CompilerServices;
using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Layouts {
    [BurstCompile]
    [UIDotsElement("Text", ConfigBlocks = UIConfigBlock.DisplayConfig | UIConfigBlock.NameConfig | UIConfigBlock.BackgroundConfig | UIConfigBlock.BorderConfig | UIConfigBlock.FontConfig | UIConfigBlock.BoxModelConfig | UIConfigBlock.TextConfig | UIConfigBlock.SizeConfig | UIConfigBlock.SelectableConfig,
    OptionalConfigBlocks = UIConfigBlock.MaterialConfig
    )]
    public unsafe static class TextElement {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UIRenderBoxCounter))]
        public static int RenderBoxCount(IntPtr configPtr, NodeInfo* nodeInfo) {
            TextConfig* config = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configPtr + nodeInfo->configOffset).ToPointer();
            return config->text.length + 1;
        }
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UIRenderPass))]
        public static void Render(IntPtr configPtr, NodeInfo* nodeInfo, UIPassState* statePtr, UIVertexData* vertexDataPtr, UIContextData* context) {
            IntPtr configSource = configPtr + nodeInfo->configOffset;
            TextConfig* textConfig = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configSource).ToPointer();
            BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
            FontConfig* fontConfig = (FontConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.FontConfig, configSource).ToPointer();
            BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
            UIRenderBoxWriters.WriteCharRenderBoxes(statePtr, configPtr + nodeInfo->nodeOffset, backgroundConfig, borderConfig, fontConfig, textConfig, vertexDataPtr, context);
        }
        [BurstCompile]
        [MonoPInvokeCallback(typeof(UILayoutPass))]
        public static void Layout(int childIndex, IntPtr configPtr, NodeInfo* nodeInfo, IntPtr statePtr, UIContextData* context) {
            if (childIndex < 0) {
                UIPassState* selfPtr = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * nodeInfo->index)).ToPointer();
                IntPtr configSource = configPtr + nodeInfo->configOffset;
                TextConfig* textConfig = (TextConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.TextConfig, configSource).ToPointer();
                BackgroundConfig* backgroundConfig = (BackgroundConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BackgroundConfig, configSource).ToPointer();
                FontConfig* fontConfig = (FontConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.FontConfig, configSource).ToPointer();
                BorderConfig* borderConfig = (BorderConfig*)UIConfigUtility.GetConfig(nodeInfo->configurationMask, UIConfigLayoutTable.BorderConfig, configSource).ToPointer();
                SizeText(selfPtr, configPtr + nodeInfo->nodeOffset, textConfig, fontConfig, context);
            }
        }
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SizeText(UIPassState* selfPtr, IntPtr configSource, TextConfig* textConfig, FontConfig* fontConfig, UIContextData* context) {
            var totalWidth = 0f;
            var height = fontConfig->size.Normalize(*context);
            var fontSizeScale = (height / textConfig->fontInfo.lineHeight) * textConfig->fontInfo.scale;
            for (int i = 0; i < textConfig->text.length; i++) {
                var charInfo = textConfig->GetCharInfo(configSource, i);
                totalWidth += (charInfo.metrics.horizontalBearingX + charInfo.metrics.horizontalAdvance) * fontSizeScale;
            }
            selfPtr->size = new float2(totalWidth + selfPtr->localBox.x + selfPtr->localBox.z, height + selfPtr->localBox.y + selfPtr->localBox.w);
        }
    }
}