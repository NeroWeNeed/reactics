using System;
using System.Collections.Generic;
using Reactics.Util;
using Unity.Entities;
namespace Reactics.Battle
{

    public struct UnitInfo : ISharedComponentData, IEquatable<UnitInfo>
    {
        public Unit unit;

        public bool Equals(UnitInfo other)
        {
            return unit == other.unit;
        }

        public override int GetHashCode()
        {
            return -1913094625 + EqualityComparer<Unit>.Default.GetHashCode(unit);
        }
    }
    public struct ActionMeter : IComponentData
    {
        public const float MAX_ACTION_POINTS = 100f;
        public float charge;
        public float rechargeRate;
        public BlittableBool chargeable;
        public bool Active() => charge >= MAX_ACTION_POINTS;
    }

    public struct UnitCommand : IComponentData {

    }


}