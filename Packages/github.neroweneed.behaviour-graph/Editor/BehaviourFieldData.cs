using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using NeroWeNeed.Commons.Editor;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;
using UnityEngine;

namespace NeroWeNeed.BehaviourGraph.Editor {
    [Serializable]
    public class BehaviourConfigData {
        [SerializeField]
        private byte[] data = Array.Empty<byte>();
        public byte[] Data { get => data; }
        [SerializeField]
        private Field[] fields = Array.Empty<Field>();


        public BehaviourConfigData(Type type = null) {
            if (type != null) {
                Init(type);
            }
        }

        public Field[] Fields { get => fields; }

        public void Init(Type type) {
            Contract.Ensures(UnsafeUtility.IsUnmanaged(type));
            this.data = Init(type, data, fields, new byte[UnsafeUtility.SizeOf(type)], out Field[] newFields);
            this.fields = newFields;
        }
        public unsafe void Init(object obj) {
            Contract.Ensures(UnsafeUtility.IsUnmanaged(obj.GetType()));
            var newData = new byte[UnsafeUtility.SizeOf(obj.GetType())];
            var ptr = UnsafeUtility.PinGCObjectAndGetAddress(obj, out ulong handle);
            Marshal.Copy((IntPtr)ptr, newData, 0, newData.Length);
            UnsafeUtility.ReleaseGCObject(handle);
            this.data = Init(obj.GetType(), data, fields, newData, out Field[] newFields);
            this.fields = newFields;
        }
        private byte[] Init(Type type, byte[] oldData, Field[] oldFields, byte[] newData, out Field[] newFields) {
            newFields = type.Decompose().Select(field =>
                {
                    var oldIndex = Array.FindIndex(oldFields, (inputField) => inputField.info.fullName == field.fullName);
                    return new Field(field, oldIndex != -1 ? oldFields[oldIndex].data : null);
                }).ToArray();
            foreach (var outputField in newFields) {
                if (outputField.info.root) {
                    var inputFieldIndex = Array.FindIndex(oldFields, field => field.info.fullName == outputField.info.fullName);
                    if (inputFieldIndex >= 0) {
                        var inputField = oldFields[inputFieldIndex];
                        if (outputField.info.length == inputField.info.length && outputField.info.type.Value == inputField.info.type.Value) {
                            Array.Copy(oldData, inputField.info.offset, newData, outputField.info.offset, outputField.info.length);
                        }
                    }
                }
            }
            return newData;
        }


        [Serializable]
        public class Field {
            [SerializeField]
            public FieldOffsetInfo info;
            [SerializeField, HideInInspector]
            public string data;
            public Field(FieldOffsetInfo info, string data) {
                this.info = info;
                this.data = data;
            }
        }
    }

}