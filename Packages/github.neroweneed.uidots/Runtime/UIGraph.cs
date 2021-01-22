using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Entities;

namespace NeroWeNeed.UIDots {
    public struct UIGraphNodeOld {
        
        public FunctionPointer<UILayoutPass> pass;
        public FunctionPointer<UIRenderBoxCounter> renderBoxHandler;
        public BlobArray<int> children;
        public ulong configurationMask;
    }
    public struct UIGraphOld {
        public BlobArray<UIGraphNodeOld> nodes;
        public BlobArray<byte> initialConfiguration;
    }

}