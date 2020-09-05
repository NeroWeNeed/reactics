using System;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [WriteGroup(typeof(LocalToWorld))]
    [Guid("cfaad2cd-647a-4a40-9788-62847a3c9bf5")]
    public struct MapBody : IComponentData {
        public Point point;
        public MapBodyDirection direction;
        public Anchor anchor;
    }



}