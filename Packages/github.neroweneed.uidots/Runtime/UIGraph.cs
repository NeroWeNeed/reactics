using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Entities;

namespace NeroWeNeed.UIDots {
    public struct UIGraphNode {
        public BlobString name;
        public FunctionPointer<UIPass> pass;
        public FunctionPointer<UIRenderBoxHandler> renderBoxHandler;
        public BlobArray<int> children;
    }
    public struct UIGraph {
        public BlobArray<UIGraphNode> nodes;
    }

}