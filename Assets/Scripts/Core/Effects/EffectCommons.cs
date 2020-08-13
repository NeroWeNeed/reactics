using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Entities;
namespace Reactics.Core.Effects {
    public enum TargetType {
        Unknown, Point, Body, Direction, PointGroup, BodyGroup, DirectionGroup
    }
    public static class TargetTypeUtility {
        public static TargetType GetType(Type type) {
            if (type == null) {

                return TargetType.Unknown;
            }
            if (type.Equals(typeof(Point))) {
                return TargetType.Point;
            }
            else if (type.Equals(typeof(MapBodyTarget))) {
                return TargetType.Body;
            }
            else if (type.Equals(typeof(MapBodyDirection))) {
                return TargetType.Direction;
            }
            else {
                return TargetType.Unknown;
            }
        }
    }


}