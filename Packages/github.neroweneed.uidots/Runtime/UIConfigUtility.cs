using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace NeroWeNeed.UIDots {
    public static class UIConfigUtility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool HasName(ulong mask, IntPtr source) {
            return TryGetConfig(mask, UIConfigLayoutTable.NameConfig, source, out IntPtr configBlock) && ((NameConfig*)configBlock.ToPointer())->name.IsCreated;
        }
        public unsafe static string GetName(ulong mask, IntPtr source) {
            var configBlock = GetConfig(mask, UIConfigLayoutTable.NameConfig, source);
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
                offset += (int)((byte)(mask >> i) & 1u) * UIConfigLayoutTable.Lengths[i];
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

            for (int i = 0; i < UIConfigTypeTable.Types.Length; i++) {
                if (((byte)(mask >> i) & 1U) != 0) {
                    types.Add(UIConfigTypeTable.Types[i]);
                }
            }
        }
        [BurstDiscard]
        public unsafe static void CreateConfiguration(ulong mask, List<object> configs) {
            configs.Clear();
            int size = 0;
            for (int i = 0; i < UIConfigTypeTable.Types.Length; i++) {
                if (((byte)(mask >> i) & 1U) != 0) {
                    var config = Activator.CreateInstance(UIConfigTypeTable.Types[i]);
                    configs.Add(config);
                    size += UIConfigLayoutTable.Lengths[i];
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLength(ulong mask) {
            int offset = 0;
            for (int i = 0; i < UIConfigLayoutTable.Lengths.Length; i++) {
                offset += (int)((byte)(mask >> i) & 1U) * UIConfigLayoutTable.Lengths[i];
            }
            return offset;
        }
    }
}