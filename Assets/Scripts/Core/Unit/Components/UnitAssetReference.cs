using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {

    public struct UnitAssetReference : ISharedComponentData, IEquatable<UnitAssetReference> {
        public UnitAsset Value;

        public UnitAssetReference(UnitAsset value) {
            Value = value;
        }

        public override bool Equals(object obj) {
            return obj is UnitAssetReference reference &&
                   EqualityComparer<UnitAsset>.Default.Equals(Value, reference.Value);
        }

        public bool Equals(UnitAssetReference other) {
            return EqualityComparer<UnitAsset>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode() {
            return -1937169414 + EqualityComparer<UnitAsset>.Default.GetHashCode(Value);
        }
    }

}