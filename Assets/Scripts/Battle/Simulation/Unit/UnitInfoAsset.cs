using UnityEngine;
namespace Reactics.Battle
{



    public class UnitInfoAsset : ScriptableObject
    {
        [SerializeField]
        private new string name;

        public string Name { get => name; }
        [SerializeField]
        public string biography;

        public string Biography { get => biography; }

    }
}