namespace Reactics.Battle.Map {
    using Reactics.Commons;
    using UnityEngine;

    [CreateAssetMenu(fileName = "MapTileEffectAsset", menuName = "Reactics/MapTileEffectAsset", order = 0)]
    public class MapTileEffectAsset : ScriptableObject {
        [SerializeField]
        [LocalizationTableName("MapTileEffectInfo")]
        public EmbeddedLocalizedAsset<MapTileEffectInfoAsset> info;
        [SerializeField]
        public AssetReference<EffectAsset> enterEffect;
        [SerializeField]
        public AssetReference<EffectAsset> exitEffect;
    }
}