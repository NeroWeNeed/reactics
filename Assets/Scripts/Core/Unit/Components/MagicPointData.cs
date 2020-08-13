using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {
    public struct MagicPointData : IComponentData {
        public ushort Max;
        public ushort Current;
        public MagicPointData(ushort magic) {
            this.Max = magic;
            this.Current = magic;
        }
        public MagicPointData(ushort max, ushort current) {
            Max = max;
            Current = current > max ? max : current;
        }
        public MagicPointData(UnitAsset unit) {
            this.Max = unit.MagicPoints;
            this.Current = Max;
        }
    }

}