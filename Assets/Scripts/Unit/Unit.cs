using UnityEngine;
namespace Reactics.Battle.Unit
{


    [CreateAssetMenu(fileName = "Unit", menuName = "Reactics/Unit", order = 0)]
    public class Unit : ScriptableObject
    {

        [SerializeField]
        private new string name;


        
        [SerializeField]
        private IStats stats;

        public IStats Stats => stats;

        [SerializeField]
        private IMagicSkill magicSkill;

        public IMagicSkill MagicSkill => magicSkill;
    }
}