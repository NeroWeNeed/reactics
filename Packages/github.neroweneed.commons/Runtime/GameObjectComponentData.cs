using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NeroWeNeed.Commons {
    public struct GameObjectComponentData<TComponent> : ISharedComponentData, IEquatable<GameObjectComponentData<TComponent>> where TComponent : MonoBehaviour {
        public GameObject value;
        public TComponent Component { get => value.GetComponent<TComponent>(); }

        public GameObjectComponentData(GameObject value) {
            this.value = value;
        }

        public bool Equals(GameObjectComponentData<TComponent> other) {
            return  (value?.GetInstanceID() ?? 0) == (other.value?.GetInstanceID() ?? 0);
        }

        public override int GetHashCode() {
            int hashCode = -865696550;
            hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(value);
            hashCode = hashCode * -1521134295 + EqualityComparer<TComponent>.Default.GetHashCode(Component);
            return hashCode;
        }
    }

}