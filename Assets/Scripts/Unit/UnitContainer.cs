using UnityEngine;
using System.Collections.Generic;
namespace Reactics.Battle.Unit
{
    public class UnitContainer : MonoBehaviour
    {
        [SerializeField]
        private Unit unit;

        public int HealthPoints { get; set; }

        public int MagicPoints { get; set; }

        public int ActionPoints { get; set; }

        private bool IsChargingActionPoints { get; set; }

        public readonly HashSet<object> ChargeBlocks = new HashSet<object>();
        public void Initialize()
        {
            HealthPoints = unit.Stats.MaxHealthPoints;
            MagicPoints = unit.Stats.MaxMagicPoints;

        }

        private void Update() {
            if (IsChargingActionPoints) {

            }
        }

        public void ChargeActionMeter() {
            //TODO: Calculate action meter charg
        }
    }
}
