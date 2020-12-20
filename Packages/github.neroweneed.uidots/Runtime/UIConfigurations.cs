using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NeroWeNeed.BehaviourGraph;
using NeroWeNeed.Commons;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace NeroWeNeed.UIDots {



    public struct UIPropertyWriterContext {
#if UNITY_EDITOR
        public UIAssetGroup group;
        #endif
    }
    public interface IConfig {
        void PreInit(ulong mask, UIPropertyWriterContext context);
        void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context);
    }

    //Static Layout to define config structure
/*     public unsafe struct UIConfigLayoutSizes {

        public fixed int sizes[10];

        public int this[int key]
        {
            get => sizes[key];
            set => sizes[key] = value;
        }
        public int Length { get => 10; }
        public static UIConfigLayoutSizes Create() {
            var sizes = new UIConfigLayoutSizes();
            for (int i = 0; i < UIConfigLayout.ConfigTypes.Length; i++) {
                sizes[i] = UnsafeUtility.SizeOf(UIConfigLayout.ConfigTypes[i]);
            }
                             sizes[0] = UnsafeUtility.SizeOf<NameConfig>();
                        sizes[1] = UnsafeUtility.SizeOf<DisplayConfig>();
                        sizes[2] = UnsafeUtility.SizeOf<PositionConfig>();
                        sizes[3] = UnsafeUtility.SizeOf<SizeConfig>();
                        sizes[4] = UnsafeUtility.SizeOf<BoxConfig>();
                        sizes[5] = UnsafeUtility.SizeOf<FontConfig>();
                        sizes[6] = UnsafeUtility.SizeOf<BackgroundConfig>();
                        sizes[7] = UnsafeUtility.SizeOf<BorderConfig>();
                        sizes[8] = UnsafeUtility.SizeOf<SequentialLayoutConfig>();
                        sizes[9] = UnsafeUtility.SizeOf<TextConfig>(); 
            return sizes;
        }
    } */
    public static class UIConfigLayoutTypes {
        public static readonly Type[] ConfigTypes = new Type[] {
            typeof(NameConfig),
            typeof(DisplayConfig),
            typeof(PositionConfig),
            typeof(SizeConfig),
            typeof(BoxConfig),
            typeof(FontConfig),
            typeof(BackgroundConfig),
            typeof(BorderConfig),
            typeof(SequentialLayoutConfig),
            typeof(TextConfig),
            typeof(SelectableConfig)
        };
    }
    public static class UIConfigLayout {
        public const byte NameConfig = 0;
        public const byte DisplayConfig = 1;
        public const byte PositionConfig = 2;
        public const byte SizeConfig = 3;
        public const byte BoxConfig = 4;
        public const byte FontConfig = 5;
        public const byte BackgroundConfig = 6;
        public const byte BorderConfig = 7;
        public const byte SequentialLayoutConfig = 8;
        public const byte TextConfig = 9;

        public const byte SelectableConfig = 10;
        //TODO: Autogenerate code

        public static readonly int[] ConfigLengths = new int[] {
                  8,
                   8,
                    36,
                    48,
                    64,
                    32,
                    24,
                    80,
                    12,
                    36,
                    8
                };

        /*         public static readonly int[] ConfigLengths = new int[] {
                            UnsafeUtility.SizeOf<NameConfig>(),
                            UnsafeUtility.SizeOf<DisplayConfig>(),
                            UnsafeUtility.SizeOf<PositionConfig>(),
                            UnsafeUtility.SizeOf<SizeConfig>(),
                            UnsafeUtility.SizeOf<BoxConfig>(),
                            UnsafeUtility.SizeOf<FontConfig>(),
                            UnsafeUtility.SizeOf<BackgroundConfig>(),
                            UnsafeUtility.SizeOf<BorderConfig>(),
                            UnsafeUtility.SizeOf<SequentialLayoutConfig>(),
                            UnsafeUtility.SizeOf<TextConfig>()
                        }; */
        


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool HasName(ulong mask, IntPtr source) {
            return UIConfigLayout.TryGetConfig(mask, NameConfig, source, out IntPtr configBlock) && ((NameConfig*)configBlock.ToPointer())->name.IsCreated;
        }
        public unsafe static string GetName(ulong mask, IntPtr source) {
            var configBlock = GetConfig(mask, NameConfig, source);
            return ((NameConfig*)configBlock.ToPointer())->name.ToString(source.ToPointer());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasConfigBlock(ulong mask, byte config) {
            return (mask & (ulong)math.pow(2, config)) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CreateMask(params byte[] configs) {
            ulong mask = 0;
            for (int i = 0; i < configs.Length; i++) {
                mask |= (ulong)math.pow(2, configs[i]);
            }
            return mask;

        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOffset(ulong mask, byte config) {
            if ((mask & (ulong)math.pow(2, config)) == 0)
                return -1;
            int offset = 0;
            for (int i = 0; i < config; i++) {
                offset += (int)((byte)(mask >> i) & 1u) * ConfigLengths[i];
            }
            return offset;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetConfig(ulong mask, byte config, IntPtr source) {
            var offset = GetOffset(mask, config);
            if (offset < 0)
                return IntPtr.Zero;
            return source + offset;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryGetConfig(ulong mask, byte config, IntPtr source, out IntPtr configBlock) {
            var offset = GetOffset(mask, config);
            if (offset < 0) {
                configBlock = IntPtr.Zero;
                return false;
            }
            configBlock = source + offset;
            return true;
        }
        [BurstDiscard]
        public static void GetTypes(ulong mask, List<Type> types) {
            types.Clear();

            for (int i = 0; i < UIConfigLayoutTypes.ConfigTypes.Length; i++) {
                if (((byte)(mask >> i) & 1U) != 0) {
                    types.Add(UIConfigLayoutTypes.ConfigTypes[i]);
                }
            }
        }
        [BurstDiscard]
        public unsafe static void CreateConfiguration(ulong mask, List<IConfig> configs) {
            configs.Clear();
            int size = 0;
            for (int i = 0; i < UIConfigLayoutTypes.ConfigTypes.Length; i++) {
                if (((byte)(mask >> i) & 1U) != 0) {
                    var config = Activator.CreateInstance(UIConfigLayoutTypes.ConfigTypes[i]) as IConfig;
                    configs.Add(config);
                    size += ConfigLengths[i];
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLength(ulong mask) {
            int offset = 0;
            for (int i = 0; i < ConfigLengths.Length; i++) {
                offset += (int)((byte)(mask >> i) & 1U) * ConfigLengths[i];
            }
            return offset;
        }
    }
    [UIConfigBlock]
    public struct NameConfig : IConfig {
        public LocalizedStringPtr name;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            
        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            
        }
    }
    [UIConfigBlock]
    public struct BoxConfig : IConfig {
        public BoxData<UILength> margin;
        public BoxData<UILength> padding;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            
        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            
        }
    }

    [UIConfigBlock]
    public struct DisplayConfig : IConfig {
        public static readonly DisplayConfig DEFAULT = new DisplayConfig
        {
            opacity = 1f,
            display = VisibilityStyle.Visible,
            visibile = VisibilityStyle.Visible,
            overflow = VisibilityStyle.Visible
        };
        public float opacity;
        public VisibilityStyle display, visibile, overflow;

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            display = VisibilityStyle.Visible;
            visibile = VisibilityStyle.Visible;
            overflow = VisibilityStyle.Visible;
            opacity = 1f;
        }
        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

    }
    [UIConfigBlock]
    public struct PositionConfig : IConfig {
        public static readonly PositionConfig DEFAULT = new PositionConfig
        {
            absolute = false
        };
        public bool absolute;
        public BoxData<UILength> position;
        public void PreInit(ulong mask, UIPropertyWriterContext context) {

        }
        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }


    }
    [UIConfigBlock]
    public struct SizeConfig : IConfig {
        public static readonly SizeConfig DEFAULT = new SizeConfig
        {
            minWidth = new UILength(0, UILengthUnit.Px),
            maxWidth = new UILength(float.PositiveInfinity, UILengthUnit.Px),
            width = new UILength(float.NaN, UILengthUnit.Px),
            height = new UILength(float.NaN, UILengthUnit.Px),
            minHeight = new UILength(0, UILengthUnit.Px),
            maxHeight = new UILength(float.PositiveInfinity, UILengthUnit.Px),
        };
        public UILength minWidth, width, maxWidth, minHeight, height, maxHeight;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            minWidth = 0f.Px();
            maxWidth = float.PositiveInfinity.Px();
            width = float.NaN.Px();
            height = float.NaN.Px();
            minHeight = 0f.Px();
            maxHeight = float.PositiveInfinity.Px();
        }
    }

    [UIConfigBlock("font")]
    public struct FontConfig : IConfig {
        [AssetReference]
        public BlittableAssetReference asset;
        public UILength size;
        public Color32 color;
        public bool wrap;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            size = 12f.Px();
            color = Color.black;
        }
    }

    [UIConfigBlock("background")]
    public struct BackgroundConfig : IConfig {
        public Color32 color;
        [AssetReference]
        public UVData image;
        public Color32 imageTint;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {
            color = Color.white;
            imageTint = Color.white;
        }
    }
    [UIConfigBlock("border")]
    public struct BorderConfig : IConfig {
        public BoxData<Color32> color;
        public BoxData<UILength> width;
        public BoxCornerData<UILength> radius;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {

        }
    }
    public struct SequentialLayoutConfig : IConfig {
        public UILength spacing;

        public HorizontalAlignment horizontalAlign;
        public VerticalAlignment verticalAlign;

        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {

        }
    }
    
    public struct SelectableConfig : IConfig {
        public FunctionPointer<UISelectDelegate> onSelect;
        public int priority;
        public void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

        }
        public void PreInit(ulong mask, UIPropertyWriterContext context) {

        }
    }

    public struct TextConfig : IConfig {
        public LocalizedStringPtr text;
        [HideInDecompositionAttribute]
        public FontInfo fontInfo;
        [HideInDecompositionAttribute]
        public int charInfoOffset;
        [HideInDecompositionAttribute]
        public int charInfoLength;
        public unsafe CharInfo GetCharInfo(IntPtr configPtr, int index) {
            return UnsafeUtility.ReadArrayElement<CharInfo>((configPtr + charInfoOffset).ToPointer(), index);
        }

        public void PreInit(ulong mask, UIPropertyWriterContext context) {

        }
        public unsafe void PostInit(IntPtr config, ulong mask, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
#if UNITY_EDITOR
            TMP_FontAsset fontAsset;
            var fontConfigPtr = UIConfigLayout.GetConfig(mask, UIConfigLayout.FontConfig, config);
            
            
            if (fontConfigPtr == IntPtr.Zero)
                return;
            FontConfig* fontConfig = ((FontConfig*)fontConfigPtr);
            var guid = fontConfig->asset.ToHex();

            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));

            var index = context.group.GetAtlasIndex(guid);
            if (fontAsset != null && index >= 0) {

                fontInfo = new FontInfo(fontAsset);
                charInfoOffset = extraBytesStream.Length;
                for (int charIndex = 0; charIndex < text.length; charIndex++) {
                    UnsafeUtility.CopyPtrToStructure((((IntPtr)extraBytesStream.Data) + (text.offset - extraByteStreamOffset) + (charIndex * 2)).ToPointer(), out char character);
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
                charInfoLength = (extraBytesStream.Length - charInfoOffset) / UnsafeUtility.SizeOf<CharInfo>();
                charInfoOffset += extraByteStreamOffset;
            }
            #endif

        }

    }

}