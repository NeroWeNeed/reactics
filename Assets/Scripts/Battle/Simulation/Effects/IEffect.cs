using System;
using Reactics.Battle.Map;
using Unity.Entities;
using Reactics.Commons;

namespace Reactics.Battle
{
    public interface IEffect { }
    public interface IEffect<TTarget> : IEffect where TTarget : unmanaged
    {
        void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, TTarget target, EntityCommandBuffer entityCommandBuffer);
    }
}