using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.UI
{

    public interface IUILayout
    {
        float2 DoLayout(EntityManager entityManager, Entity self, UILayoutSystem layoutSystem, NativeHashMap<Entity, float2> sizes);
    }

    public struct FixedSizeLayout : IUILayout
    {


        public float2 DoLayout(EntityManager entityManager, Entity self, UILayoutSystem layoutSystem, NativeHashMap<Entity, float2> sizes)
        {
            var size = entityManager.GetComponentData<UIFixedSize>(self);
            var layout = entityManager.GetComponentData<UILayoutInfo>(self);
            
            throw new NotImplementedException();
        }
    }
}