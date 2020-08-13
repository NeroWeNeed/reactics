using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Camera {
    /*     public struct CameraData : ISharedComponentData, IEquatable<CameraData> {
            public UnityEngine.Camera camera;

            public override bool Equals(object obj) {
                if (obj is CameraData data) {
                    return Equals(data);
                }
                return false;
            }

            public bool Equals(CameraData other) {
                return EqualityComparer<UnityEngine.Camera>.Default.Equals(camera, other.camera);
            }

            public override int GetHashCode() {
                return -1929491842 + EqualityComparer<UnityEngine.Camera>.Default.GetHashCode(camera);
            }
        } */

    public struct CameraTag : ISharedComponentData, IEquatable<CameraTag> {
        public string Value;
        public override bool Equals(object obj) {
            if (obj is CameraTag data) {
                return Equals(data);
            }
            return false;
        }

        public bool Equals(CameraTag other) {
            return Value == other.Value;
        }

        public override int GetHashCode() {
            return -1573750901 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
        public static implicit operator string(CameraTag cameraTag) => cameraTag.Value;
    }
}