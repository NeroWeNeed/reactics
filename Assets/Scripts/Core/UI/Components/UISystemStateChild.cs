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
    public struct UISystemStateChild : ISystemStateBufferElementData, IEquatable<UIChild>, IEquatable<UISystemStateChild> {
        public Entity value;
        public bool Equals(UISystemStateChild other) {
            return value == other.value;
        }

        public bool Equals(UIChild other) {
            return value == other.value;
        }
        public static implicit operator UISystemStateChild(Entity entity) => new UISystemStateChild { value = entity };
        public static implicit operator UISystemStateChild(UIChild child) => new UISystemStateChild { value = child.value };
        public static implicit operator Entity(UISystemStateChild child) => child.value;
    }

}