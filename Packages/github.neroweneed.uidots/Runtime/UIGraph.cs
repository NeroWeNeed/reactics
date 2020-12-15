using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Entities;

namespace NeroWeNeed.UIDots {
    public struct UIGraphNode {
        public FunctionPointer<UIPass> pass;
        public FunctionPointer<UIRenderBoxHandler> renderBoxHandler;
        public BlobArray<int> children;
    }
    public struct UIGraph {
        public BlittableAssetReference material;
        public BlobArray<UIGraphNode> nodes;
        public BlobArray<byte> initialConfiguration;
    }

}