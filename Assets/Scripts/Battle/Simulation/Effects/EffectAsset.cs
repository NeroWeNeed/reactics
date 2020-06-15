using System.Collections.ObjectModel;
using System.Collections.Generic;
using Reactics.Battle.Map;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Battle
{


    public class EffectAsset : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public TargetType type;
        [SerializeReference, HideInInspector]
        public IEffect[] effect;
        [SerializeField, HideInInspector]
        public int[] rootIndices;
        public int EffectCount { get => effect == null ? 0 : effect.Length; }
        public int RootCount { get => rootIndices == null ? 0 : rootIndices.Length; }
    }
}