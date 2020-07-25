using System;
using Reactics.Battle.Map;
using Unity.Entities;

namespace Reactics.Battle
{
    public enum TargetType
    {
        Unknown, Point, Body, Direction, PointGroup, BodyGroup, DirectionGroup
    }
    public static class TargetTypeUtility
    {
        public static TargetType GetType(Type type)
        {
            if (type == null)
            {

                return TargetType.Unknown;
            }
            if (type.Equals(typeof(Point)))
            {
                return TargetType.Point;
            }
            else if (type.Equals(typeof(MapBodyTarget)))
            {
                return TargetType.Body;
            }
            else if (type.Equals(typeof(MapBodyDirection)))
            {
                return TargetType.Direction;
            }
            else
            {
                return TargetType.Unknown;
            }
        }
    }


    public struct MapBodyTarget
    {
        public Entity entity;

        public MapBody mapBody;
    }
}