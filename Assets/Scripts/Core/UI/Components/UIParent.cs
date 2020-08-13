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
    public struct UIParent : IComponentData, IEquatable<UIParent>, IEquatable<UISystemStateParent> {
        public Entity value;

        public bool Equals(UISystemStateParent other) {
            return value == other.value;
        }

        public bool Equals(UIParent other) {
            return value == other.value;
        }

        public static implicit operator UIParent(Entity entity) => new UIParent { value = entity };
        public static implicit operator UIParent(UISystemStateParent child) => new UIParent { value = child.value };
        public static implicit operator Entity(UIParent parent) => parent.value;
    }

}