using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Effects {
    public enum TargetType {
        Unknown, Point, Body, Direction, PointGroup, BodyGroup, DirectionGroup
    }
    public static class TargetTypeUtility {

        public static Type GetAssetType(Type type) {
            if (type == null) {
                return null;
            }
            if (type.Equals(typeof(PointTarget))) {
                return typeof(PointEffectAsset);
            }
            else if (type.Equals(typeof(MapBodyTarget))) {
                return typeof(MapBodyEffectAsset);
            }
            else if (type.Equals(typeof(DirectionTarget))) {
                return typeof(DirectionEffectAsset);
            }
            else {
                return null;
            }
        }
        public static TargetType GetType(Type type) {
            if (type == null) {

                return TargetType.Unknown;
            }
            if (type.Equals(typeof(PointTarget))) {
                return TargetType.Point;
            }
            else if (type.Equals(typeof(MapBodyTarget))) {
                return TargetType.Body;
            }
            else if (type.Equals(typeof(DirectionTarget))) {
                return TargetType.Direction;
            }
            else {
                return TargetType.Unknown;
            }
        }
        public static int GetTypeIndex(Type type) {
            var targetType = GetType(type);
            return Array.IndexOf(Enum.GetValues(typeof(TargetType)), targetType);
        }
    }

}