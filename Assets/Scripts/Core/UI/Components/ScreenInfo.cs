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
    [GenerateAuthoringComponent]
    public struct ScreenInfo : IComponentData {
        public int2 screen;
        public int2 resolution;
        public float dpi;
        public ScreenOrientation orientation;
    }

}