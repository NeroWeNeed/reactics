using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    
    public static class StandardPropertyWriters {
        internal static readonly Dictionary<Type, IUIPropertyWriter> writers = new Dictionary<Type, IUIPropertyWriter>
        {
            { typeof(bool),UIPropertyWriterFactory.Create<bool>() },
            { typeof(char),UIPropertyWriterFactory.Create<char>() },
            { typeof(byte),UIPropertyWriterFactory.Create<byte>() },
            { typeof(ushort),UIPropertyWriterFactory.Create<ushort>() },
            { typeof(uint),UIPropertyWriterFactory.Create<uint>() },
            { typeof(ulong),UIPropertyWriterFactory.Create<ulong>() },
            { typeof(sbyte),UIPropertyWriterFactory.Create<sbyte>() },
            { typeof(short),UIPropertyWriterFactory.Create<short>() },
            { typeof(int),UIPropertyWriterFactory.Create<int>() },
            { typeof(long),UIPropertyWriterFactory.Create<long>() },
            { typeof(float),UIPropertyWriterFactory.Create<float>() },
            { typeof(double),UIPropertyWriterFactory.Create<double>() },
            { typeof(UILength),UIPropertyWriterFactory.Create<UILength>() },
            { typeof(Color32), new UIPropertyWriter<Color32>(TryParseColor) },
            { typeof(Color), new UIPropertyWriter<Color>((string s, out Color result) => ColorUtility.TryParseHtmlString(s, out result))},
            {typeof(UVData), new UIUVPropertyWriter() },
            { typeof(LocalizedStringPtr),new UILocalizedStringPointerPropertyWriter() },
            { typeof(BlittableAssetReference),new UIAssetPointerPropertyWriter() }
        };
        private static bool TryParseColor(string s, out Color32 result) {
            if (ColorUtility.TryParseHtmlString(s, out Color r)) { result = r; return true; }
            else {
                result = default; return false;
            }
        }
    }
}