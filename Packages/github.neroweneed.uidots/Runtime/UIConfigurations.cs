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
namespace NeroWeNeed.UIDots {
    public interface IInitializable {
        void PreInit(UIConfig config, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context);
        void PostInit(UIConfig config, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context);
    }
    public struct UIConfig {
        public static readonly UIConfig DEFAULT = new UIConfig
        {
            display = DisplayConfig.DEFAULT,
            position = PositionConfig.DEFAULT,
            size = SizeConfig.DEFAULT,
            font = FontConfig.DEFAULT,
            background = BackgroundConfig.DEFAULT
        };
        public LocalizedStringPtr name;
        public DisplayConfig display;
        public PositionConfig position;
        [Embed]
        public SizeConfig size;
        public BoxData<UILength> margin;
        public BoxData<UILength> padding;
        public FontConfig font;
        public BackgroundConfig background;
        public BorderConfig border;
        [HideInDecompositionAttribute]
        public ConfigPreprocessor preprocessor;
    }
    [Flags]
    public enum ConfigPreprocessor : byte {
        None = 0,
        TextProcessing = 1
    }

    public struct DisplayConfig {
        public static readonly DisplayConfig DEFAULT = new DisplayConfig
        {
            opacity = 1f,
            display = VisibilityStyle.Visible,
            visibile = VisibilityStyle.Visible,
            overflow = VisibilityStyle.Visible
        };
        public float opacity;
        public VisibilityStyle display, visibile, overflow;

    }
    public struct PositionConfig {
        public static readonly PositionConfig DEFAULT = new PositionConfig
        {
            absolute = false
        };
        public bool absolute;
        public BoxData<UILength> position;
    }
    public struct SizeConfig {
        public static readonly SizeConfig DEFAULT = new SizeConfig
        {
            minWidth = new UILength(0, UILengthUnit.Px),
            maxWidth = new UILength(float.PositiveInfinity, UILengthUnit.Px),
            width = new UILength(float.NaN,UILengthUnit.Px),
            height = new UILength(float.NaN, UILengthUnit.Px),
            minHeight = new UILength(0, UILengthUnit.Px),
            maxHeight = new UILength(float.PositiveInfinity, UILengthUnit.Px),
        };
        public UILength minWidth, width, maxWidth, minHeight, height, maxHeight;
    }

    public enum VisibilityStyle : byte {
        Hidden = 0, Visible = 1
    }

    public struct FontConfig {
        public static readonly FontConfig DEFAULT = new FontConfig
        {
            size = new UILength(12, UILengthUnit.Px)
        };
        [AssetReference]
        public BlittableAssetReference asset;
        public UILength size;
        public Color32 color;
        public HorizontalAlignStyle horizontalAlign;
        public VerticalAlignStyle verticalAlign;
        public bool wrap;
    }

    public enum HorizontalAlignStyle : byte {
        Left = 0, Center = 1, Right = 2
    }
    public enum VerticalAlignStyle : byte {
        Top = 0, Center = 1, Bottom = 2
    }
    public struct BackgroundConfig {
        public static readonly BackgroundConfig DEFAULT = default;
        public Color32 color;
        [AssetReference]
        public UVData image;
        public Color32 imageTint;
    }
    public struct BorderConfig {
        public static readonly BorderConfig DEFAULT = default;
        public BoxData<Color32> color;
        public BoxData<UILength> width;
        public BoxCornerData<UILength> radius;
    }
    public struct BoxConfig {
        public UILength spacing;
    }

    public struct TextConfig : IInitializable {
        public LocalizedStringPtr text;
        [HideInDecompositionAttribute]
        public FontInfo fontInfo;
        [HideInDecompositionAttribute]
        public int charInfoOffset;
        [HideInDecompositionAttribute]
        public int charInfoLength;
        public unsafe CharInfo GetCharInfo(void* configPtr, int index) {
            return UnsafeUtility.ReadArrayElement<CharInfo>((((IntPtr)configPtr) + charInfoOffset).ToPointer(), index);
        }

        public void PreInit(UIConfig config, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }
        public unsafe void PostInit(UIConfig config, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            TMP_FontAsset fontAsset;
            var guid = config.font.asset.ToHex();
#if UNITY_EDITOR
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
#else
            fontAsset = Addressables.LoadAsset<TMP_FontAsset>(guid);
#endif
            if (fontAsset != null) {
                fontInfo = new FontInfo(fontAsset);

                charInfoOffset = extraBytesStream.Length;
                for (int charIndex = 0; charIndex < text.length; charIndex++) {
                    UnsafeUtility.CopyPtrToStructure((((IntPtr)extraBytesStream.Data) + (text.offset - extraByteStreamOffset) + (charIndex * 2)).ToPointer(), out char character);
                    var charInfo = fontAsset.characterLookupTable[character];
                    var charInfoValue = new CharInfo
                    {
                        uvs = new float4(charInfo.glyph.glyphRect.x / (float)fontAsset.atlasWidth, charInfo.glyph.glyphRect.y / (float)fontAsset.atlasHeight, charInfo.glyph.glyphRect.width / (float)fontAsset.atlasWidth, charInfo.glyph.glyphRect.height / (float)fontAsset.atlasHeight),
                        metrics = charInfo.glyph.metrics,
                        index = (byte)(Array.IndexOf(context.fonts, guid) + (context.spriteGroup?.IsEmpty == false ? 1 : 0)),
                        unicode = charInfo.unicode
                    };
                    extraBytesStream.WriteBytes(UnsafeUtility.AddressOf(ref charInfoValue), UnsafeUtility.SizeOf<CharInfo>());
                }
                charInfoLength = (extraBytesStream.Length - charInfoOffset) / UnsafeUtility.SizeOf<CharInfo>();
                charInfoOffset += extraByteStreamOffset;
            }

        }

    }

}