namespace Reactics.Battle
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Ability", menuName = "Reactics/Ability", order = 0)]
    public class Ability : ScriptableObject
    {
        [SerializeField]
        private new string name;

        [SerializeField]
        private string description;

    }

}