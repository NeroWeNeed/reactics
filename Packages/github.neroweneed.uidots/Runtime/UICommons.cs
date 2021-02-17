using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
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
    [Serializable]
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
        public float Normalize(UIContextData context) {
            switch (unit) {
                case UILengthUnit.Auto:
                    break;
                case UILengthUnit.Px:
                    return value * context.pixelScale;
                case UILengthUnit.Cm:
                    return value * context.pixelScale * context.dpi / 2.54f;
                case UILengthUnit.Mm:
                    return value * context.pixelScale * context.dpi / 25.4f;
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
        /// <summary>
        /// Offset of the string relative to the start of the Config Block section of a node.
        /// </summary>
        public long offset;
        public int length;
        public bool IsCreated { get => length > 0; }
        public unsafe char GetChar(void* configPtr, int index) {
            return UnsafeUtility.ReadArrayElement<char>(new IntPtr(((IntPtr)configPtr).ToInt64() + offset).ToPointer(), index);
        }
        public unsafe string ToString(void* configPtr) {
            var chars = (byte*)new IntPtr(((IntPtr)configPtr).ToInt64() + offset).ToPointer();
            return Encoding.Unicode.GetString(chars, length * 2);
        }
    }
    [Terminal]
    public unsafe struct BlittableAssetReference : IEquatable<BlittableAssetReference> {
        public fixed byte guid[16];
        public BlittableAssetReference(string guid) {
#if UNITY_EDITOR
            if (UnityEditor.GUID.TryParse(guid, out UnityEditor.GUID result)) {
                fixed (byte* g = this.guid) {
                    UnsafeUtility.CopyStructureToPtr(ref result, g);
                }
            }
#endif
        }


        public bool Equals(BlittableAssetReference other) {
            fixed (byte* thisGuidPtr = guid) {
                return UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref other), thisGuidPtr, 16) == 0;
            }
        }

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


        public override string ToString() {
            return ToHex();
        }

        public override int GetHashCode() {
            int hashCode = -780628215;
            for (int i = 0; i < 16; i++) {
                hashCode = hashCode * -1521134295 + guid[i].GetHashCode();
            }
            return hashCode;
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
        Em = 0b00000001, Ex = 0b00000011, Ch = 0b00000101, Rem = 0b00000111, Vw = 0b00001001, Vh = 0b00001011, Vmin = 0b00001101, Vmax = 0b00001111, Percent = 0b00010001, Auto = 0b00010011, Inherit = 0b00010101
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
        public static float NormalizedWidth<TComposite>(this TComposite self, UIContextData context) where TComposite : struct, ICompositeData<UILength> {
            return self.X.Normalize(context) + self.Z.Normalize(context);
        }
        public static float NormalizedHeight<TComposite>(this TComposite self, UIContextData context) where TComposite : struct, ICompositeData<UILength> {
            return self.Y.Normalize(context) + self.W.Normalize(context);
        }
        public static float4 Normalize<TComposite>(this TComposite self, UIContextData context) where TComposite : struct, ICompositeData<UILength> {
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
    public struct UICameraLayer : IComponentData {

    }
    [Flags]
    public enum Alignment : byte {
        Center = 0b00000000,
        Left = 0b00000001,
        Right = 0b00000010,
        Top = 0b00001000,
        Bottom = 0b00000100,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }
    public enum HorizontalAlignment : byte {
        Center = 0, Left = 1, Right = 2
    }
    public enum Direction : byte {
        Left = 0, Up = 1, Right = 2, Down = 3
    }
    public enum VerticalAlignment : byte {
        Center = 0, Top = 2, Bottom = 1

    }
    public static class AlignmentUtil {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetOffset(byte alignment, float objectSize, float containerSize, byte objectAlignment = 0) {
            var alignmentMultiplier = (((alignment & 2) >> 1) - (alignment & 1));
            var initialAlignmentMultiplier = (((objectAlignment & 2) >> 1) - (objectAlignment & 1));
            var diff = math.abs(containerSize - objectSize);
            // + (objectSize * objectMultiplier) - (containerSize * containerMultiplier)
            return diff * ((alignmentMultiplier - initialAlignmentMultiplier) * 0.5f);
        }

        /*         [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static float GetOffset(byte alignment, float objectSize, float containerSize, byte containerAlignment = 0, byte objectAlignment = 0) {
                    var multiplier = ((alignment & 1) - ((alignment & 2) >> 1)) * 0.5f;
                    var containerMultiplier = ((containerAlignment & 1)  - ((containerAlignment & 2) >> 1)) * 0.5f;
                    var objectMultiplier = ((objectAlignment & 1) - ((objectAlignment & 2) >> 1)) * 0.5f;
                    // + (objectSize * objectMultiplier) - (containerSize * containerMultiplier)
                    return ((multiplier + containerMultiplier) * containerSize) - ((multiplier + objectMultiplier) * objectSize);
                } */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetOffset(this HorizontalAlignment alignment, float objectSize, float containerSize, HorizontalAlignment objectAlignment = HorizontalAlignment.Center) {
            return GetOffset((byte)alignment, objectSize, containerSize, (byte)objectAlignment);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetOffset(this VerticalAlignment alignment, float objectSize, float containerSize, VerticalAlignment objectAlignment = VerticalAlignment.Center) {
            return GetOffset((byte)alignment, objectSize, containerSize, (byte)objectAlignment);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetOffset(this Alignment alignment, float2 objectSize, float2 containerSize, Alignment objectAlignment = Alignment.Center) {
            var a = (byte)alignment;
            var o = (byte)objectAlignment;
            return new float2(
                GetOffset((byte)alignment, objectSize.x, containerSize.x, o),
                GetOffset((byte)(a >> 2), objectSize.y, containerSize.y, (byte)(o >> 2))
            );
        }
        public static HorizontalAlignment AsHorizontal(this Alignment alignment) => (HorizontalAlignment)(((byte)alignment) & 0b00000011);
        public static HorizontalAlignment AsVertical(this Alignment alignment) => (HorizontalAlignment)(((byte)alignment >> 2) & 0b00000011);
        public static Alignment As2D(this HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment = VerticalAlignment.Center) => (Alignment)((((byte)horizontalAlignment) & 0b00000011) | (((byte)verticalAlignment << 2) & 0b00001100));
        public static Alignment As2D(this VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) => (Alignment)((((byte)horizontalAlignment) & 0b00000011) | (((byte)verticalAlignment << 2) & 0b00001100));

    }
    public static class UIJobUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int GetConfigOffset(BlobAssetReference<UIGraphOld> graph, int index, out int length) {
            int offset = 0;
            for (int currentIndex = 0; currentIndex < index; currentIndex++) {
                var size = UnsafeUtility.AsRef<int>((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + offset).ToPointer());
                offset += UnsafeUtility.SizeOf<int>() + size;
            }
            length = UnsafeUtility.AsRef<int>((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + offset).ToPointer());
            offset += UnsafeUtility.SizeOf<int>();
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static int GetConfigLayout(UIGraphData graph, out NativeArray<OffsetInfo> configLayout, Allocator allocator) {
            var offset = sizeof(ulong) + sizeof(int);
            int subMeshCount = 0;
            configLayout = new NativeArray<OffsetInfo>(graph.GetNodeCount(), allocator);
            for (int currentIndex = 0; currentIndex < graph.GetNodeCount(); currentIndex++) {
                //var size = UnsafeUtility.AsRef<int>((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + offset).ToPointer());
                var size = *(int*)(graph.value + offset);
                var header = (HeaderConfig*)(graph.value + offset + sizeof(int));
                //var size = graph.GetNodeLength(currentIndex);
                if (header->IsDedicatedNode) {
                    subMeshCount++;
                }
                offset += UnsafeUtility.SizeOf<int>();
                configLayout[currentIndex] = new OffsetInfo(offset, size, header->configurationMask);
                offset += size;
            }
            return subMeshCount;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Constrain(float2 widthConstraints, float2 heightConstraints, float2 size) {
            return new float2(float.IsPositiveInfinity(widthConstraints.y) ? math.max(size.x, widthConstraints.x) : math.clamp(size.x, widthConstraints.x, widthConstraints.y), float.IsPositiveInfinity(heightConstraints.y) ? math.max(size.y, heightConstraints.x) : math.clamp(size.y, heightConstraints.x, heightConstraints.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void AdjustPosition(float2 size, float4 constraints, BoxLayoutConfig* boxConfig, UIPassState* selfPtr, IntPtr statePtr, int stateChildCount, void* stateChildren, int4 multiplier) {

            var outer = new float2(math.max(size.x, float.IsPositiveInfinity(constraints.y) ? constraints.x : constraints.y), math.max(size.y, float.IsPositiveInfinity(constraints.w) ? constraints.z : constraints.w));
            var alignment = boxConfig->horizontalAlign.As2D(boxConfig->verticalAlign);
            float2 minorAdjustment;
            float2 majorAdjustment = alignment.GetOffset(size, outer, Alignment.BottomLeft);
            for (int i = 0; i < stateChildCount; i++) {
                var childState = (UIPassState*)(statePtr + (UnsafeUtility.SizeOf<UIPassState>() * (UnsafeUtility.ReadArrayElement<int>(stateChildren, i)))).ToPointer();
                minorAdjustment = alignment.GetOffset(childState->size, outer, Alignment.BottomLeft);
                childState->localBox.x += minorAdjustment.x * multiplier.z + majorAdjustment.x * multiplier.x;
                childState->localBox.y += minorAdjustment.y * multiplier.w + majorAdjustment.y * multiplier.y;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 ConstrainOuterBox(float2 widthConstraints, float2 heightConstraints, float2 size) {
            return new float2(float.IsPositiveInfinity(widthConstraints.y) ? size.x : math.max(size.x, widthConstraints.y), float.IsPositiveInfinity(widthConstraints.y) ? size.y : math.max(size.y, heightConstraints.y));
        }
    }


    public enum VisibilityStyle : byte {
        Hidden = 0, Visible = 1
    }

    public struct CompiledUISchema {

        public BlobArray<UISchema.CompiledElement> elements;
        public BlobArray<UISchema.InheritableField> inheritableFields;

    }


}