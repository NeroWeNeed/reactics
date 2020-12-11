using Unity.Entities;
using UnityEngine;

namespace NeroWeNeed.Commons {

    public struct MeshIndexUpdateData16 : IBufferElementData {
        public ushort Value;
    }
    public struct MeshIndexUpdateData32 : IBufferElementData {
        public uint Value;
    }
    public struct MeshIndexUpdate : IComponentData {
        public MeshTopology topology;

        public bool calculateBounds;

        public int baseVertex;
        public MeshIndexUpdateMode mode;
    }
    public enum MeshIndexUpdateMode {
        Set, Append, Prepend, Clear
    }
}