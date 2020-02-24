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

    }


}