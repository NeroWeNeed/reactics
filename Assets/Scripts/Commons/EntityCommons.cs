using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Commons
{
    public static class EntityCommons
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponent<T>(this EntityManager manager, Entity entity, out T data) where T : struct, IComponentData
        {
            if (manager.HasComponent<T>(entity))
            {
                data = manager.GetComponentData<T>(entity);
                return true;
            }
            else
            {
                data = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSharedComponent<T>(this EntityManager manager, Entity entity, out T data) where T : struct, ISharedComponentData
        {
            if (manager.HasComponent<T>(entity))
            {
                data = manager.GetSharedComponentData<T>(entity);
                return true;
            }
            else
            {
                data = default;
                return false;
            }
        }

        public static void AppendArchetype(this EntityManager manager, Entity entity, EntityArchetype archetype)
        {
            manager.AddComponents(entity, archetype.ToComponentTypes());
        }
        public static Entity CreateEntityAndAppend(this EntityManager manager, EntityArchetype archetype)
        {
            var entity = manager.CreateEntity();
            manager.AddComponents(entity,archetype.ToComponentTypes());
            return entity;
        }
        public static void CreateEntityAndAppend(this EntityManager manager, EntityArchetype archetype, NativeArray<Entity> entities)
        {
            
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = manager.CreateEntityAndAppend(archetype);
            }
            
        }

        public static ComponentTypes ToComponentTypes(this EntityArchetype archetype)
        {
            var components = archetype.GetComponentTypes();
            var result = new ComponentTypes(components.ToArray());
            components.Dispose();
            return result;
        }

        /*         [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static NativeArray<V> ToNativeArray<K, V>(this NativeMultiHashMap<K, V>.Enumerator enumerator, Allocator allocator)
                where K : struct, IEquatable<K>
                where V : struct
                {
                    NativeList<V> result = new NativeList<V>(allocator);
                    foreach (var r in enumerator)
                        result.Add(r);
                    return result;
                } */

    }

}