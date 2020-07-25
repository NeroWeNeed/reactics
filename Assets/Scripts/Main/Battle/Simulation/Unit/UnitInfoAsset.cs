using Reactics.Commons;
using UnityEngine;
namespace Reactics.Battle {
    public class UnitInfoAsset : ScriptableObject {
        [SerializeField]
        [Max(10)]
        private new string name;

        public string Name { get => name; }
        [SerializeField]
        [Multiline]
        public string biography;

        public string Biography { get => biography; }

    }
}