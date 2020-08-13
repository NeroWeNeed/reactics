using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [WriteGroup(typeof(LocalToWorld))]
    public struct MapBodyPathFindingRoute : IBufferElementData {
        public Point next;
        public Point previous;
        public float speed;
        public float completion;

        public MapBodyPathFindingRoute(Point next, Point previous, float speed) {
            this.next = next;
            this.previous = previous;
            this.speed = speed;
            this.completion = 0;
        }
    }



}