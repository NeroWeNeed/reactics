using System;
using NeroWeNeed.UIDots;

/// <summary>
/// AUTO-GENERATED, DO NOT EDIT.
/// Tables for configuration block layouts. Type Table is separated so the layout table can be accessed from burst.
/// </summary>
namespace NeroWeNeed.UIDots {
    public static class UIConfigLayoutTable {
        public const byte NameConfig = 0;
        public const byte BoxModelConfig = 1;
        public const byte DisplayConfig = 2;
        public const byte PositionConfig = 3;
        public const byte SizeConfig = 4;
        public const byte FontConfig = 5;
        public const byte BackgroundConfig = 6;
        public const byte BorderConfig = 7;
        public const byte MaterialConfig = 8;
        public const byte BoxLayoutConfig = 9;
        public const byte SelectableConfig = 10;
        public const byte TextConfig = 11;
        public static readonly int[] Lengths = new int[] {
            16,
            64,
            8,
            36,
            48,
            32,
            24,
            80,
            16,
            12,
            16,
            56
        };
    }

    public static class UIConfigTypeTable {
        public static readonly Type[] Types = new Type[] {
            typeof(NameConfig),
            typeof(BoxModelConfig),
            typeof(DisplayConfig),
            typeof(PositionConfig),
            typeof(SizeConfig),
            typeof(FontConfig),
            typeof(BackgroundConfig),
            typeof(BorderConfig),
            typeof(MaterialConfig),
            typeof(BoxLayoutConfig),
            typeof(SelectableConfig),
            typeof(TextConfig)
        };
    }
    [Flags]
    public enum UIConfigBlock : ulong {
        Empty = 0,
        NameConfig = 1,
        BoxModelConfig = 2,
        DisplayConfig = 4,
        PositionConfig = 8,
        SizeConfig = 16,
        FontConfig = 32,
        BackgroundConfig = 64,
        BorderConfig = 128,
        MaterialConfig = 256,
        BoxLayoutConfig = 512,
        SelectableConfig = 1024,
        TextConfig = 2048
    }
}
