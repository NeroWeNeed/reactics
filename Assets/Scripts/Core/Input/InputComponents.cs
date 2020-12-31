using System;
using System.Collections.Generic;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reactics.Core.Input {
    public struct InputDirty : ISharedComponentData, IEquatable<InputDirty> {
        public bool value;

        public bool Equals(InputDirty other) {
            return this.value == other.value;
        }

        public InputDirty(bool value) {
            this.value = value;
        }

        public override int GetHashCode() {
            return -1584136870 + value.GetHashCode();
        }
        public static implicit operator InputDirty(bool value) => new InputDirty(value);
    }
    public struct MenuInputData : IComponentData, IEquatable<MenuInputData> {
        public InputValue<float2> directionalNavigation, pointerNavigation;
        public InputValue<bool> select, back;
        public void Update(TimeData time) {
            directionalNavigation.duration += time.DeltaTime;
            pointerNavigation.duration += time.DeltaTime;
            select.duration += time.DeltaTime;
            back.duration += time.DeltaTime;
        }

        public MenuInputData(InputValue<float2> directionalNavigation, InputValue<float2> pointerNavigation, InputValue<bool> select, InputValue<bool> back) {
            this.directionalNavigation = directionalNavigation;
            this.pointerNavigation = pointerNavigation;
            this.select = select;
            this.back = back;
        }

        public override bool Equals(object obj) {
            return obj is MenuInputData data &&
                   directionalNavigation.Equals(data.directionalNavigation) &&
                   pointerNavigation.Equals(data.pointerNavigation) &&
                   select.Equals(data.select) &&
                   back.Equals(data.back);
        }
        public bool Equals(MenuInputData data) {
            return directionalNavigation.Equals(data.directionalNavigation) &&
                   pointerNavigation.Equals(data.pointerNavigation) &&
                   select.Equals(data.select) &&
                   back.Equals(data.back);
        }

        public override int GetHashCode() {
            int hashCode = 1387260840;
            hashCode = hashCode * -1521134295 + directionalNavigation.GetHashCode();
            hashCode = hashCode * -1521134295 + pointerNavigation.GetHashCode();
            hashCode = hashCode * -1521134295 + select.GetHashCode();
            hashCode = hashCode * -1521134295 + back.GetHashCode();
            return hashCode;
        }
    }
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