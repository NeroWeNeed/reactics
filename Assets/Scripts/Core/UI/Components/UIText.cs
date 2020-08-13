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
    public struct UIText : IComponentData {
        public FixedString128 value;
        public UILength minWidth;
        public UILength maxWidth;
        public UIText(string value, UILength minWidth, UILength maxWidth) {
            this.value = value;
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
        }
        public UIText(string value) : this(value, UILength.Undefined, UILength.Undefined) { }
    }

}