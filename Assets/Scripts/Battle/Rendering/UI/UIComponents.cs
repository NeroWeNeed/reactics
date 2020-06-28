using Reactics.Commons;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.UI
{

    public struct UIFixedSize : IComponentData
    {
        public UILength width, height;
    }
    public struct UIToLocal : IComponentData
    {
        public float4 value;
        public int zOrder;
    }
    public struct UILayoutInfo : IComponentData
    {
        public float4 bounds;
        public float fontSize;
        

        /// <summary>
        /// Represents the depth of the UI Box relative to other elements.
        /// </summary>
        public Entity parent;
        public BlittableGuid layout;
    }

}