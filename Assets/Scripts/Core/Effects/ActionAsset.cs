using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Core.Effects {
    [CreateAssetMenu(fileName = "Action", menuName = "Reactics/Action", order = 0)]
    public class ActionAsset : ScriptableObject {
        [SerializeField]
        [LocalizationTableName("ActionInfo")]
        public EmbeddedLocalizedAsset<ActionAssetInfo> info;
        [SerializeField]
        public AssetReference<EffectAsset> effectAsset;
        [SerializeField]
        public AssetReference<TargetFilterAsset> targetFilterAsset;
    }

    public class ActionAssetInfo : ScriptableObject {

        [SerializeField]
        public new string name;

        [SerializeField]
        public string description;

    }



}