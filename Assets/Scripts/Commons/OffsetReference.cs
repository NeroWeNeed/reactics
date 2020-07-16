using System;
using Unity.Collections;

namespace Reactics.Commons {
    [Serializable]
    public struct OffsetReference {
        public uint sourceOffset;

        public uint sourceLength;

        public uint targetOffset;

        public uint targetLength;
    }


}