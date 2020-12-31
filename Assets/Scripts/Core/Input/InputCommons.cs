using System;
using System.Collections.Generic;
using Unity.Core;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Interactions;

namespace Reactics.Core.Input {
    public struct InputValue<TValue> : IEquatable<InputValue<TValue>> where TValue : struct {
        public TValue value;
        public double  duration;
        public InputValue(InputAction.CallbackContext context) {
            duration = context.duration;
            value = context.ReadValue<TValue>();
        }
        public InputValue(TValue value, double duration = 0.0) {
            this.value = value;
            this.duration = duration;
        }
        //public static implicit operator InputValue<TValue>(InputAction.CallbackContext context) => new InputValue<TValue>(context);
        public static InputValue<float2> FromContextVector2(InputAction.CallbackContext context) => new InputValue<float2>(context.ReadValue<Vector2>());
        public static InputValue<float3> FromContextVector3(InputAction.CallbackContext context) => new InputValue<float3>(context.ReadValue<Vector3>());
        public static InputValue<bool> FromButton(InputAction.CallbackContext context) => new InputValue<bool>(context.performed);

        public override bool Equals(object obj) {
            return obj is InputValue<TValue> other &&
                   this.value.Equals(other.value) &&
                   duration == other.duration;
        }

        public override int GetHashCode() {
            int hashCode = -577728587;
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            hashCode = hashCode * -1521134295 + duration.GetHashCode();
            return hashCode;
        }

        public bool Equals(InputValue<TValue> other) {
            return this.value.Equals(other.value) &&
                    duration == other.duration;
        }
    }
}