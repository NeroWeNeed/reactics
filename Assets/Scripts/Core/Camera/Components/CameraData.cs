using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Camera {
    public struct CameraData : ISharedComponentData, IEquatable<CameraData> {
        public UnityEngine.Camera camera;
        public string tag;

        public override bool Equals(object obj) {
            if (obj is CameraData data) {
                return Equals(data);
            }
            return false;
        }

        public bool Equals(CameraData other) {
            return EqualityComparer<UnityEngine.Camera>.Default.Equals(camera, other.camera) &&
                   tag == other.tag;
        }

        public override int GetHashCode() {
            int hashCode = 586511377;
            hashCode = hashCode * -1521134295 + EqualityComparer<UnityEngine.Camera>.Default.GetHashCode(camera);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(tag);
            return hashCode;
        }
    }
}