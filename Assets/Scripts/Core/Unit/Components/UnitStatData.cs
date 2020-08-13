using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {
    /// <summary>
    /// Components for referencing Unit Information
    /// </summary>
    public struct UnitStatData : IComponentData {
        public ushort Defense { get; set; }

        public ushort Resistance { get; set; }

        public ushort Strength { get; set; }

        public ushort Magic { get; set; }

        public ushort Speed { get; set; }

        public ushort Movement { get; set; }

        public UnitStatData(UnitAsset unitAsset) {
            Defense = unitAsset.Defense;
            Resistance = unitAsset.Resistance;
            Strength = unitAsset.Strength;
            Magic = unitAsset.Magic;
            Speed = unitAsset.Speed;
            Movement = unitAsset.Movement;
        }
    }

}