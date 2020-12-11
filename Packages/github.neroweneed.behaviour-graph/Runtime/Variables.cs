using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NeroWeNeed.BehaviourGraph {

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class VariableDefinitionAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class VariableOperationAttribute : Attribute {
        public string Identifier { get; }
        public string DisplayName { get; }
        public Type InputType { get; }
        public Type OutputType { get; }
        public Type ConfigurationType { get; }
        public Type BehaviourType { get; }

        public VariableOperationAttribute(string identifier, Type inputType, Type outputType, Type configurationType, Type behaviourType, string displayName = null) {

            Identifier = identifier;
            DisplayName = displayName ?? identifier;
            InputType = inputType;
            OutputType = outputType;
            ConfigurationType = configurationType;
            BehaviourType = behaviourType;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class BehaviourAttribute : Attribute {
        public string Identifier { get; }
        public Type ConfigurationType { get; }
        public Type BehaviourType { get; }
        public string DisplayName { get; }

        public BehaviourAttribute(string identifier, Type configurationType, Type behaviourType, string displayName = null) {
            Identifier = identifier;
            ConfigurationType = configurationType;
            BehaviourType = behaviourType;
            DisplayName = displayName ?? identifier;
        }
    }
    [Serializable]
    public struct VariableInfo {
        [SerializeField]
        private TypeCode typeCode;
        public TypeCode TypeCode { get => typeCode; }
        [SerializeField]
        private int memoryLength;
        public int MemoryLength { get => memoryLength; }
        public VariableInfo(Type type, byte memoryLength = 0) {
            this.typeCode = System.Type.GetTypeCode(type);
            switch (this.typeCode) {
                //Boolean
                case TypeCode.Boolean:
                    this.memoryLength = 1;
                    break;
                //Character
                case TypeCode.Char:
                    this.memoryLength = 2;
                    break;
                //Unsigned
                case TypeCode.Byte:
                    this.memoryLength = memoryLength == default ? 1 : memoryLength;
                    break;
                case TypeCode.UInt16:
                    this.memoryLength = memoryLength == default ? 2 : memoryLength;
                    break;
                case TypeCode.UInt32:
                    this.memoryLength = memoryLength == default ? 4 : memoryLength;
                    break;
                case TypeCode.UInt64:
                    this.memoryLength = memoryLength == default ? 8 : memoryLength;
                    break;
                //Signed
                case TypeCode.SByte:
                    this.memoryLength = memoryLength == default ? 1 : memoryLength;
                    break;
                case TypeCode.Int16:
                    this.memoryLength = memoryLength == default ? 2 : memoryLength;
                    break;
                case TypeCode.Int32:
                    this.memoryLength = memoryLength == default ? 4 : memoryLength;
                    break;
                case TypeCode.Int64:
                    this.memoryLength = memoryLength == default ? 8 : memoryLength;
                    break;
                //Floating Point
                case TypeCode.Single:
                    this.memoryLength = 4;
                    break;
                case TypeCode.Double:
                    this.memoryLength = 8;
                    break;
                //Unmanaged
                case TypeCode.Object:
                    Contract.Requires(UnsafeUtility.IsUnmanaged(type));

                    this.memoryLength = memoryLength == default ? UnsafeUtility.SizeOf(type) : memoryLength;
                    break;
                //Unsupported
                default:
                    throw new ArgumentException($"Unsupported Type: {type}");


            }
        }
    }
    public static class PointerTypeUtility {
        public static readonly int[] INTEGER_FIELD_LENGTHS = new int[] { 1, 2, 4, 8 };
        public static readonly int[] FLOATING_POINT_FIELD_LENGTHS = new int[] { 4, 8 };
        public static readonly int[] BOOLEAN_FIELD_LENGTHS = new int[] { 1 };
        public static readonly int[] CHARACTER_FIELD_LENGTHS = new int[] { 2 };
        /// <summary>
        /// Returns possible field lengths for this Pointer Type. Unmanaged Types return empty arrays because the field length should match the size of the unmanaged type.
        /// </summary>
        /// <param name="type">Pointer Type</param>
        /// <returns>Array refering to the possible field lengths for the pointer type.</returns>
        public static int[] GetFieldLengths(this TypeCode type) {
            int a = 4;
            uint b = 8;
            var c = a + b;
            switch (type) {
                case TypeCode.Boolean:
                    return BOOLEAN_FIELD_LENGTHS;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return INTEGER_FIELD_LENGTHS;
                case TypeCode.Char:
                    return CHARACTER_FIELD_LENGTHS;
                case TypeCode.Single:
                case TypeCode.Double:
                    return FLOATING_POINT_FIELD_LENGTHS;
                default:
                    return Array.Empty<int>();
            }
        }
    }
    public unsafe delegate void VariableOperation(IntPtr input, int length, IntPtr output, int outputLength);


}