using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NeroWeNeed.BehaviourGraph;
using NeroWeNeed.Commons;
using NeroWeNeed.UIDots;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;
[assembly: UIConfigBlock]
namespace NeroWeNeed.UIDots {

    public struct GraphHeaderConfig {
        /// <summary>
        /// Represents how large in size (Excluding the graph header config) the graph is in bytes.
        /// </summary>
        public ulong size;
        /// <summary>
        /// Represents how many nodes are present in the graph
        /// </summary>
        public int nodeCount;
    }
    public struct HeaderConfig {
        public FunctionPointer<UIPass> pass;
        public FunctionPointer<UIRenderBoxHandler> renderBoxHandler;
        public ulong configurationMask;
        /// <summary>
        /// Bit field for certain behaviour
        /// bit 1: whether to create dedicated entity for node or not.
        /// </summary>
        public byte flags;
        public int childCount;

    }
    [UIConfigBlock]
    public struct NameConfig {
        public LocalizedStringPtr name;
    }
    [UIConfigBlock]
    public struct BoxConfig {
        public BoxData<UILength> margin;
        public BoxData<UILength> padding;


    }

    [UIConfigBlock]
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
    [UIConfigBlock]
    public struct PositionConfig {
        public static readonly PositionConfig DEFAULT = new PositionConfig
        {
            absolute = false
        };
        public bool absolute;
        public BoxData<UILength> position;

    }
    [UIConfigBlock]
    public struct SizeConfig {
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
    }

    [UIConfigBlock("font")]
    public struct FontConfig {
        [AssetReference]
        public BlittableAssetReference asset;
        public UILength size;
        public Color32 color;
        public bool wrap;
    }

    [UIConfigBlock("background")]
    public struct BackgroundConfig {
        public Color32 color;
        [AssetReference]
        public UVData image;
        public Color32 imageTint;
    }
    [UIConfigBlock("border")]
    public struct BorderConfig {
        public BoxData<Color32> color;
        public BoxData<UILength> width;
        public BoxCornerData<UILength> radius;
    }
    [UIConfigBlock]
    public struct SequentialLayoutConfig {
        public UILength spacing;

        public HorizontalAlignment horizontalAlign;
        public VerticalAlignment verticalAlign;

    }
    [UIConfigBlock]
    public struct SelectableConfig {
        public FunctionPointer<UISelectDelegate> onSelect;
        public int priority;

    }
    [UIConfigBlock]
    public struct TextConfig {
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



    }

}