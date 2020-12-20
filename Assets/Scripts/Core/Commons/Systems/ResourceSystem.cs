using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
/* 
namespace Reactics.Core.Commons {
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    public abstract class ResourceManager : SystemBase {


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
    public abstract class ResourceManager<TEntry> : ResourceManager where TEntry : IResourceSystemEntry, new() {
        protected List<Resource> available;
        protected List<Resource> unavailable;
        protected Dictionary<Resource, TEntry> handles;

        protected Dictionary<object, ResourceCounter> keys;

        protected EventManagementSystem eventManagementSystem;

        protected override void OnCreate() {

            handles = new Dictionary<Resource, TEntry>();
            available = new List<Resource>();
            unavailable = new List<Resource>();
            keys = new Dictionary<object, ResourceCounter>();

            Enabled = false;
            eventManagementSystem = World.GetOrCreateSystem<EventManagementSystem>();
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
            if (keys.TryGetValue(key, out ResourceCounter counter)) {
                counter.count++;
                keys[key] = counter;
                return counter.resource;
            }
            var handle = new TEntry();
            var task = handle.LoadAsync<TObject>(key, timeout);

            if (task?.Wait(timeout) != false) {
                var k = AllocateKey();
                handles[k] = handle;
                keys[key] = new ResourceCounter(k);
                eventManagementSystem.Dispatch(new ResourceLoadEvent<TObject>(k, (TObject)handle.Value));
                return k;
            }
            else {
                return Resource.Null;
            }
        }

        public override Resource LoadAsync<TObject>(object key, int timeout = -1) {

            if (keys.TryGetValue(key, out ResourceCounter counter)) {
                counter.count++;
                keys[key] = counter;
                return counter.resource;

            }
            var handle = new TEntry();
            var task = handle.LoadAsync<TObject>(key, timeout);
            var k = AllocateKey();
            keys[key] = new ResourceCounter(k);
            task.ContinueWith((t) => eventManagementSystem.Dispatch(new ResourceLoadEvent<TObject>(k, t.Result)));
            handles[k] = handle;

            return k;
        }

        public override void Release(Resource resource) {
            if (handles.TryGetValue(resource, out TEntry handle)) {
                eventManagementSystem.Dispatch(new ResourceReleaseEvent(resource, handle.Value));
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

    public struct ResourceCounter {
        public Resource resource;
        public int count;

        public ResourceCounter(Resource resource) {
            this.resource = resource;
            this.count = 1;
        }
    }
    public class ResourceEventRouter : IEventRouter {

        private ObjectPool<EventIndex> EventIndexPool;

        private Dictionary<Type, EventIndex> eventIndices;

        public void Dispose() {
            foreach (var item in eventIndices) {
                EventIndexPool.Destroy(item.Value);
            }
            eventIndices.Clear();
        }

        public void DisposeEventIndex(EventIndex index) {
            foreach (var kv in eventIndices.Where((kv) => kv.Value == index).ToArray()) {
                eventIndices.Remove(kv.Key);
            }
        }

        public void Init(ObjectPool<EventIndex> eventIndexPool) {
            eventIndices = new Dictionary<Type, EventIndex>();
            EventIndexPool = eventIndexPool;
        }

        public EventIndex Route(object eventObject, bool shouldCreate = false) {
            if (eventObject is IResourceEvent resourceEvent) {

                if (eventIndices.TryGetValue(resourceEvent.EventType, out EventIndex index)) {
                    return index;
                }
                else if (shouldCreate) {
                    index = EventIndexPool.Create();
                    eventIndices[resourceEvent.EventType] = index;
                    return index;
                }
                else {
                    return EventIndex.Null;
                }
            }
            else {
                return EventIndex.Null;
            }
        }

        public EventIndex Route(Type eventType, bool shouldCreate = false) {

            if (typeof(IResourceEvent).IsAssignableFrom(eventType) && eventType.IsGenericType) {
                var resourceType = eventType.GenericTypeArguments[0];
                if (eventIndices.TryGetValue(resourceType, out EventIndex index)) {
                    return index;
                }
                else if (shouldCreate) {
                    index = EventIndexPool.Create();
                    eventIndices[resourceType] = index;
                    return index;
                }
                else {
                    return EventIndex.Null;
                }
            }
            else {
                return EventIndex.Null;
            }
        }
    }
    public interface IResourceEvent {
        Type EventType { get; }
        Resource Resource { get; }

        object Value { get; }
    }
    public interface IResourceEvent<TObject> : IResourceEvent {
        new TObject Value { get; }
    }
    public struct ResourceLoadEvent : IResourceEvent {


        public Resource Resource { get; }
        public object Value { get; }
        public Type EventType => Value.GetType();
        public ResourceLoadEvent(Resource resource, object value) {
            Resource = resource;
            Value = value;
        }
    }
    public struct ResourceLoadEvent<TObject> : IResourceEvent<TObject> {
        public Resource Resource { get; }
        public TObject Value { get; }
        public Type EventType => Value.GetType();
        object IResourceEvent.Value => Value;
        public ResourceLoadEvent(Resource resource, TObject value) {
            Value = value;
            Resource = resource;
        }


    }

    public struct ResourceReleaseEvent : IResourceEvent {

        public Resource Resource { get; }
        public object Value { get; }
        public Type EventType => Value.GetType();
        public ResourceReleaseEvent(Resource resource, object value) {
            Resource = resource;
            Value = value;
        }

    }
    public struct ResourceReleaseEvent<TObject> : IResourceEvent<TObject> {
        public Resource Resource { get; }
        public TObject Value { get; }
        public Type EventType => Value.GetType();
        object IResourceEvent.Value => Value;

        public ResourceReleaseEvent(Resource resource, TObject value) {
            Resource = resource;
            Value = value;
        }
    }
    public interface IResourceSystemEntry {

        float Percentage { get; }

        bool IsDone { get; }

        UnityEngine.Object Value { get; }
        void Release();

        Task<TObject> LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object;

        Task<TObject> LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object;

        bool IsResource(object key);

        bool IsResource(IResourceLocation key);
    }
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddressableResourceManager : ResourceManager<AddressableResourceManager.Entry> {
        public struct Entry : IResourceSystemEntry {

            private int key;
            public float Percentage => handle.PercentComplete;

            public bool IsDone => handle.IsDone;

            public UnityEngine.Object Value => handle.Result as UnityEngine.Object;

            private AsyncOperationHandle handle;


            public Task<TObject> LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object {
                foreach (var locator in Addressables.ResourceLocators) {
                    if (locator.Locate(key, typeof(TObject), out var locationBuffer)) {
                        return LoadAsync<TObject>(locationBuffer[0], timeout);
                    }
                }
                return null;

            }

            public Task<TObject> LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object {
                this.key = key.Hash(typeof(TObject));
                var t = Addressables.LoadAssetAsync<TObject>(key);
                handle = t;
                return t.Task;
            }

            public void Release() {
                Addressables.Release(handle);
            }

            public bool IsResource(object key) {

                if (Value == null)
                    return false;
                foreach (var locator in Addressables.ResourceLocators) {
                    if (locator.Locate(key, Value.GetType(), out var locationBuffer)) {
                        return IsResourceInternal(locationBuffer[0], Value.GetType());
                    }
                }
                return Value != null && key.Equals(this.key);
            }
            private bool IsResourceInternal(IResourceLocation key, Type type) {
                return key.Hash(type) == this.key;
            }

            public bool IsResource(IResourceLocation key) {
                return key.Equals(this.key);
            }
        }
    }

#if UNITY_EDITOR
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EditorResourceManager : ResourceManager<EditorResourceManager.Entry> {
        public struct Entry : IResourceSystemEntry {
            private object key;
            public float Percentage { get; private set; }
            public bool IsDone { get; private set; }
            public UnityEngine.Object Value { get; private set; }

            public bool IsResource(object key) {
                return key.Equals(this.key);
            }

            public bool IsResource(IResourceLocation key) {
                return key.Equals(this.key);
            }

            public Task<TObject> LoadAsync<TObject>(object key, int timeout = -1) where TObject : UnityEngine.Object {
                this.key = key;
                var target = this;
                return Task.Run(() =>
                {
                    if (key is string str) {
                        var result = AssetDatabase.LoadAssetAtPath<TObject>(str);
                        target.Value = result;
                        target.Percentage = 1;
                        target.IsDone = true;
                        return result;
                    }
                    return null;

                });
            }
            public Task<TObject> LoadAsync<TObject>(IResourceLocation key, int timeout = -1) where TObject : UnityEngine.Object {
                this.key = key;
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
        public static ResourceManager GetResourceSystem(this World world) {
#if UNITY_EDITOR
            return world.GetOrCreateSystem<EditorResourceManager>();
#else
            return world.GetOrCreateSystem<AddressableResourceManagementSystem>();
#endif
        }
    }

    public interface IResourceSystemCallback {

    }
    public interface IResourceSystemCallback<TObject> : IResourceSystemCallback where TObject : UnityEngine.Object {
        void AfterLoad(Resource key, TObject value);
        void BeforeRelease(Resource key, TObject value);
    }


} */