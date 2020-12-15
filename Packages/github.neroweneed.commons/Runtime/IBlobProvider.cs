using Unity.Entities;

namespace NeroWeNeed.Commons {
    public interface IBlobProvider {
        public object Create();
    }
    public interface IBlobProvider<TValue> : IBlobProvider where TValue : struct {
        new public BlobAssetReference<TValue> Create();
    }
}
