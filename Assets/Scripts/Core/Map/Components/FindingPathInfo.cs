using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.Map {


    public struct FindingPathInfo : IComponentData {
        public Point destination;

        public float speed;

        public int maxElevationDifference;

        public BlittableBool routeClosest;

        public int currentlyTraveled;

        public int maxTravel;
    }



}