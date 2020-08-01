using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Commons {

    //TODO: Make Burst compatible
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EventManagementSystem : SystemBase {

        private DefaultEventPropagator defaultEventRouter = new DefaultEventPropagator();
        private ObjectPool<EventIndex> eventIndexPool;


        //TODO: Proper multimap implementation
        private Dictionary<EventIndex, List<IEventListener>> eventListeners;

        private HashSet<IEventRouter> eventRouters;

        protected override void OnCreate() {
            eventListeners = new Dictionary<EventIndex, List<IEventListener>>();
            eventRouters = new HashSet<IEventRouter>();
            eventIndexPool = new ObjectPool<EventIndex>();
            defaultEventRouter.Init(eventIndexPool);
        }
        protected override void OnUpdate() {

        }
        protected override void OnDestroy() {
            defaultEventRouter.Dispose();
        }
        public void RegisterListener(IEventListener listener) {
            foreach (var index in eventRouters.Select((r) => r.Route(eventType: listener.EventType, true)).Append(defaultEventRouter.Route(eventType: listener.EventType, true)).Distinct()) {
                RegisterListener(index, listener);
            }
        }
        private void RegisterListener(EventIndex index, IEventListener listener) {
            if (index == EventIndex.Null)
                return;
            if (!eventListeners.TryGetValue(index, out List<IEventListener> listeners)) {
                listeners = new List<IEventListener>();
                eventListeners[index] = listeners;
            }
            listeners.Add(listener);
        }
        public void UnregisterListener(IEventListener listener) {
            foreach (var index in eventRouters.Select((r) => r.Route(eventType: listener.EventType, false)).Append(defaultEventRouter.Route(eventType: listener.EventType, false)).Distinct()) {
                UnregisterListener(index, listener);
            }

        }
        private void UnregisterListener(EventIndex index, IEventListener listener) {
            if (index == EventIndex.Null)
                return;
            if (eventListeners.TryGetValue(index, out List<IEventListener> listeners)) {
                listeners.Remove(listener);
                if (listeners.Count == 0) {
                    eventListeners.Remove(index);
                    foreach (var r in eventRouters) {
                        r.DisposeEventIndex(index);
                    }
                    defaultEventRouter.DisposeEventIndex(index);
                    eventIndexPool.Destroy(index);
                }
            }
        }
        public void RegisterRouter(IEventRouter eventRouter) {
            eventRouters.Add(eventRouter);
        }
        public void UnregisterRouter(IEventRouter eventRouter) {
            eventRouters.Remove(eventRouter);
        }
        public void Dispatch(object eventObject) {
            foreach (var index in eventRouters.Select((r) => r.Route(eventObject: eventObject)).Append(defaultEventRouter.Route(eventObject: eventObject, false)).Distinct()) {
                Dispatch(index, eventObject);
            }
        }
        private void Dispatch(EventIndex index, object eventObject) {
            if (index == EventIndex.Null)
                return;
            if (eventListeners.TryGetValue(index, out List<IEventListener> listeners)) {
                foreach (var listener in listeners) {
                    listener.OnEvent(eventObject);
                }
            }
        }


    }
    public interface IEventRouter : IDisposable {
        void Init(ObjectPool<EventIndex> eventIndexPool);
        EventIndex Route(object eventObject, bool shouldCreate = false);
        EventIndex Route(Type eventType, bool shouldCreate = false);

        void DisposeEventIndex(EventIndex index);
    }
    public abstract class BaseEventRouter<TEvent> : IEventRouter {

        public virtual void Dispose() { }
        protected ObjectPool<EventIndex> EventIndexPool { get; private set; }
        public void Init(ObjectPool<EventIndex> eventIndexPool) {
            EventIndexPool = eventIndexPool;
        }
        public abstract EventIndex Route(TEvent eventObject, bool shouldCreate);

        public EventIndex Route(object eventObject, bool shouldCreate) {
            if (eventObject is TEvent tEvent) {
                return Route(tEvent, shouldCreate);
            }
            else {
                return EventIndex.Null;
            }
        }

        public abstract EventIndex Route(Type eventType, bool shouldCreate);
        public abstract void DisposeEventIndex(EventIndex index);
    }
    public class DefaultEventPropagator : IEventRouter {

        public Type[] EventTypes => null;
        private readonly Dictionary<Type, EventIndex> eventIndices = new Dictionary<Type, EventIndex>();
        protected ObjectPool<EventIndex> EventIndexPool { get; private set; }
        public void Dispose() {
            foreach (var item in eventIndices) {
                EventIndexPool.Destroy(item.Value);
            }
            eventIndices.Clear();
        }
        public void Init(ObjectPool<EventIndex> eventIndexPool) {
            EventIndexPool = eventIndexPool;
        }

        public EventIndex Route(object eventObject, bool shouldCreate) {
            return Route(eventObject.GetType(), shouldCreate);
        }

        public EventIndex Route(Type eventType, bool shouldCreate) {
            if (eventIndices.TryGetValue(eventType, out EventIndex index)) {
                return index;
            }
            else if (shouldCreate) {
                index = EventIndexPool.Create();
                eventIndices[eventType] = index;
                return index;
            }
            else {
                return EventIndex.Null;
            }
        }

        public void DisposeEventIndex(EventIndex index) {
            foreach (var kv in eventIndices.Where((kv) => kv.Value == index).ToArray()) {
                eventIndices.Remove(kv.Key);
            }
        }
    }
    public interface IEventListener {
        Type EventType { get; }
        void OnEvent(object eventObject);
    }
    public abstract class BaseEventListener<TEvent> : IEventListener {
        public Type EventType => typeof(TEvent);
        public void OnEvent(object eventObject) {
            OnEvent((TEvent)eventObject);
        }
        public abstract void OnEvent(TEvent eventObject);
    }

    public struct EventIndex : IObjectPoolObject, IEquatable<EventIndex> {
        public static readonly EventIndex Null;
        public int Index { get; set; }
        public int Version { get; set; }

        public bool Equals(EventIndex other) {
            return Index == other.Index && Version == other.Version;
        }

        public override int GetHashCode() {
            int hashCode = -561076678;
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + Version.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj) {
            if (obj is EventIndex other) {
                return Equals(other: other);
            }
            return false;
        }

        public static bool operator ==(EventIndex self, EventIndex other) => self.Equals(other);
        public static bool operator !=(EventIndex self, EventIndex other) => self.Equals(other);
    }


    public class ObjectPool<TObject> where TObject : IObjectPoolObject, new() {
        private readonly Queue<TObject> available = new Queue<TObject>();
        private readonly HashSet<TObject> unavailable = new HashSet<TObject>();

        private int index;
        public TObject Create() {
            if (available.Count > 0) {
                var obj = available.Dequeue();
                obj.Version++;
                unavailable.Add(obj);
                return obj;
            }
            else {
                var obj = new TObject();
                obj.Index = index++;
                obj.Version++;
                unavailable.Add(obj);
                return obj;
            }
        }
        public void Destroy(TObject obj) {
            if (unavailable.Remove(obj)) {
                available.Enqueue(obj);
            }
        }
    }
    public interface IObjectPoolObject {
        int Index { get; set; }
        int Version { get; set; }
    }
}