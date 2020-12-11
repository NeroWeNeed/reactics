using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.Ast;
using TMPro;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.TextCore;

namespace NeroWeNeed.UIDots {

    [Terminal]
    public struct UVData {
        /// <summary>
        /// Stored x,y,width,height
        /// </summary>
        public float4 value;
    }
    [Terminal]
    public struct UILength {
        private static readonly Regex regex = new Regex("(NAN|[-]?INF(INITY)?|[-]?[0-9]+(?:\\.[0-9]+)?)([a-zA-Z%]*)?", RegexOptions.IgnoreCase);
        public float value;
        public UILengthUnit unit;

        public UILength(float value, UILengthUnit unit) {
            this.value = value;
            this.unit = unit;
        }
        public static bool TryParse(string s, out UILength result) {
            var match = regex.Match(s);

            if (match.Success) {
                if (!float.TryParse(match.Groups[1].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out float number)) {
                    number = default;
                }
                if (match.Groups.Count <= 3 || !Enum.TryParse<UILengthUnit>(match.Groups[3].Value, true, out UILengthUnit unit)) {
                    unit = UILengthUnit.Px;
                }
                result = new UILength(number, unit);
                return true;
            }
            else {
                result = default;
                return false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize(UILengthContext context) {
            switch (unit) {
                case UILengthUnit.Auto:
                    break;
                case UILengthUnit.Px:
                    return value;
                case UILengthUnit.Cm:
                    return value * context.dpi * 2.54f;
                case UILengthUnit.Mm:
                    return value * context.dpi * 25.4f;
                case UILengthUnit.In:
                    return value * context.dpi;
                case UILengthUnit.Pt:
                    return value * context.dpi * (1f / 72f);
                case UILengthUnit.Pc:
                    return value * context.dpi * (1f / 6f);
                case UILengthUnit.Em:

                    break;
                case UILengthUnit.Ex:
                    break;
                case UILengthUnit.Ch:
                    break;
                case UILengthUnit.Rem:
                    break;
                case UILengthUnit.Vw:
                    return value * (context.viewportSize.x * 0.01f);
                case UILengthUnit.Vh:
                    return value * (context.viewportSize.y * 0.01f);
                case UILengthUnit.Vmin:
                    return value * (math.min(context.viewportSize.x, context.viewportSize.y) * 0.01f);
                case UILengthUnit.Vmax:
                    return value * (math.max(context.viewportSize.x, context.viewportSize.y) * 0.01f);
                case UILengthUnit.Percent:
                    return value * context.relativeTo;
                default:
                    break;
            }
            return 0f;
        }

        public override string ToString() {
            return $"{value} {unit}";
        }

    }
    public struct UILengthContext {

        public int dpi;
        public float2 viewportSize;
        public float relativeTo;
    }
    public struct FontInfo {
        public float lineHeight;
        public float baseline;
        public float ascentLine;
        public float descentLine;
        public float scale;
        public FontInfo(TMP_FontAsset font) {
            lineHeight = font.faceInfo.lineHeight;
            scale = font.faceInfo.scale;
            baseline = font.faceInfo.baseline;
            ascentLine = font.faceInfo.ascentLine;
            descentLine = font.faceInfo.descentLine;
        }
    }
    [Terminal]
    public struct LocalizedStringPtr {
        public int offset;
        public int length;
        public bool IsCreated { get => length > 0; }
        public unsafe char GetChar(void* configPtr, int index) {
            return UnsafeUtility.ReadArrayElement<char>((((IntPtr)configPtr) + offset).ToPointer(), index);
        }
    }
    [Terminal]
    public unsafe struct LocalizedAssetPtr {
        public fixed byte guid[16];
        public string ToHex() {
            var sb = new StringBuilder(32);
            char t;
            for (int i = 0; i < 16; i++) {
                sb.AppendFormat("{0:x2}", guid[i]);
                t = sb[(i * 2) + 1];
                sb[(i * 2) + 1] = sb[i * 2];
                sb[i * 2] = t;
            }
            return sb.ToString();
        }


    }
    public struct LocalizedTextPtr {
        public FontInfo fontInfo;
        public int offset;
        public int length;
        public unsafe CharInfo GetCharInfo(void* configPtr, int index) {
            return UnsafeUtility.ReadArrayElement<CharInfo>((((IntPtr)configPtr) + offset).ToPointer(), index);
        }
    }
    public struct CharInfo {
        public uint unicode;
        public float4 uvs;
        public GlyphMetrics metrics;
    }
    public enum UILengthUnit : byte {
        Auto, Px, Cm, Mm, In, Pt, Pc, Em, Ex, Ch, Rem, Vw, Vh, Vmin, Vmax, Percent
    }

    public static class UILengthExtensions {
        public static float4 Normalize<TComposite>(this TComposite self, UILengthContext context) where TComposite : struct, ICompositeData<UILength> {
            return new float4(self.X.Normalize(context), self.Y.Normalize(context), self.Z.Normalize(context), self.W.Normalize(context));
        }

        public static UILength Cm(this float value) => new UILength(value, UILengthUnit.Cm);

        public static UILength Mm(this float value) => new UILength(value, UILengthUnit.Mm);

        public static UILength In(this float value) => new UILength(value, UILengthUnit.In);

        public static UILength Px(this float value) => new UILength(value, UILengthUnit.Px);

        public static UILength Pt(this float value) => new UILength(value, UILengthUnit.Pt);

        public static UILength Pc(this float value) => new UILength(value, UILengthUnit.Pc);

        public static UILength Em(this float value) => new UILength(value, UILengthUnit.Em);

        public static UILength Ex(this float value) => new UILength(value, UILengthUnit.Ex);

        public static UILength Ch(this float value) => new UILength(value, UILengthUnit.Ch);

        public static UILength Rem(this float value) => new UILength(value, UILengthUnit.Rem);

        public static UILength Vw(this float value) => new UILength(value, UILengthUnit.Vw);

        public static UILength Vh(this float value) => new UILength(value, UILengthUnit.Vh);

        public static UILength Vmin(this float value) => new UILength(value, UILengthUnit.Vmin);

        public static UILength Vmax(this float value) => new UILength(value, UILengthUnit.Vmax);

        public static UILength Percent(this float value) => new UILength(value, UILengthUnit.Percent);
    }


    public interface ICompositeData<TValue> where TValue : struct {
        public TValue X { get; set; }
        public TValue Y { get; set; }
        public TValue Z { get; set; }
        public TValue W { get; set; }
    }
    public struct BoxData<TValue> : ICompositeData<TValue> where TValue : struct {
        public TValue left, top, right, bottom;
        public TValue X { get => left; set => left = value; }

        public TValue Y { get => top; set => top = value; }

        public TValue Z { get => right; set => right = value; }

        public TValue W { get => bottom; set => bottom = value; }
    }
    public struct BoxCornerData<TValue> : ICompositeData<TValue> where TValue : struct {
        public TValue topLeft, topRight, bottomRight, bottomLeft;
        public TValue X { get => topLeft; set => topLeft = value; }

        public TValue Y { get => topRight; set => topRight = value; }

        public TValue Z { get => bottomRight; set => bottomRight = value; }

        public TValue W { get => bottomLeft; set => bottomLeft = value; }
    }

}