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


    public struct UIChild : IBufferElementData, IEquatable<UIChild>, IEquatable<UISystemStateChild> {
        public Entity value;

        public bool Equals(UISystemStateChild other) {
            return value == other.value;
        }

        public bool Equals(UIChild other) {
            return value == other.value;
        }
        public static implicit operator UIChild(Entity entity) => new UIChild { value = entity };
        public static implicit operator UIChild(UISystemStateChild child) => new UIChild { value = child.value };
        public static implicit operator Entity(UIChild child) => child.value;
    }

}