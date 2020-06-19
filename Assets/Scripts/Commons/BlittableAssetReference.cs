using System.Text;
using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Mathematics;

namespace Reactics.Commons
{

    public sealed class AssetType : Attribute {

        public readonly Type type;

        public AssetType(Type type)
        {
            this.type = type;
        }
    }
    /// <summary>
    /// Asset Reference designed to be used in unmanaged contexts. Sub-Object names should be under 118 characters (index 118 is reserved for null-termination). Structured to be 256 bytes in size in memory.
    /// </summary>
    [Serializable]
    public unsafe struct BlittableAssetReference : IKeyEvaluator
    {
        [SerializeField]
        private fixed byte assetGuid[16];
        [SerializeField]
        private fixed char subObjectName[119];

        [SerializeField]
        private short subObjectNameLength;

        public BlittableAssetReference(Guid guid)
        {
            var bytes = guid.ToByteArray();
            if (guid == Guid.Empty)
            {
                fixed (byte* dstPtr = this.assetGuid, srcPtr = bytes)
                {
                    UnsafeUtility.MemCpy(dstPtr, srcPtr, UnsafeUtility.SizeOf<byte>() * 16);
                }
                subObjectNameLength = 0;
            }
            else
            {
                subObjectNameLength = 0;
            }

        }
        public BlittableAssetReference(Guid guid, string subObjectName)
        {
            if (guid != Guid.Empty)
            {


                if (subObjectName.Length >= 119)
                    throw new ArgumentException("Sub-Object Name exceeds 119 characters. Consider renaming.");

                var guidBytes = guid.ToByteArray();
                if (subObjectName.Length == 0)
                {
                    fixed (byte* guidDstPtr = this.assetGuid, guidSrcPtr = guidBytes)
                    {
                        UnsafeUtility.MemCpy(guidDstPtr, guidSrcPtr, UnsafeUtility.SizeOf<byte>() * 16);
                    }
                }
                else
                {
                    var subObjectNameBytes = subObjectName.ToCharArray();
                    fixed (byte* guidDstPtr = this.assetGuid, guidSrcPtr = guidBytes)
                    {
                        UnsafeUtility.MemCpy(guidDstPtr, guidSrcPtr, UnsafeUtility.SizeOf<byte>() * 16);
                        fixed (char* subObjectNameDstPtr = this.subObjectName, subObjectNameSrcPtr = subObjectNameBytes)
                        {
                            UnsafeUtility.MemCpy(subObjectNameDstPtr, subObjectNameSrcPtr, UnsafeUtility.SizeOf<byte>() * subObjectName.Length);
                            UnsafeUtility.WriteArrayElement(subObjectNameDstPtr, subObjectName.Length, 0);
                        }
                    }

                }
                subObjectNameLength = (short)subObjectName.Length;
            }
            else
            {
                subObjectNameLength = 0;
            }
        }

        public object RuntimeKey
        {
            get
            {
                var sb = new StringBuilder();
                for (byte i = 0; i < 16; i++)
                {
                    sb.Append(ToHex((byte)(assetGuid[i] / 16)));
                    sb.Append(ToHex((byte)(assetGuid[i] % 16)));
                }
                if (subObjectNameLength > 0)
                {
                    fixed (char* subObjectPtr = subObjectName)
                    {
                        sb.Append(subObjectPtr, subObjectNameLength);
                    }
                }
                return sb.ToString();
            }
        }
        private char ToHex(byte value)
        {
            if (value < 10)
                return (char)(48 + value);
            else
                return (char)(97 + value - 10);
        }

        public override bool Equals(object obj)
        {
            if (obj is BlittableAssetReference guid)
            {
                return Equals(guid);
            }
            else
                return false;

        }

        public bool Equals(BlittableAssetReference other)
        {
            fixed (byte* thisPtr = assetGuid)
            {
                return UnsafeUtility.MemCmp(thisPtr, other.assetGuid, UnsafeUtility.SizeOf<byte>() * 16) == 0;
            }
        }

        public override int GetHashCode()
        {

            fixed (byte* thisPtr = assetGuid)
            {
                int r = -1584136870;
                for (byte i = 0; i < 16; i++)
                {
                    r += assetGuid[i];
                }
                return r;
            }
        }

        public bool RuntimeKeyIsValid()
        {
            for (byte i = 0; i < 16; i++)
            {
                if (assetGuid[i] != 0)
                    return true;
            }
            return false;
        }

        public static implicit operator Guid(BlittableAssetReference value)
        {
            var bytes = new byte[16];
            fixed (byte* bytePtr = bytes)
            {
                UnsafeUtility.MemCpy(bytePtr, value.assetGuid, UnsafeUtility.SizeOf<byte>() * 16);
            }
            return new Guid(bytes);
        }

        public static implicit operator BlittableAssetReference(Guid value)
        {
            return new BlittableAssetReference(value);
        }
    }
}