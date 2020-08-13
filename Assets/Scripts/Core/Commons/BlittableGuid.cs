using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Reactics.Core.Commons {
    [Serializable]
    public unsafe struct BlittableGuid : IEquatable<BlittableGuid> {
        [SerializeField]
        private fixed byte value[16];
        public BlittableGuid(Guid guid) {
            var bytes = guid.ToByteArray();
            fixed (byte* dstPtr = value, srcPtr = bytes) {
                UnsafeUtility.MemCpy(dstPtr, srcPtr, UnsafeUtility.SizeOf<byte>() * 16);
            }
        }
        public BlittableGuid(byte[] bytes) {
            if (bytes.Length != 16)
                throw new ArgumentException("Invalid Guid");

            fixed (byte* dstPtr = value, srcPtr = bytes) {
                UnsafeUtility.MemCpy(dstPtr, srcPtr, UnsafeUtility.SizeOf<byte>() * 16);
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
                return UnsafeUtility.MemCmp(thisPtr, other.value, UnsafeUtility.SizeOf<byte>() * 16) == 0;
            }
        }

        public override int GetHashCode() {

            fixed (byte* thisPtr = value) {
                int r = -1584136870;
                for (byte i = 0; i < 16; i++) {
                    r += value[i];
                }
                return r;
            }
        }
        public static implicit operator Guid(BlittableGuid value) {
            var bytes = new byte[16];
            fixed (byte* bytePtr = bytes) {
                UnsafeUtility.MemCpy(bytePtr, value.value, UnsafeUtility.SizeOf<byte>() * 16);
            }
            return new Guid(bytes);
        }

        public static implicit operator BlittableGuid(Guid value) {
            return new BlittableGuid(value);
        }
    }
}