using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Unit {

    public struct ActionMeterData : IComponentData {
        public const float DEFAULT_MAX = 1f;
        public float Max;
        public float Current;
        public bool Active { get => Current >= Max; }
        public static ActionMeterData Create(float max = DEFAULT_MAX) => new ActionMeterData
        {
            Max = max,
            Current = max
        };
        public static ActionMeterData Create(float max, float initial) => new ActionMeterData
        {
            Max = max,
            Current = initial
        };
    }

}