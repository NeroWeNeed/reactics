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
    public struct UIMeshIndexData : IBufferElementData {
        public int value;

        public UIMeshIndexData(int value) {
            this.value = value;
        }
        public static implicit operator UIMeshIndexData(int value) => new UIMeshIndexData(value);
    }

}