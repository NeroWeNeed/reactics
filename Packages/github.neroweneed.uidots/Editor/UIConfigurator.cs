using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public static class StandardConfigurators {
        private static readonly Dictionary<Type, UIConfigurator> configurators = new Dictionary<Type, UIConfigurator>
        {
            {typeof(DisplayConfig),new DisplayConfigurator()},
            {typeof(SizeConfig),new SizeConfigurator()},
            {typeof(FontConfig),new FontConfigurator()},
            {typeof(BackgroundConfig),new BackgroundConfigurator()},
            {typeof(TextConfig),new TextConfigurator()}
        };

        public static void PreInit(Type type, IntPtr value, ulong mask, UIPropertyWriterContext context) {
            if (configurators.TryGetValue(type, out UIConfigurator configurator)) {
                configurator.PreInit(value, mask, context);
            }
        }
        public static void PostInit(Type type, IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            if (configurators.TryGetValue(type, out UIConfigurator configurator)) {
                configurator.PostInit(value, config, mask, extraBytesStream, extraByteStreamOffset, context);
            }
        }
    }
    public struct UIPropertyWriterContext {

        public UIAssetGroup group;

    }
    public unsafe abstract class UIConfigurator {
        public virtual void PreInit(IntPtr value, ulong mask, UIPropertyWriterContext context) { }
        public virtual void PostInit(IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) { }
    }
    public unsafe abstract class UIConfigurator<TValue> : UIConfigurator where TValue : unmanaged {
        public abstract void PreInit(TValue* value, ulong mask, UIPropertyWriterContext context);
        public abstract void PostInit(TValue* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context);
        public override void PreInit(IntPtr value, ulong mask, UIPropertyWriterContext context) => PreInit((TValue*)value.ToPointer(), mask, context);
        public override void PostInit(IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) => PostInit((TValue*)value.ToPointer(), config, mask, extraBytesStream, extraByteStreamOffset, context);
    }

    public unsafe class DisplayConfigurator : UIConfigurator<DisplayConfig> {
        public override unsafe void PostInit(DisplayConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public override unsafe void PreInit(DisplayConfig* value, ulong mask, UIPropertyWriterContext context) {
            value->display = VisibilityStyle.Visible;
            value->visibile = VisibilityStyle.Visible;
            value->overflow = VisibilityStyle.Visible;
            value->opacity = 1f;
        }
    }
    public unsafe class SizeConfigurator : UIConfigurator<SizeConfig> {
        public override unsafe void PostInit(SizeConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public override unsafe void PreInit(SizeConfig* value, ulong mask, UIPropertyWriterContext context) {
            value->minWidth = 0f.Px();
            value->maxWidth = float.PositiveInfinity.Px();
            value->width = float.NaN.Px();
            value->height = float.NaN.Px();
            value->minHeight = 0f.Px();
            value->maxHeight = float.PositiveInfinity.Px();
        }
    }
    public unsafe class FontConfigurator : UIConfigurator<FontConfig> {
        public override unsafe void PostInit(FontConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public override unsafe void PreInit(FontConfig* value, ulong mask, UIPropertyWriterContext context) {
            value->size = 12f.Px();
            value->color = Color.black;
        }
    }
    public unsafe class BackgroundConfigurator : UIConfigurator<BackgroundConfig> {
        public override unsafe void PostInit(BackgroundConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public override unsafe void PreInit(BackgroundConfig* value, ulong mask, UIPropertyWriterContext context) {
            value->color = Color.white;
            value->imageTint = Color.white;
        }
    }
    public unsafe class TextConfigurator : UIConfigurator<TextConfig> {
        public override unsafe void PostInit(TextConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            TMP_FontAsset fontAsset = null;
            var fontConfigPtr = UIConfigUtility.GetConfig(mask, UIConfigLayoutTable.FontConfig, config);


            if (fontConfigPtr == IntPtr.Zero)
                return;
            FontConfig* fontConfig = ((FontConfig*)fontConfigPtr);
            var guid = fontConfig->asset.ToHex();

#if UNITY_EDITOR
            fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
//var task = Addressables.LoadAssetAsync<TMP_FontAsset>(guid);
      //fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
#endif






            var index = context.group.GetAtlasIndex(guid);
            if (fontAsset != null && index >= 0) {

                value->fontInfo = new FontInfo(fontAsset);
                value->charInfoOffset = extraBytesStream.Length;
                for (int charIndex = 0; charIndex < value->text.length; charIndex++) {
                    UnsafeUtility.CopyPtrToStructure((((IntPtr)extraBytesStream.Data) + (value->text.offset - extraByteStreamOffset) + (charIndex * 2)).ToPointer(), out char character);
                    var charInfo = fontAsset.characterLookupTable[character];
                    var charInfoValue = new CharInfo
                    {
                        uvs = new float4(charInfo.glyph.glyphRect.x / (float)fontAsset.atlasWidth, charInfo.glyph.glyphRect.y / (float)fontAsset.atlasHeight, charInfo.glyph.glyphRect.width / (float)fontAsset.atlasWidth, charInfo.glyph.glyphRect.height / (float)fontAsset.atlasHeight),
                        metrics = charInfo.glyph.metrics,
                        index = (byte)index,
                        unicode = charInfo.unicode
                    };
                    extraBytesStream.WriteBytes(UnsafeUtility.AddressOf(ref charInfoValue), UnsafeUtility.SizeOf<CharInfo>());
                }
                value->charInfoLength = (extraBytesStream.Length - value->charInfoOffset) / UnsafeUtility.SizeOf<CharInfo>();
                value->charInfoOffset += extraByteStreamOffset;
            }
        }

        public override unsafe void PreInit(TextConfig* value, ulong mask, UIPropertyWriterContext context) {
        }
    }
}