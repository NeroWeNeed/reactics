using System;
using NeroWeNeed.UIDots;

/// <summary>
/// AUTO-GENERATED, DO NOT EDIT.
/// Tables for configuration block layouts. Type Table is separated so the layout table can be accessed from burst.
/// </summary>

namespace NeroWeNeed.UIDots {
    public static class UIConfigLayoutTable {
        public const byte NameConfig = 0;
        public const byte BoxConfig = 1;
        public const byte DisplayConfig = 2;
        public const byte PositionConfig = 3;
        public const byte SizeConfig = 4;
        public const byte FontConfig = 5;
        public const byte BackgroundConfig = 6;
        public const byte BorderConfig = 7;
        public const byte SequentialLayoutConfig = 8;
        public const byte SelectableConfig = 9;
        public const byte TextConfig = 10;
        public static readonly int[] Lengths = new int[] {
            8,
            64,
            8,
            36,
            48,
            32,
            24,
            80,
            12,
            16,
            36
        };
    }

    public static class UIConfigTypeTable {
        public static readonly Type[] Types = new Type[] {
            typeof(NameConfig),
            typeof(BoxConfig),
            typeof(DisplayConfig),
            typeof(PositionConfig),
            typeof(SizeConfig),
            typeof(FontConfig),
            typeof(BackgroundConfig),
            typeof(BorderConfig),
            typeof(SequentialLayoutConfig),
            typeof(SelectableConfig),
            typeof(TextConfig)
        };
    }
}
