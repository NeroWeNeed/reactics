using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using UnityEditor;
using UnityEngine;
namespace Reactics.Editor.Graph {
    public abstract class ObjectGraphAsset : ScriptableObject {
        [SerializeField, HideInInspector]
        protected int hash;

        public int Hash { get => hash; set => hash = value; }
        [SerializeField, HideInInspector]
        protected int version;
        public int Version { get => version; set => version = value; }

        public abstract Type GetOutputAssetType();

        public abstract string DefaultOutputPath { get; }
        [SerializeField, HideInInspector, Delayed]
        public string outputPath;
        [SerializeField, HideInInspector, Delayed]
        public string outputName;

        [SerializeField, HideInInspector]
        public bool generateOnReload = true;
        [SerializeField, HideInInspector]
        public bool generateOnPlay = true;
        public abstract void UpdateAsset(SerializedObject serializedObject);
        private void OnValidate() {
            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = DefaultOutputPath;
            if (string.IsNullOrWhiteSpace(outputName))
                outputName = name;
        }
    }
    public abstract class ObjectGraphAsset<TObject, TOutputAsset> : ObjectGraphAsset where TOutputAsset : ScriptableObject {

        public override Type GetOutputAssetType() => typeof(TOutputAsset);
        [SerializeField, HideInInspector]
        protected ObjectGraphAssetNode[] nodes;

        [SerializeField, HideInInspector]
        protected ObjectGraphAssetVariableNode[] variables;
        [SerializeReference, HideInInspector]
        protected TObject[] objects;

        public TObject[] Objects { get => objects; }

    }
    [Serializable]
    public struct ObjectGraphAssetNode {
        public Rect layout;
        public string id;
        public int index;
        public int next;
    }
    [Serializable]
    public struct ObjectGraphAssetVariableNode {
        public Rect layout;
        public string id;
        public string type;
        public string address;
        public Connection[] connections;
        [Serializable]
        public struct Connection {
            public int index;
            public string field;
        }
        public Variable CreateVariable() {
            int offset = 0;
            Type containerType = Type.GetType(this.type);
            Type type = containerType;
            Debug.Log(containerType.GUID);
            foreach (var step in address.Split('.')) {
                var field = type.GetField(step, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) {
                    throw new System.InvalidOperationException($"Invalid Address {address} for type {type}");
                }
                offset += Marshal.OffsetOf(type, field.Name).ToInt32();
                type = field.FieldType;
            }
            return new Variable
            {
                containerId = containerType.GUID,
                offset = offset,
                length = Marshal.SizeOf(type.IsEnum ? Enum.GetUnderlyingType(type) : type)
            };

        }
        public bool ResolveAddress(out int offset, out long length, out BlittableGuid guid) {
            offset = 0;
            Type containerType = Type.GetType(this.type);
            Type type = containerType;

            /*             if (addressSegments == null || addressSegments.Length == 0)
                            addressSegments = new string[] { address }; */

            foreach (var step in address.Split('.')) {
                var field = type.GetField(step, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) {
                    offset = -1;
                    length = -1;
                    guid = default;
                    return false;
                }
                offset += Marshal.OffsetOf(type, field.Name).ToInt32();
                type = field.FieldType;
            }
            length = Marshal.SizeOf(type.IsEnum ? Enum.GetUnderlyingType(type) : type);
            guid = containerType.GUID;
            return true;


        }
    }



}