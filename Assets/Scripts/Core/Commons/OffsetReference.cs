using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Reactics.Core.Commons {
    [Serializable]
    public unsafe struct OffsetReference {
        public BlittableGuid guid;
        public int sourceOffset;
        public long sourceLength;
        public int targetOffset;
    }

    [Serializable]
    public struct VariableCollection {
        public OffsetReference[] references;

        public unsafe void SetVariables(IntPtr pointer, NativeHashMap<BlittableGuid, IntPtr> sources) {
            foreach (var reference in references) {
                if (sources.TryGetValue(reference.guid, out IntPtr src)) {
                    UnsafeUtility.MemCpy((pointer + reference.targetOffset).ToPointer(), (src + reference.sourceOffset).ToPointer(), reference.sourceLength);
                }
            }
        }
    }

}