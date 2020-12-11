using System;
using NeroWeNeed.BehaviourGraph.Editor;
using NeroWeNeed.Commons;
using Reactics.Core.Commons;
using Unity.Entities;
[assembly: SearchableAssembly]
namespace Reactics.Core.AssetDefinitions {
    public struct EffectDelegateInfo {
        public Entity entity;

        public Point tile;
    }
    [Color("#FF303C")]
    public unsafe delegate void EffectDelegate(IntPtr data, long dataLength, EffectDelegateInfo* source, EffectDelegateInfo* target, int targetLength, IntPtr entityCommandBuffer);

    public unsafe delegate void FilterDelegate();

    public interface IEffectData {
        public DataReference<BehaviourGraph<EffectDelegate>> Effect { get; }
    }
    public interface IFilteredEffectData : IEffectData {
        public DataReference<BehaviourGraph<FilterDelegate>> Filter { get; }
    }
    [Serializable]
    public struct ActiveSkillEffectData : IFilteredEffectData {
        public ushort id;
        public DataReference<BehaviourGraph<EffectDelegate>> effect;
        public DataReference<BehaviourGraph<FilterDelegate>> filter;

        public DataReference<BehaviourGraph<FilterDelegate>> Filter => filter;

        public DataReference<BehaviourGraph<EffectDelegate>> Effect => effect;
    }
    [Serializable]
    public struct PassiveSkillEffectData : IFilteredEffectData {
        public ushort id;
        public DataReference<BehaviourGraph<EffectDelegate>> effect;
        public DataReference<BehaviourGraph<FilterDelegate>> filter;
        public DataReference<BehaviourGraph<FilterDelegate>> Filter => filter;

        public DataReference<BehaviourGraph<EffectDelegate>> Effect => effect;
    }
    [Serializable]
    public struct StatSkillEffectData : IEffectData {
        public ushort id;
        public DataReference<BehaviourGraph<EffectDelegate>> effect;
        public DataReference<BehaviourGraph<EffectDelegate>> Effect => effect;
    }

    public struct TileEffectData : IEffectData {
        public ushort id;
        public DataReference<BehaviourGraph<EffectDelegate>> effect;
        public DataReference<BehaviourGraph<EffectDelegate>> Effect => effect;
    }
    [Serializable]
    public struct CommanderSkillEffectData : IEffectData {
        public ushort id;
        public DataReference<BehaviourGraph<EffectDelegate>> effect;
        public DataReference<BehaviourGraph<EffectDelegate>> Effect => effect;
    }


}