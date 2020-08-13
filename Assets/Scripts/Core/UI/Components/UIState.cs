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

    public struct UIState : IComponentData {
        public State value;
    }
    [Flags]
    public enum State {
        None = 0, Hover = 1, Pressed = 2
    }
}