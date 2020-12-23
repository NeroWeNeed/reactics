using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    public struct InputHandlerStateData : IComponentData {
        public InputHandlerState value;
    }
    [Flags]
    public enum InputHandlerState : byte {
        Clean = 0, ActionMapDirty = 1, ControlSchemeDirty = 2, Dirty = ActionMapDirty | ControlSchemeDirty
    }
    public struct PlayerIndexData : IComponentData {
        public int value;
    }
    public struct InputHandlerData : ISharedComponentData, IEquatable<InputHandlerData> {
        public GameObject value;
        public PlayerInput PlayerInput { get => value.GetComponent<PlayerInput>(); }

        public bool Equals(InputHandlerData other) {
            return value == other.value;
        }

        public override int GetHashCode() {
            return -1584136870 + EqualityComparer<GameObject>.Default.GetHashCode(value);
        }
    }
    public struct InputContext : ISharedComponentData, IEquatable<InputContext> {
        public string actionMapName;
        public string controlSchemeName;
        public InputContext(InputActionMap inputActions, InputControlScheme controlScheme) {
            this.actionMapName = inputActions.name;
            this.controlSchemeName = controlScheme.name;
        }
        public InputContext(string actionMapName, string controlSchemeName) {
            this.actionMapName = actionMapName;
            this.controlSchemeName = controlSchemeName;
        }

        public bool Equals(InputContext other) {
            return actionMapName == other.actionMapName && controlSchemeName == other.controlSchemeName;
        }

        public override int GetHashCode() {
            int hashCode = 2076751591;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(actionMapName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(controlSchemeName);
            return hashCode;
        }
    }
    public struct CameraDependent : IComponentData {
        public Entity cameraEntity;
    }

}