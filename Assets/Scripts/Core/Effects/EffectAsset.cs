using System.Collections.Generic;
using System.Collections.ObjectModel;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Core.Effects {


    [CreateAssetMenu(fileName = "Effect", menuName = "Reactics/Effect", order = 0)]
    public class EffectAsset : ScriptableObject {
        [SerializeField, HideInInspector]
        public TargetType type;
        [SerializeReference, HideInInspector]
        public IEffect[] effect;
        [SerializeField, HideInInspector]
        public int[] roots;

        [SerializeField, HideInInspector]
        public VariableCollection[] variables;

        public int EffectCount { get => effect == null ? 0 : effect.Length; }
        public int RootCount { get => roots == null ? 0 : roots.Length; }
    }
}