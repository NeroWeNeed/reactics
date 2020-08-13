using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {
    public struct HealthPointData : IComponentData {
        public ushort Max;
        public ushort Current;
        public HealthPointData(ushort health) {
            this.Max = health;
            this.Current = health;
        }
        public HealthPointData(ushort max, ushort current) {
            Max = max;
            Current = current > max ? max : current;
        }
        public HealthPointData(UnitAsset unit) {
            this.Max = unit.HealthPoints;
            this.Current = Max;
        }
    }

}