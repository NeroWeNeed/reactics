using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
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
    public struct Box {
        public float4 value;
        public float Width { get => value.x + value.z; }
        public float Height { get => value.y + value.w; }
        public static implicit operator Box(float4 value) => new Box { value = value };
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
        public float Normalize(UIContext context) {
            switch (unit) {
                case UILengthUnit.Auto:
                    break;
                case UILengthUnit.Px:
                    return value * context.pixelScale;
                case UILengthUnit.Cm:
                    return value * context.pixelScale * context.dpi * 2.54f;
                case UILengthUnit.Mm:
                    return value * context.pixelScale * context.dpi * 25.4f;
                case UILengthUnit.In:
                    return value * context.pixelScale * context.dpi;
                case UILengthUnit.Pt:
                    return value * context.pixelScale * context.dpi * (1f / 72f);
                case UILengthUnit.Pc:
                    return value * context.pixelScale * context.dpi * (1f / 6f);
                case UILengthUnit.Em:

                    break;
                case UILengthUnit.Ex:
                    break;
                case UILengthUnit.Ch:
                    break;
                case UILengthUnit.Rem:
                    break;
                case UILengthUnit.Vw:
                    return value * context.pixelScale * (context.size.x * 0.01f);
                case UILengthUnit.Vh:
                    return value * context.pixelScale * (context.size.y * 0.01f);
                case UILengthUnit.Vmin:
                    return value * context.pixelScale * (math.min(context.size.x, context.size.y) * 0.01f);
                case UILengthUnit.Vmax:
                    return value * context.pixelScale * (math.max(context.size.x, context.size.y) * 0.01f);
                case UILengthUnit.Percent:
                    return value * context.pixelScale * context.relativeTo;
                default:
                    break;
            }
            return 0f;
        }

        public override string ToString() {
            return $"{value} {unit}";
        }

    }
    public interface IUILengthContext {
        public float Dpi { get; }
        public float PixelScale { get; }
        public float2 ViewportSize { get; }
        public float RelativeTo { get; set; }

    }
    public struct UIContext : IComponentData {
        public float dpi;
        public float pixelScale;
        public float2 size;
        public float relativeTo;

        public static UIContext CreateContext(UICamera camera = null) {
            return new UIContext
            {
                dpi = Screen.dpi,
                pixelScale = camera == null ? 0.001f : 0.01f,
                size = camera == null ? new float2(float.PositiveInfinity, float.PositiveInfinity) : new float2(camera.UILayerCamera.orthographicSize * camera.UILayerCamera.aspect * 2, camera.UILayerCamera.orthographicSize * 2)
            };
        }

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
        public unsafe string ToString(void* configPtr) {
            var chars = (byte*)(((IntPtr)configPtr) + offset).ToPointer();
            return Encoding.Unicode.GetString(chars, length * 2);
        }
    }
    [Terminal]
    public unsafe struct BlittableAssetReference : IEquatable<BlittableAssetReference> {
        public fixed byte guid[16];
        public BlittableAssetReference(string guid) {
            if (GUID.TryParse(guid, out GUID result)) {
                fixed (byte* g = this.guid) {
                    UnsafeUtility.CopyStructureToPtr(ref result, g);
                }
            }
        }
        public BlittableAssetReference(GUID guid) {
            fixed (byte* g = this.guid) {
                UnsafeUtility.CopyStructureToPtr(ref guid, g);
            }
        }

        public bool Equals(BlittableAssetReference other) {
            fixed (byte* thisGuidPtr = guid) {
                return UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref other), thisGuidPtr, 16) == 0;
            }

        }
        public static implicit operator BlittableAssetReference(GUID guid) => new BlittableAssetReference(guid);
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
        public byte index;
        public GlyphMetrics metrics;
    }
    /// <summary>
    /// First byte denotes whether the value is an absolute or relative length.
    /// </summary>
    public enum UILengthUnit : byte {
        Px = 0b00000000, Cm = 0b00000010, Mm = 0b00000100, In = 0b00000110, Pt = 0b00001000, Pc = 0b00001010,
        Em = 0b00000001, Ex = 0b00000011, Ch = 0b00000101, Rem = 0b00000111, Vw = 0b00001001, Vh = 0b00001011, Vmin = 0b00001101, Vmax = 0b00001111, Percent = 0b00010001, Auto = 0b00010011
    }


    public static class UILengthExtensions {
        public static bool IsAbsolute(this UILengthUnit unit) => ((byte)unit & 0b00000001) == 0;
        public static bool IsRelative(this UILengthUnit unit) => ((byte)unit & 0b00000001) != 0;
        public static bool IsAbsolute(this UILength length) => ((byte)length.unit & 0b00000001) == 0;
        public static bool IsRelative(this UILength length) => ((byte)length.unit & 0b00000001) != 0;
        public static bool4 IsAbsolute<TComposite>(this TComposite self) where TComposite : struct, ICompositeData<UILength> {
            return new bool4(self.X.IsAbsolute(), self.Y.IsAbsolute(), self.Z.IsAbsolute(), self.W.IsAbsolute());
        }
        public static bool4 IsRelative<TComposite>(this TComposite self) where TComposite : struct, ICompositeData<UILength> {
            return new bool4(self.X.IsRelative(), self.Y.IsRelative(), self.Z.IsRelative(), self.W.IsRelative());
        }
        public static bool AllAbsolute<TComposite>(this TComposite self) where TComposite : struct, ICompositeData<UILength> {
            return self.X.IsAbsolute() && self.Y.IsAbsolute() && self.Z.IsAbsolute() && self.W.IsAbsolute();
        }
        public static bool AllRelative<TComposite>(this TComposite self) where TComposite : struct, ICompositeData<UILength> {
            return self.X.IsRelative() && self.Y.IsRelative() && self.Z.IsRelative() && self.W.IsRelative();
        }
        public static float NormalizedWidth<TComposite>(this TComposite self, UIContext context) where TComposite : struct, ICompositeData<UILength> {
            return self.X.Normalize(context) + self.Z.Normalize(context);
        }
        public static float NormalizedHeight<TComposite>(this TComposite self, UIContext context) where TComposite : struct, ICompositeData<UILength> {
            return self.Y.Normalize(context) + self.W.Normalize(context);
        }
        public static float4 Normalize<TComposite>(this TComposite self, UIContext context) where TComposite : struct, ICompositeData<UILength> {
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

    public interface ICompositeData { }
    public interface ICompositeData<TValue> : ICompositeData where TValue : struct {
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
    //TODO: Temporary fix until generics in queries are supported
    public struct GameObjectUICameraData : ISharedComponentData, IEquatable<GameObjectUICameraData> {
        public GameObject value;

        public UICamera Component;

        public GameObjectUICameraData(GameObject value) {
            this.value = value;
            Component = value.GetComponent<UICamera>();
        }
        public bool Equals(GameObjectUICameraData other) {
            return (value?.GetInstanceID() ?? 0) == (other.value?.GetInstanceID() ?? 0);
        }

        public override int GetHashCode() {
            int hashCode = -865696550;
            hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(value);
            return hashCode;
        }
    }
    [Flags]
    public enum Alignment : byte {
        Center = 0b00000000,
        Left = 0b00000001,
        Right = 0b00000010,
        Top = 0b00000100,
        Bottom = 0b00001000,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }
    public enum HorizontalAlignment : byte {
        Center = 0, Left = 2, Right = 1
    }
    public enum VerticalAlignment : byte {
        Center = 0, Top = 1, Bottom = 2

    }
    public static class AlignmentUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetOffset(byte alignment, float objectSize, float containerSize, byte containerAlignment = 0, byte objectAlignment = 0) {
            var multiplier = ((alignment & 0b00000001) - ((alignment & 0b00000010) >> 1)) * 0.5f;
            var containerOffset = ((containerAlignment & 0b00000001) - ((containerAlignment & 0b00000010) >> 1)) * 0.5f;
            var objectOffset = ((objectAlignment & 0b00000001) - ((objectAlignment & 0b00000010) >> 1)) * 0.5f;
            return ((multiplier + containerOffset) * containerSize) - ((multiplier + objectOffset) * objectSize);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetOffset(this HorizontalAlignment alignment, float objectSize, float containerSize, HorizontalAlignment containerAlignment = HorizontalAlignment.Center, HorizontalAlignment objectAlignment = HorizontalAlignment.Center) {
            return GetOffset((byte)alignment, objectSize, containerSize, (byte)containerAlignment, (byte)objectAlignment);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetOffset(this VerticalAlignment alignment, float objectSize, float containerSize, VerticalAlignment containerAlignment = VerticalAlignment.Center, VerticalAlignment objectAlignment = VerticalAlignment.Center) {
            return GetOffset((byte)alignment, objectSize, containerSize, (byte)containerAlignment, (byte)objectAlignment);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetOffset(this Alignment alignment, float2 objectSize, float2 containerSize, Alignment containerAlignment = Alignment.Center, Alignment objectAlignment = Alignment.Center) {
            var a = (byte)alignment;
            var c = (byte)containerAlignment;
            var o = (byte)objectAlignment;
            return new float2(
                GetOffset((byte)alignment, objectSize.x, containerSize.x, (byte)containerAlignment, (byte)objectAlignment),
                GetOffset((byte)(a >> 2), objectSize.y, containerSize.y, (byte)(c >> 2), (byte)(o >> 2))
            );
        }
        public static HorizontalAlignment AsHorizontal(this Alignment alignment) => (HorizontalAlignment)(((byte)alignment) & 0b00000011);
        public static HorizontalAlignment AsVertical(this Alignment alignment) => (HorizontalAlignment)(((byte)alignment >> 2) & 0b00000011);
        public static Alignment As2D(this HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment = VerticalAlignment.Center) => (Alignment)((((byte)horizontalAlignment) & 0b00000011) | (((byte)verticalAlignment) & 0b00001100));
        public static Alignment As2D(this VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) => (Alignment)((((byte)horizontalAlignment) & 0b00000011) | (((byte)verticalAlignment) & 0b00001100));

    }

}