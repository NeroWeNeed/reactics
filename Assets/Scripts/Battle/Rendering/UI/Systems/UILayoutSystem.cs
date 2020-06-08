using System;
using Reactics.UI;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reactics.UI
{
    [UpdateInGroup(typeof(UILayoutSystemGroup))]

    public class UILayoutSystem : SystemBase
    {

        private EntityQuery query;
        protected override void OnCreate()
        {
            query = GetEntityQuery(ComponentType.ReadOnly<UIElementLayout>(), ComponentType.ReadOnly<UIElement>(), typeof(UIElementBounds));
            query.SetChangedVersionFilter(ComponentType.ReadOnly<UIElement>());
        }
        protected override void OnUpdate()
        {

            if (query.CalculateEntityCount() > 0)
            {
                NativeMultiHashMap<Entity, Entity> dependencies = new NativeMultiHashMap<Entity, Entity>(8, Allocator.Temp);
                NativeHashMap<Entity, UIElementBounds> oldBounds = new NativeHashMap<Entity, UIElementBounds>(8 + 1, Allocator.Temp);
                NativeHashMap<Entity, UIElementBounds> newBounds = new NativeHashMap<Entity, UIElementBounds>(8 + 1, Allocator.Temp);
                oldBounds.Add(Entity.Null, new UIElementBounds(0, 0, float.PositiveInfinity, float.PositiveInfinity));
                newBounds.Add(Entity.Null, new UIElementBounds(0, 0, float.PositiveInfinity, float.PositiveInfinity));


                Entities.WithReadOnly(typeof(UIElementLayout)).ForEach((Entity entity, in UIElement element, in UIElementBounds bounds) =>
                {
                    dependencies.Add(element.parent, entity);
                    oldBounds.Add(entity, bounds);
                    newBounds.Add(entity, UIElementBounds.Null);

                }).WithStoreEntityQueryInField(ref query).Run();
                var enumerator = dependencies.GetValuesForKey(Entity.Null);
                ValueInfo info = new ValueInfo();
                while (enumerator.MoveNext())
                {
                    EntityManager.GetSharedComponentData<UIElementLayout>(enumerator.Current).Invoke(EntityManager, enumerator.Current, GetComponent<UIElement>(enumerator.Current).parent, dependencies, newBounds, info);


                }
                enumerator.Dispose();


                oldBounds.Remove(Entity.Null);
                newBounds.Remove(Entity.Null);


                var keys = oldBounds.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < keys.Length; i++)
                {
                    var oldBound = oldBounds[keys[i]];
                    var newBound = newBounds[keys[i]];
                    if (!oldBound.Equals(newBound))
                    {

                        SetComponent(keys[i], newBound);
                    }
                }


            }

        }



    }
}