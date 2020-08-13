using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {
    public struct UIFont : ISharedComponentData, IEquatable<UIFont> {
        public TMP_FontAsset value;
        public UILength size;

        public override bool Equals(object obj) {
            if (obj is UIFont font) {
                return Equals(font);
            }
            return false;
        }

        public bool Equals(UIFont other) {
            return EqualityComparer<TMP_FontAsset>.Default.Equals(value, other.value) &&
                   size.Equals(other.size);
        }

        public override int GetHashCode() {
            int hashCode = -986392906;
            hashCode = hashCode * -1521134295 + EqualityComparer<TMP_FontAsset>.Default.GetHashCode(value);
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            return hashCode;
        }
    }

}