using System.Collections.Generic;
using Reactics.Battle.Map;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Battle {
    [CreateAssetMenu(fileName = "TargetFilter", menuName = "Reactics/Target Filter", order = 0)]
    public class TargetFilterAsset : ScriptableObject {
        [SerializeField, HideInInspector]
        public TargetType type;
        [SerializeReference]
        public ITargetFilter[] filter;
    }


}