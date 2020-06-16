using System;
using System.Collections.Generic;
using Reactics.Battle.Unit;
using Reactics.Commons;
using Unity.Entities;
using Reactics.Battle.Map;
namespace Reactics.Battle
{

    public struct HealthPointData : IComponentData
    {
        public ushort Max;
        public ushort Current;
        public HealthPointData(ushort health)
        {
            this.Max = health;
            this.Current = health;
        }
        public HealthPointData(ushort max, ushort current)
        {
            Max = max;
            Current = current > max ? max : current;
        }
        public HealthPointData(UnitAsset unit)
        {
            this.Max = unit.HealthPoints;
            this.Current = Max;
        }
    }
    public struct MagicPointData : IComponentData
    {
        public ushort Max;
        public ushort Current;
        public MagicPointData(ushort magic)
        {
            this.Max = magic;
            this.Current = magic;
        }
        public MagicPointData(ushort max, ushort current)
        {
            Max = max;
            Current = current > max ? max : current;
        }
        public MagicPointData(UnitAsset unit)
        {
            this.Max = unit.MagicPoints;
            this.Current = Max;
        }
    }

    public struct ActionMeterData : IComponentData
    {
        public const float DEFAULT_MAX = 1f;
        public float Max;
        public float Current;
        public bool Active { get => Current >= Max; }
        public static ActionMeterData Create(float max = DEFAULT_MAX) => new ActionMeterData
        {
            Max = max,
            Current = max
        };
        public static ActionMeterData Create(float max, float initial) => new ActionMeterData
        {
            Max = max,
            Current = initial
        };
    }
    /// <summary>
    /// Components for referencing Unit Information
    /// </summary>
    public struct UnitStats : IComponentData
    {
        public ushort Defense { get; set; }

        public ushort Resistance { get; set; }

        public ushort Strength { get; set; }

        public ushort Magic { get; set; }

        public ushort Speed { get; set; }

        public ushort Movement { get; set; }

        public UnitStats(UnitAsset unitAsset)
        {
            Defense = unitAsset.Defense;
            Resistance = unitAsset.Resistance;
            Strength = unitAsset.Strength;
            Magic = unitAsset.Magic;
            Speed = unitAsset.Speed;
            Movement = unitAsset.Movement;
        }
    }

    public struct UnitAssetReference : ISharedComponentData, IEquatable<UnitAssetReference>
    {
        public UnitAsset Value;

        public UnitAssetReference(UnitAsset value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is UnitAssetReference reference &&
                   EqualityComparer<UnitAsset>.Default.Equals(Value, reference.Value);
        }

        public bool Equals(UnitAssetReference other)
        {
            return EqualityComparer<UnitAsset>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<UnitAsset>.Default.GetHashCode(Value);
        }
    }
    public struct ActionList : ISharedComponentData, IEquatable<ActionList>
    {
        public List<ActionAsset> value;

        public override bool Equals(object obj)
        {
            return obj is ActionList list &&
                   EqualityComparer<List<ActionAsset>>.Default.Equals(value, list.value);
        }

        public bool Equals(ActionList other)
        {
            return EqualityComparer<List<ActionAsset>>.Default.Equals(value, other.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<List<ActionAsset>>.Default.GetHashCode(value);
        }
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