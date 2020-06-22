using UnityEngine;
namespace Reactics.Battle
{


    [CreateAssetMenu(fileName = "UnitInfoAsset", menuName = "Reactics/UnitInfoAsset", order = 0)]
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