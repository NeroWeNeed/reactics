using System;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace NeroWeNeed.Commons {

    [MayOnlyLiveInBlobStorage]
    public struct BlobResourceAddress {
        internal BlobArray<char> Data;
        public int Length => Data.Length;

        public new unsafe string ToString() {
            return new string((char*)Data.GetUnsafePtr(), 0, Data.Length);
        }
    }
    public static class BlobResourceAddressExtension {
        public unsafe static void AllocateAddress(this BlobBuilder builder, ref BlobResourceAddress resourceAddress, string address) {
            var arr = builder.Allocate<char>(ref resourceAddress.Data, address.Length);
            var bytes = Encoding.Unicode.GetBytes(address);
            Marshal.Copy(bytes, 0, (IntPtr)arr.GetUnsafePtr(), bytes.Length);
        }
    }
}