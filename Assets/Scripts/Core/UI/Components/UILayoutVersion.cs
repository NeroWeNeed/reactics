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
    public struct UILayoutVersion : IComponentData, IVersion {
        private int version;
        public int Version { get => version; set => version = value; }
    }

}