using System;
using System.Diagnostics.Contracts;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Reactics.Core.Commons {

    public sealed class AssetType : Attribute {

        public readonly Type type;

        public AssetType(Type type) {
            this.type = type;
        }
    }

    public interface IBlittableAssetReference : IKeyEvaluator { }

    /// <summary>
    /// Blittable Asset Reference that supports SubObjects with names less than 31 characters.
    /// </summary>
    [Serializable]
    public unsafe struct BlittableAssetReference64 : IBlittableAssetReference {
        public const byte SubObjectNameOffset = 32;
        public const short SubObjectNameMaxLength = 31;

        public const short ReferenceBufferSize = 63;
        [SerializeField]
        private short subObjectNameLength;
        public short SubObjectNameLength { get => subObjectNameLength; private set => subObjectNameLength = value; }
        [SerializeField]
        private fixed char reference[ReferenceBufferSize];
        public BlittableAssetReference64(AssetReference reference) {
            subObjectNameLength = 0;
            InternalSetReference(reference);
        }
        private void InternalSetReference(AssetReference reference) {
            if (reference == null || reference.AssetGUID == null) {
                fixed (char* destination = this.reference) {
                    UnsafeUtility.MemSet(destination, 0, ReferenceBufferSize * UnsafeUtility.SizeOf<char>());
                    subObjectNameLength = 0;
                }
            }
            else {
                var assetGuidCharArray = reference.AssetGUID.ToCharArray();
                if (string.IsNullOrEmpty(reference.SubObjectName)) {
                    fixed (char* destination = this.reference, assetGuid = &assetGuidCharArray[0]) {
                        UnsafeUtility.MemCpy(destination, assetGuid, 32 * UnsafeUtility.SizeOf<char>());
                        subObjectNameLength = 0;
                    }
                }
                else if (reference.SubObjectName.Length <= SubObjectNameMaxLength) {
                    var assetSubObjectNameCharArray = reference.SubObjectName.ToCharArray();
                    fixed (char* destination = this.reference, assetGuid = &assetSubObjectNameCharArray[0], assetSubObjectName = &assetSubObjectNameCharArray[0]) {
                        UnsafeUtility.MemCpy(destination, assetGuid, 32 * UnsafeUtility.SizeOf<char>());
                        subObjectNameLength = (short)reference.SubObjectName.Length;
                        UnsafeUtility.MemCpy(destination + (SubObjectNameOffset * UnsafeUtility.SizeOf<char>()), assetSubObjectName, subObjectNameLength * UnsafeUtility.SizeOf<char>());
                    }
                }
                else {
                    throw new ArgumentException($"Asset Reference to SubObject must have a name less than or equal to {SubObjectNameMaxLength} characters.");
                }
            }

        }
        public object RuntimeKey
        {
            get
            {
                if (subObjectNameLength > 0) {
                    var chars = new char[32 + subObjectNameLength + 2];
                    fixed (char* reference = this.reference, dst = &chars[0]) {
                        UnsafeUtility.MemCpy(dst, reference, 32 * UnsafeUtility.SizeOf<char>());
                        chars[32] = '[';
                        UnsafeUtility.MemCpy(dst + 33 * UnsafeUtility.SizeOf<char>(), reference, subObjectNameLength * UnsafeUtility.SizeOf<char>());
                        chars[subObjectNameLength + 33] = ']';

                    }
                    return new string(chars);
                }
                else {
                    fixed (char* reference = this.reference) {
                        return new string(reference, 0, 32);
                    }
                }
            }
        }

        public bool RuntimeKeyIsValid() {
            return true;
        }

        public static explicit operator BlittableAssetReference64(AssetReference value) => new BlittableAssetReference64(value);

        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>() => Addressables.LoadAssetAsync<TObject>(RuntimeKey);
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100) => Addressables.LoadSceneAsync(RuntimeKey, loadMode, activateOnLoad, priority);
        public override bool Equals(object obj) {
            if (obj is BlittableAssetReference64 blittableAssetReference64) {
                return Equals(blittableAssetReference64);
            }
            else
                return false;

        }

        public bool Equals(BlittableAssetReference64 other) {

            fixed (char* reference = this.reference) {
                return UnsafeUtility.MemCmp(reference, other.reference, UnsafeUtility.SizeOf<char>() * ReferenceBufferSize) == 0;
            }
        }

        public override int GetHashCode() {

            fixed (char* reference = this.reference) {
                int r = -1584136870;
                for (byte i = 0; i < ReferenceBufferSize; i++) {
                    r += reference[i];
                }
                return r;
            }
        }


    }
}