using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.Effects {
    [Serializable]
    public struct Variable : IEquatable<Variable> {
        public BlittableGuid containerId;
        public int offset;
        public long length;

        public override bool Equals(object obj) {
            return obj is Variable other && Equals(other);
        }

        public bool Equals(Variable other) {
            return containerId.Equals(other.containerId) &&
                   offset == other.offset &&
                   length == other.length;
        }

        public override int GetHashCode() {
            int hashCode = -1045769724;
            hashCode = hashCode * -1521134295 + containerId.GetHashCode();
            hashCode = hashCode * -1521134295 + offset.GetHashCode();
            hashCode = hashCode * -1521134295 + length.GetHashCode();
            return hashCode;
        }
    }
    public interface IVariableOperation {
        void Invoke(IntPtr pointer, long length, TypeCode typeCode);
    }
    public unsafe struct MultiplyVariable : IVariableOperation {
        public float value;
        public void Invoke(IntPtr pointer, long length, TypeCode typeCode) {
            switch (typeCode) {
                case TypeCode.Boolean:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (UnsafeUtility.ReadArrayElement<byte>(pointer.ToPointer(), 0) * value) != 0);
                    break;
                case TypeCode.Single:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (float)(UnsafeUtility.ReadArrayElement<float>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Double:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (double)(UnsafeUtility.ReadArrayElement<double>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Int16:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (short)(UnsafeUtility.ReadArrayElement<short>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Int32:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (int)(UnsafeUtility.ReadArrayElement<int>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Int64:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (long)(UnsafeUtility.ReadArrayElement<long>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.SByte:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (sbyte)(UnsafeUtility.ReadArrayElement<sbyte>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.UInt16:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (ushort)(UnsafeUtility.ReadArrayElement<ushort>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Char:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (char)(UnsafeUtility.ReadArrayElement<char>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.UInt32:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (uint)(UnsafeUtility.ReadArrayElement<uint>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.UInt64:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (ulong)(UnsafeUtility.ReadArrayElement<ulong>(pointer.ToPointer(), 0) * value));
                    break;
                case TypeCode.Byte:
                    UnsafeUtility.WriteArrayElement(pointer.ToPointer(), 0, (byte)(UnsafeUtility.ReadArrayElement<byte>(pointer.ToPointer(), 0) * value));
                    break;
            }


        }
    }
    [Serializable]
    public struct VariableOperationSequence : IEquatable<VariableOperationSequence> {
        public int variable;
        public int component;
        public int offset;
        public long length;
        public TypeCode type;
        [SerializeReference]
        public IVariableOperation[] operations;

        public override bool Equals(object obj) {
            return obj is VariableOperationSequence sequence &&
                   variable == sequence.variable &&
                   component == sequence.component &&
                   offset == sequence.offset &&
                   length == sequence.length &&
                   EqualityComparer<IVariableOperation[]>.Default.Equals(operations, sequence.operations);
        }
        public bool Equals(VariableOperationSequence other) {
            return variable == other.variable &&
                    component == other.component &&
                    offset == other.offset &&
                    length == other.length &&
            EqualityComparer<IVariableOperation[]>.Default.Equals(operations, other.operations);
        }

        public override int GetHashCode() {
            int hashCode = 1493794182;
            hashCode = hashCode * -1521134295 + variable.GetHashCode();
            hashCode = hashCode * -1521134295 + component.GetHashCode();
            hashCode = hashCode * -1521134295 + offset.GetHashCode();
            hashCode = hashCode * -1521134295 + length.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IVariableOperation[]>.Default.GetHashCode(operations);
            return hashCode;
        }

        public unsafe void Invoke(NativeHashMap<BlittableGuid, IntPtr> sources, NativeArray<Variable> variables, NativeArray<IntPtr> components) {
            var variable = variables[this.variable];
            var component = components[this.component];
            if (sources.TryGetValue(variable.containerId, out IntPtr source)) {
                var pointer = (component + offset + ((int)math.max(0, this.length - variable.length))).ToPointer();
                long length = variable.length - math.abs(variable.length - this.length);
                UnsafeUtility.MemCpy(pointer, (source + variable.offset + ((int)math.max(variable.length - this.length, 0))).ToPointer(), length);
                if (operations != null) {
                    foreach (var operation in operations) {
                        operation.Invoke((IntPtr)pointer, length, type);
                    }
                }
            }
        }
    }
}