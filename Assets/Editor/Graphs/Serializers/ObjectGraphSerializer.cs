using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEngine;

namespace Reactics.Core.Editor.Graph {
    public abstract class ObjectGraphSerializer<TOutput> {

        public virtual bool CanSerialize(IObjectGraphNodeProvider provider, ObjectGraphView graphView, out string message) {
            message = "";
            return true;
        }
        public abstract bool Serialize(TOutput target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out TOutput result);
        public abstract bool Deserialize(TOutput target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out TOutput result);
    }

    public interface IObjectGraphPreSerializerCallback {
        void OnPreSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload);
    }
    public interface IObjectGraphPostSerializerCallback {
        void OnPostSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload);
    }
    public interface IObjectGraphPreDeserializerCallback {
        object[] OnPreDeserialize(SerializedObject obj, ObjectGraphView graphview, object[] data);
    }
    public interface IObjectGraphPostDeserializerCallback {
        void OnPostDeserialize(SerializedObject obj, ObjectGraphView graphview, ref SortedDictionary<string, ObjectGraphModel.Entry> entries);
    }

    public struct ObjectGraphSerializerPayload {
        public ObjectGraphView graphView;
        public List<Entry> entries;
        public struct Entry {
            public string key;
            public object data;
            public ObjectGraphModel.Entry entry;
            public ObjectGraphNode node;

            public Entry(string key, object data, ObjectGraphModel.Entry entry, ObjectGraphNode node) {
                this.key = key;
                this.data = data;
                this.entry = entry;
                this.node = node;
            }
            public Entry(Entry old, object data, ObjectGraphModel.Entry entry, ObjectGraphNode node) {
                this.key = old.key;
                this.data = data;
                this.entry = entry;
                this.node = node;
            }
        }
    }

    public class ObjectGraphSerializationPayload {
        public ObjectGraphNode[] nodes;
        public ObjectGraphModel.Entry[] entries;
        public string[] keys;
        public object[] data;
        public ObjectGraphSerializationPayload(int length) {
            this.nodes = new ObjectGraphNode[length];
            this.entries = new ObjectGraphModel.Entry[length];
            this.keys = new string[length];
            this.data = new object[length];
        }
        public ObjectGraphSerializationPayload(string[] keys) {
            this.nodes = new ObjectGraphNode[keys.Length];
            this.entries = new ObjectGraphModel.Entry[keys.Length];
            this.keys = keys;
            this.data = new object[keys.Length];
        }


    }

}