using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {
    public struct ActionList : ISharedComponentData, IEquatable<ActionList> {
        public List<ActionAsset> value;

        public override bool Equals(object obj) {
            return obj is ActionList list &&
                   EqualityComparer<List<ActionAsset>>.Default.Equals(value, list.value);
        }

        public bool Equals(ActionList other) {
            return EqualityComparer<List<ActionAsset>>.Default.Equals(value, other.value);
        }

        public override int GetHashCode() {
            return -1584136870 + EqualityComparer<List<ActionAsset>>.Default.GetHashCode(value);
        }
    }

}