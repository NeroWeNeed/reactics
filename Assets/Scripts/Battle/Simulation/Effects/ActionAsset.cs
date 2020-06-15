using System;
using System.Collections.Generic;
using Reactics.Battle.Map;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Battle.Unit
{

    [Serializable]
    public class AssetReferenceAction : AssetReferenceT<ActionAsset>
    {
        public AssetReferenceAction(string guid) : base(guid)
        {
        }
    }
    public class ActionAsset : ScriptableObject
    {
        [SerializeField]
        public EffectAsset effectAsset;
        [SerializeField]
        public TargetFilterAsset targetAsset;
    }



}