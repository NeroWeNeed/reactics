using UnityEngine;


namespace Reactics.Core.Map {
    public class MapTileEffectInfoAsset : ScriptableObject {
        [SerializeField]
        private new string name;

        [SerializeField]
        private string description;

        public string Name { get => name; }

        public string Description { get => description; }
    }
}