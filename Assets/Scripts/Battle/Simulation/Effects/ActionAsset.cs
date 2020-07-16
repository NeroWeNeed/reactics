using System;
using System.Collections.Generic;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Battle.Unit {
    [CreateAssetMenu(fileName = "Action", menuName = "Reactics/Action", order = 0)]
    public class ActionAsset : ScriptableObject {
        [SerializeField]
        [LocalizationTableName("ActionInfo")]
        public EmbeddedLocalizedAsset<ActionAssetInfo> info;
        [SerializeField]
        public EffectAsset effectAsset;
        [SerializeField]
        public TargetFilterAsset targetAsset;
        [SerializeField]
        public FixedString64 sample;
    }

    public class ActionAssetInfo : ScriptableObject {

        [SerializeField]
        public new string name;

        [SerializeField]
        public string description;

        [SerializeField]
        public string summary;

    }



}