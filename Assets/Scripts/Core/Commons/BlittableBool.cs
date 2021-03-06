using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Reactics.Core.Commons {
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlittableBool : IEquatable<BlittableBool> {
        [SerializeField]
        private byte boolValue;
        public BlittableBool(bool value) {
            boolValue = (byte)(value ? 1 : 0);
        }

        public static implicit operator bool(BlittableBool value) {
            return value.boolValue == 1;
        }

        public static implicit operator BlittableBool(bool value) {
            return new BlittableBool(value);
        }

        public override string ToString() {
            if (boolValue == 1)
                return "true";
            return "false";
        }

        public bool Equals(BlittableBool other) {
            return boolValue == other.boolValue;
        }
    }
}