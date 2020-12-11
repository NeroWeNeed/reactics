using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public static class StandardPropertyWriters {
        internal static readonly Dictionary<Type, IPropertyWriter> writers = new Dictionary<Type, IPropertyWriter>
        {
            { typeof(bool),PropertyWriterFactory.Create<bool>() },
            { typeof(char),PropertyWriterFactory.Create<char>() },
            { typeof(byte),PropertyWriterFactory.Create<byte>() },
            { typeof(ushort),PropertyWriterFactory.Create<ushort>() },
            { typeof(uint),PropertyWriterFactory.Create<uint>() },
            { typeof(ulong),PropertyWriterFactory.Create<ulong>() },
            { typeof(sbyte),PropertyWriterFactory.Create<sbyte>() },
            { typeof(short),PropertyWriterFactory.Create<short>() },
            { typeof(int),PropertyWriterFactory.Create<int>() },
            { typeof(long),PropertyWriterFactory.Create<long>() },
            { typeof(float),PropertyWriterFactory.Create<float>() },
            { typeof(double),PropertyWriterFactory.Create<double>() },
            { typeof(UILength),PropertyWriterFactory.Create<UILength>() },
            { typeof(Color32), new PropertyWriter<Color32>(TryParseColor) },
            { typeof(Color), new PropertyWriter<Color>((string s, out Color result) => ColorUtility.TryParseHtmlString(s, out result))},
            {typeof(UVData), new UVPropertyWriter() },
            { typeof(LocalizedStringPtr),new LocalizedStringPointerPropertyWriter() },
            { typeof(LocalizedAssetPtr),new LocalizedAssetPointerPropertyWriter() }
        };
        private static bool TryParseColor(string s, out Color32 result) {
            if (ColorUtility.TryParseHtmlString(s, out Color r)) { result = r; return true; }
            else {
                result = default; return false;
            }
        }
    }
}