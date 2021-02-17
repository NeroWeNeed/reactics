using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public static class StandardConfigurationHandlers {
        private static readonly Dictionary<Type, UIConfigurationHandler> configurators = new Dictionary<Type, UIConfigurationHandler>
        {
            {typeof(DisplayConfig),new DisplayConfigurationHandler()},
            {typeof(SizeConfig),new SizeConfigurationHandler()},
            {typeof(FontConfig),new FontConfigurationHandler()},
            {typeof(BackgroundConfig),new BackgroundConfigurationHandler()},
            {typeof(TextConfig),new TextConfigurationHandler()},
            {typeof(NameConfig),new NameConfigurationHandler() },
            {typeof(MaterialConfig),new CustomShaderConfigurationHandler() }
        };

        public static byte PreInit(Type type, IntPtr value, ulong mask, ref UIPropertyWriterContext context) {
            if (configurators.TryGetValue(type, out UIConfigurationHandler configurator)) {
                return configurator.PreInit(value, mask, ref context);
            }
            else {
                return 0;
            }
        }
        public static byte PostInit(Type type, IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            if (configurators.TryGetValue(type, out UIConfigurationHandler configurator)) {
                return configurator.PostInit(value, config, mask, extraBytesStream, extraByteStreamOffset, context);
            }
            else {
                return 0;
            }
        }
    }
    public struct UIPropertyWriterContext {

        public UIAssetGroup group;

    }
    public unsafe abstract class UIConfigurationHandler {
        public virtual byte PreInit(IntPtr value, ulong mask, ref UIPropertyWriterContext context) { return 0; }
        public virtual byte PostInit(IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) { return 0; }
    }
    public unsafe abstract class UIConfigurationHandler<TValue> : UIConfigurationHandler where TValue : unmanaged {
        public abstract byte PreInit(TValue* value, ulong mask, ref UIPropertyWriterContext context);
        public abstract byte PostInit(TValue* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context);
        public override byte PreInit(IntPtr value, ulong mask, ref UIPropertyWriterContext context) => PreInit((TValue*)value.ToPointer(), mask, ref context);
        public override byte PostInit(IntPtr value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) => PostInit((TValue*)value.ToPointer(), config, mask, extraBytesStream, extraByteStreamOffset, context);
    }

    public unsafe class DisplayConfigurationHandler : UIConfigurationHandler<DisplayConfig> {
        public override unsafe byte PostInit(DisplayConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return 0;
        }

        public override unsafe byte PreInit(DisplayConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            value->display = VisibilityStyle.Visible;
            value->visible = VisibilityStyle.Visible;
            value->overflow = VisibilityStyle.Visible;
            value->opacity = 1f;
            return 0;
        }
    }
    public unsafe class CustomShaderConfigurationHandler : UIConfigurationHandler<MaterialConfig> {
        public override unsafe byte PostInit(MaterialConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return 1;
        }

        public override unsafe byte PreInit(MaterialConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            return 1;
        }
    }
    public unsafe class SizeConfigurationHandler : UIConfigurationHandler<SizeConfig> {
        public override unsafe byte PostInit(SizeConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return 0;
        }

        public override unsafe byte PreInit(SizeConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            value->minWidth = 0f.Px();
            value->maxWidth = float.PositiveInfinity.Px();
            value->width = float.NaN.Px();
            value->height = float.NaN.Px();
            value->minHeight = 0f.Px();
            value->maxHeight = float.PositiveInfinity.Px();
            return 0;
        }
    }
    public unsafe class FontConfigurationHandler : UIConfigurationHandler<FontConfig> {
        public override unsafe byte PostInit(FontConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return 0;
        }

        public override unsafe byte PreInit(FontConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            value->size = 12f.Px();
            value->color = Color.black;
            return 0;
        }
    }
    public unsafe class BackgroundConfigurationHandler : UIConfigurationHandler<BackgroundConfig> {
        public override unsafe byte PostInit(BackgroundConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return 0;
        }

        public override unsafe byte PreInit(BackgroundConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            value->color = Color.white;
            value->imageTint = Color.white;
            return 0;
        }
    }
    public unsafe class NameConfigurationHandler : UIConfigurationHandler<NameConfig> {
        public override unsafe byte PostInit(NameConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            return (byte)(value->name.IsCreated ? 1 : 0);
        }
        public override unsafe byte PreInit(NameConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            return 0;
        }
    }
    public unsafe class TextConfigurationHandler : UIConfigurationHandler<TextConfig> {
        public override unsafe byte PostInit(TextConfig* value, IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, long extraByteStreamOffset, UIPropertyWriterContext context) {
            TMP_FontAsset fontAsset = null;
            var fontConfigPtr = UIConfigUtility.GetConfig(mask, UIConfigLayoutTable.FontConfig, config);


            if (fontConfigPtr == IntPtr.Zero)
                return 0;
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
                    UnsafeUtility.CopyPtrToStructure(new IntPtr(((IntPtr)extraBytesStream.Data).ToInt64() + (value->text.offset - extraByteStreamOffset) + (charIndex * 2)).ToPointer(), out char character);
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
                value->charInfoLength = value->text.length;
                //value->charInfoLength = (int)((extraBytesStream.Length - value->charInfoOffset) / UnsafeUtility.SizeOf<CharInfo>());
                value->charInfoOffset += extraByteStreamOffset;

            }
            return 0;
        }

        public override unsafe byte PreInit(TextConfig* value, ulong mask, ref UIPropertyWriterContext context) {
            return 0;
        }
    }
}