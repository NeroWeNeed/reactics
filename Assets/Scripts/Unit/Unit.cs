using System.Collections.Generic;
using UnityEngine;
namespace Reactics.Battle
{


    [CreateAssetMenu(fileName = "Unit", menuName = "Reactics/Unit", order = 0)]
    public class Unit : ScriptableObject
    {

        [SerializeField]
        private readonly new string name;

        public string Name => name;


        [SerializeField]
        private readonly IStats stats;

        public IStats Stats => stats;

        [SerializeField]
        private readonly Dictionary<Proficiency, Proficiency.Level> proficiencies;

        public Proficiency.Level this[Proficiency proficiency]
        {
            
            get
            {
                return proficiencies.ContainsKey(proficiency) ? proficiencies[proficiency] : Proficiency.Level.F;
            }
            set
            {
                proficiencies[proficiency] = value;
            }
        }



    }
}