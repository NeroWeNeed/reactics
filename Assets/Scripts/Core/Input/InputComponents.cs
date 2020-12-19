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
    public struct InputActionMapData : ISharedComponentData, IEquatable<InputActionMapData> {
        public string name;
        public InputActionMapData(string id) {

            this.name = id;
        }

        public bool Equals(InputActionMapData other) {
            return EqualityComparer<string>.Default.Equals(name, other.name);
        }

        public override int GetHashCode() {
            return -1721492029 + EqualityComparer<string>.Default.GetHashCode(name);
        }
    }
    public struct InputControlSchemeData : ISharedComponentData, IEquatable<InputControlSchemeData> {
        public string name;

        public bool Equals(InputControlSchemeData other) {
            return EqualityComparer<string>.Default.Equals(name, other.name);
        }

        public override int GetHashCode() {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }
        public static implicit operator InputControlSchemeData(string name) => new InputControlSchemeData { name = name };
    }
    public struct CameraDependent : IComponentData {
        public Entity cameraEntity;
    }

}