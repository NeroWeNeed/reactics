using System;
using System.Collections.Generic;
using Reactics.Util;
using Unity.Entities;
namespace Reactics.Battle
{
    /// <summary>
    /// Shared Component for referencing Unit Information
    /// </summary>
    public struct UnitData : IComponentData
    {
        public BlobAssetReference<UnitBlob> unit;
        public ushort healthPoints;
        public ushort maxHealthPoints;
        public ushort magicPoints;
        public ushort maxMagicPoints;
        public int defense;
        public int resistance;
        public int strength;
        public int magic;
        public int speed;
        public int movement;
        public ushort Defense() => defense < 0 ? (ushort)0 : (ushort)defense;
        public ushort Resistance() => resistance < 0 ? (ushort)0 : (ushort)resistance;
        public ushort Strength() => strength < 0 ? (ushort)0 : (ushort)strength;
        public ushort Magic() => magic < 0 ? (ushort)0 : (ushort)magic;
        public ushort Speed() => speed < 0 ? (ushort)0 : (ushort)speed;
        public ushort Movement() => movement < 0 ? (ushort)0 : (ushort)movement;
    }
    /// <summary>
    /// Handles Recharging Action Meters and determining if the unit can act.
    /// </summary>
    public struct ActionMeter : IComponentData
    {
        public const float MAX_ACTION_POINTS = 100f;
        public float charge;
        public float rechargeRate;
        public BlittableBool chargeable;
        public bool Active() => charge >= MAX_ACTION_POINTS;
    }

    public struct UnitCommand : IComponentData
    {
        public Entity unitManagerEntity;
    }

    /// <summary>
    /// Singleton to help out with unit selection and movement logic.
    /// </summary>
    public struct UnitManagerData : IComponentData
    {
        public bool commanding;
        //public ushort selectedUnitID;
        //public bool unitTargeted;
        //public Point actionTile;
        public Point moveTile;
        public bool moveTileSelected;
        public bool effectReady;
        public bool effectSelected;
        public Effect effect;
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