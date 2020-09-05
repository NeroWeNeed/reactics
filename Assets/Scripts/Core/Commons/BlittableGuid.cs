using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.Commons {
    [Serializable]
    public unsafe struct BlittableGuid : IEquatable<BlittableGuid>, IEquatable<Guid> {
        public const int SIZE = 16;
        [SerializeField]
        private fixed byte value[SIZE];
        public BlittableGuid(Guid guid) {
            var bytes = guid.ToByteArray();
            fixed (byte* dstPtr = value, srcPtr = bytes) {
                UnsafeUtility.MemCpy(dstPtr, srcPtr, UnsafeUtility.SizeOf<byte>() * SIZE);
            }
        }
        public BlittableGuid(byte[] bytes) {
            if (bytes.Length != SIZE)
                throw new ArgumentException("Invalid Guid");

            fixed (byte* dstPtr = value, srcPtr = bytes) {
                UnsafeUtility.MemCpy(dstPtr, srcPtr, UnsafeUtility.SizeOf<byte>() * SIZE);
            }
        }

        public override bool Equals(object obj) {
            if (obj is BlittableGuid guid) {
                return Equals(guid);
            }
            else
                return false;

        }

        public bool Equals(BlittableGuid other) {
            fixed (byte* thisPtr = value) {
                return UnsafeUtility.MemCmp(thisPtr, other.value, UnsafeUtility.SizeOf<byte>() * SIZE) == 0;
            }
        }

        public override int GetHashCode() {

            fixed (byte* thisPtr = value) {
                int r = -1584136870;
                for (byte i = 0; i < SIZE; i++) {
                    r += value[i];
                }
                return r;
            }
        }
        public static implicit operator Guid(BlittableGuid value) {
            var bytes = new byte[SIZE];
            fixed (byte* bytePtr = bytes) {
                UnsafeUtility.MemCpy(bytePtr, value.value, UnsafeUtility.SizeOf<byte>() * SIZE);
            }
            return new Guid(bytes);
        }

        public static implicit operator BlittableGuid(Guid value) {
            return new BlittableGuid(value);
        }
        public byte[] ToByteArray() {
            var bytes = new byte[SIZE];
            fixed (byte* bytePtr = bytes, myBytes = value) {
                UnsafeUtility.MemCpy(bytePtr, myBytes, UnsafeUtility.SizeOf<byte>() * SIZE);
            }
            return bytes;
        }
        public override string ToString() {
            fixed (byte* bytePtr = value) {

                return $"{Hex(value[3])}{Hex(value[2])}{Hex(value[1])}{Hex(value[0])}-{Hex(value[5])}{Hex(value[4])}-{Hex(value[7])}{Hex(value[6])}-{Hex(value[8])}{Hex(value[9])}-{Hex(value[10])}{Hex(value[11])}{Hex(value[12])}{Hex(value[13])}{Hex(value[14])}{Hex(value[15])}";

            }
        }
        private string Hex(byte b) {
            var l = b / 16;
            var r = b % 16;
            return $"{(char)((l < 10) ? 48 + l : 97 + l - 10)}{(char)((r < 10) ? 48 + r : 97 + r - 10)}";
        }

        public bool Equals(Guid other) {
            var oBytes = other.ToByteArray();
            fixed (byte* bytePtr = oBytes, myBytes = value) {
                return UnsafeUtility.MemCmp(bytePtr, myBytes, UnsafeUtility.SizeOf<byte>() * SIZE) == 0;
            }
        }
    }
}