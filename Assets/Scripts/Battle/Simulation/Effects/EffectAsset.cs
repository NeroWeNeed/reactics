using System.Collections.Generic;
using System.Collections.ObjectModel;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Battle {


    [CreateAssetMenu(fileName = "Effect", menuName = "Reactics/Effect", order = 0)]
    public class EffectAsset : ScriptableObject {
        [SerializeField, HideInInspector]
        public TargetType type;
        [SerializeReference, HideInInspector]
        public IEffect[] effect;
        [SerializeField, HideInInspector]
        public int[] roots;

        [SerializeField, HideInInspector]
        public OffsetReference[] variables;
        public int EffectCount { get => effect == null ? 0 : effect.Length; }
        public int RootCount { get => roots == null ? 0 : roots.Length; }
    }
}