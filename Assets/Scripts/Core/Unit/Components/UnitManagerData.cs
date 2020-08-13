using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {
    /// <summary>
    /// Singleton to help out with unit selection and movement logic.
    /// </summary>
    public struct UnitManagerData : IComponentData {
        public bool commanding;
        //public ushort selectedUnitID;
        //public bool unitTargeted;
        //public Point actionTile;
        public Point moveTile;
        public bool moveTileSelected;
        public bool effectReady;
        public bool effectSelected;
        public Reactics.Core.Effects.OldEffect effect;
        //public bool affectsTiles;
        //public bool affectsEnemies;
        //public bool affectsAllies;
        //public bool affectsSelf;
        //public ushort targetedUnitID;
        public Entity selectedUnit;
        public Entity targetedUnit;
        public ushort moveRange;

        //public ushort actionRange;
        //public bool commandIssued; //maybe not needed
    }

}