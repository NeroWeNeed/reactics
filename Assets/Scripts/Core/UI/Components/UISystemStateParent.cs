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
    public struct UISystemStateParent : ISystemStateComponentData, IEquatable<UIParent>, IEquatable<UISystemStateParent> {
        public Entity value;
        public bool Equals(UISystemStateParent other) {
            return value == other.value;
        }

        public bool Equals(UIParent other) {
            return value == other.value;
        }
        public static implicit operator UISystemStateParent(Entity entity) => new UISystemStateParent { value = entity };
        public static implicit operator UISystemStateParent(UIParent child) => new UISystemStateParent { value = child.value };
        public static implicit operator Entity(UISystemStateParent parent) => parent.value;
    }

}