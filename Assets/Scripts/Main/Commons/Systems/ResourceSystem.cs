using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Reactics.Commons {
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    public abstract class ResourceManagementSystem : SystemBase {
        public abstract bool IsValid(Resource resource);
        public abstract bool IsValid(params Resource[] resources);
        public abstract bool IsValid(NativeArray<Resource> resources);
        public abstract bool IsLoaded(Resource resource);
        public abstract float GetPercentageComplete(Resource resource);
        public abstract object this[Resource resource]
        {
            get;
        }
        public abstract bool TryGetValue(Resource resource, out object result);
        public abstract Resource Load<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object;
        public abstract Resource LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object;
        public abstract void Release(Resource resource);
    }
    public abstract class ResourceManagementSystem<TEntry> : ResourceManagementSystem where TEntry : IResourceSystemEntry, new() {
        protected List<Resource> available;
        protected List<Resource> unavailable;
        protected Dictionary<Resource, TEntry> handles;
        protected override void OnCreate() {
            handles = new Dictionary<Resource, TEntry>();
            available = new List<Resource>();
            unavailable = new List<Resource>();
            Enabled = false;
        }
        protected override void OnUpdate() {
            //Flush();
        }
        protected override void OnDestroy() {
            foreach (var kv in handles) {
                kv.Value.Release();
            }
        }
        public override bool IsValid(Resource resource) {
            return handles.ContainsKey(resource);
        }
        public override bool IsValid(params Resource[] resources) {
            foreach (var resource in resources) {
                if (!handles.ContainsKey(resource))
                    return false;
            }
            return true;
        }
        public override bool IsValid(NativeArray<Resource> resources) {
            foreach (var resource in resources) {
                if (!handles.ContainsKey(resource))
                    return false;
            }
            return true;
        }
        public override bool IsLoaded(Resource resource) {
            return handles[resource].IsDone;
        }
        public override float GetPercentageComplete(Resource resource) {
            return handles[resource].Percentage;
        }
        public override object this[Resource resource]
        {
            get
            {
                var handle = handles[resource];
                if (handle.IsDone)
                    return handle.Value;
                else
                    return null;
            }
        }

        public override bool TryGetValue(Resource resource, out object result) {
            if (handles.TryGetValue(resource, out TEntry handle) && handle.IsDone) {
                result = handle.Value;
                return true;
            }
            else {
                result = null;
                return false;
            }
        }

        public override Resource Load<TObject>(object key, int timeout = -1) {
            var handle = new TEntry();
            var task = handle.LoadAsync<TObject>(key, timeout);
            if (task?.Wait(timeout) != false) {
                var k = AllocateKey();
                handles[k] = handle;
                return k;
            }
            else {
                return Resource.Null;
            }
        }
        public override Resource LoadAsync<TObject>(object key, int timeout = -1) {
            var handle = new TEntry();
            handle.LoadAsync<TObject>(key, timeout);

            //var handle = Addressables.LoadAssetAsync<TObject>(key);
            var k = AllocateKey();
            handles[k] = handle;
            return k;
        }
        public override void Release(Resource resource) {
            if (handles.TryGetValue(resource, out TEntry handle)) {
                handle.Release();
                handles.Remove(resource);
            }
        }
        protected Resource AllocateKey() {
            Resource key;
            if (available.Count != 0) {
                key = available[0];
                available.RemoveAt(0);
            }
            else {
                key = new Resource
                {
                    Index = unavailable.Count,
                    Version = 0
                };
            }
            key.Version++;
            unavailable.Add(key);
            return key;
        }

        protected void DeallocateKey(Resource key) {

            if (available.Remove(key)) {
                unavailable.Add(key);
            }
        }


    }
    public interface IResourceSystemEntry {
        float Percentage { get; }

        bool IsDone { get; }

        object Value { get; }
        void Release();

        Task LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object;

        Task LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object;
    }
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddressableResourceManagementSystem : ResourceManagementSystem<AddressableResourceManagementSystem.Entry> {
        public struct Entry : IResourceSystemEntry {
            public float Percentage => handle.PercentComplete;

            public bool IsDone => handle.IsDone;

            public object Value => handle.Result;

            private AsyncOperationHandle handle;

            public Task LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object {
                handle = Addressables.LoadAssetAsync<TObject>(key);
                return handle.Task;
            }

            public Task LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object {
                handle = Addressables.LoadAssetAsync<TObject>(key);
                return handle.Task;
            }

            public void Release() {
                Addressables.Release(handle);
            }
        }
    }

#if UNITY_EDITOR
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EditorResourceManagementSystem : ResourceManagementSystem<EditorResourceManagementSystem.Entry> {
        public struct Entry : IResourceSystemEntry {
            public float Percentage { get; private set; }
            public bool IsDone { get; private set; }
            public object Value { get; private set; }
            public Task LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object {
                if (key is string str) {
                    Value = AssetDatabase.LoadAssetAtPath<TObject>(str);
                    Percentage = 1;
                    IsDone = true;
                }
                return null;
            }
            public Task LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object {
                return null;
            }
            public void Release() { }
        }
    }
#endif
    public struct Resource : IEquatable<Resource>, IComparable<Resource> {
        public int Index;
        public int Version;
        public static Resource Null;
        public int CompareTo(Resource other) {
            return this.Index - other.Index;
        }
        public bool Equals(Resource other) => this.Index == other.Index && this.Version == other.Version;

        public override bool Equals(object obj) {
            return this == (Resource)obj;
        }
        public override int GetHashCode() {
            return Index;
        }
        public override string ToString() {
            return Equals(Null) ? "Resource.Null" : $"Resource({Index}:{Version})";
        }
        public static bool operator <(Resource left, Resource right) {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(Resource left, Resource right) {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(Resource left, Resource right) {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(Resource left, Resource right) {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(Resource left, Resource right) {
            return left.Equals(right);
        }
        public static bool operator !=(Resource left, Resource right) {
            return !left.Equals(right);
        }
    }

    public static class ResourceManagementExtensions {
        public static ResourceManagementSystem GetResourceManagementSystem(this World world) {
#if UNITY_EDITOR
            return world.GetOrCreateSystem<EditorResourceManagementSystem>();
#else
            return world.GetOrCreateSystem<AddressableResourceManagementSystem>();
#endif
        }
    }
}