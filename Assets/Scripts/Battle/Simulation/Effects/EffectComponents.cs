using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Reactics.Battle.Map;
using Unity.Burst;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace Reactics.Battle
{

    public struct EffectReference : ISharedComponentData, IEquatable<EffectReference>
    {
        public EffectAsset effect;

        public EffectReference(EffectAsset effect)
        {
            this.effect = effect;
        }

        public override bool Equals(object obj)
        {
            return obj is EffectReference reference &&
                   EqualityComparer<EffectAsset>.Default.Equals(effect, reference.effect);
        }

        public bool Equals(EffectReference other)
        {
            return EqualityComparer<EffectAsset>.Default.Equals(effect, other.effect);
        }

        public override int GetHashCode()
        {
            return 1971684980 + EqualityComparer<EffectAsset>.Default.GetHashCode(effect);
        }
    }

    public struct EffectCursor<T> : IComponentData where T : unmanaged
    {
        public int index;

        public T target;

        public EffectCursor(int index, T target)
        {

            this.index = index;
            this.target = target;
        }
    }
    public struct RunEffectCursor : IComponentData { }

    public struct ExecuteEffect
    {

    }
    public struct DoEffect<T> : IComponentData where T : unmanaged
    {
        public Entity effectDataEntity;
        public int index;
        public Entity dataEntity;
        public T data;
        public DoEffect(Entity effectDataEntity, int index, Entity dataEntity, T data)
        {

            this.effectDataEntity = effectDataEntity;
            this.index = index;
            this.dataEntity = dataEntity;
            this.data = data;
        }
    }
    public struct EffectIndexData : IComponentData
    {
        public Entity effectDataEntity;
        public int index;
        public EffectIndexData(Entity effectDataEntity, int index)
        {
            this.effectDataEntity = effectDataEntity;
            this.index = index;
        }
    }

    public struct EffectSelection<T> : IComponentData where T : unmanaged
    {
        public UnsafeList<T> selection;
    }

}