using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [WriteGroup(typeof(LocalToWorld))]
    public struct MapBody : IComponentData {
        public Point point;
        public MapBodyDirection direction;
        public Anchor3D anchor;
    }



}