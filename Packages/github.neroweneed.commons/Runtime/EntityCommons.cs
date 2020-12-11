using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace NeroWeNeed.Commons {
    public static class EntityCommons {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponent<T>(this EntityManager manager, Entity entity, out T data) where T : struct, IComponentData {
            if (manager.HasComponent<T>(entity)) {
                data = manager.GetComponentData<T>(entity);
                return true;
            }
            else {
                data = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSharedComponent<T>(this EntityManager manager, Entity entity, out T data) where T : struct, ISharedComponentData {
            if (manager.HasComponent<T>(entity)) {
                data = manager.GetSharedComponentData<T>(entity);
                return true;
            }
            else {
                data = default;
                return false;
            }
        }

        public static void AppendArchetype(this EntityManager manager, Entity entity, EntityArchetype archetype) {
            manager.AddComponents(entity, archetype.ToComponentTypes());
        }
        public static Entity CreateEntityAndAppend(this EntityManager manager, EntityArchetype archetype) {
            var entity = manager.CreateEntity();
            manager.AddComponents(entity, archetype.ToComponentTypes());
            return entity;
        }
        public static void CreateEntityAndAppend(this EntityManager manager, EntityArchetype archetype, NativeArray<Entity> entities) {

            for (int i = 0; i < entities.Length; i++) {
                entities[i] = manager.CreateEntityAndAppend(archetype);
            }

        }

        public static ComponentTypes ToComponentTypes(this EntityArchetype archetype) {
            var components = archetype.GetComponentTypes();
            var result = new ComponentTypes(components.ToArray());
            components.Dispose();
            return result;
        }
        public static void SymmetricDifference<TElement1, TElement2>(NativeArray<TElement1> array1, NativeArray<TElement2> array2, out NativeArray<TElement1> onlyInArray1, out NativeArray<TElement2> onlyInArray2, Allocator allocator = Allocator.Temp) where TElement1 : struct, IEquatable<TElement2> where TElement2 : struct, IEquatable<TElement1> {
            var onlyInArray1List = new NativeList<TElement1>(allocator);
            var onlyInArray2List = new NativeList<TElement2>(allocator);
            for (int i = 0; i < array1.Length; i++) {
                if (!array2.Contains(array1[i])) {
                    onlyInArray1List.Add(array1[i]);
                }
            }
            for (int i = 0; i < array2.Length; i++) {
                if (!array1.Contains(array2[i])) {
                    onlyInArray2List.Add(array2[i]);
                }
            }
            onlyInArray1 = onlyInArray1List.AsArray();
            onlyInArray2 = onlyInArray2List.AsArray();
        }
        public static bool Contains<TElement>(this DynamicBuffer<TElement> buffer, TElement value) where TElement : struct, IEquatable<TElement> {
            for (int i = 0; i < buffer.Length; i++) {
                if (buffer[i].Equals(value))
                    return true;
            }
            return false;
        }
        public static bool Remove<TElement>(this DynamicBuffer<TElement> buffer, TElement value) where TElement : struct, IEquatable<TElement> {
            for (int i = 0; i < buffer.Length; i++) {
                if (buffer[i].Equals(value)) {
                    buffer.RemoveAt(i);
                    return true;
                }
            }
            return false;
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