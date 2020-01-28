namespace Reactics.Battle
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Proficiency", menuName = "Project ATT/Proficiency", order = 0)]
    public class Proficiency : ScriptableObject
    {
        public enum Level
        {
            F, E, D, C, B, A, S
        }
        [SerializeField]
        private new string name;

        public string Name { get => name; set => name = value; }


    }

}