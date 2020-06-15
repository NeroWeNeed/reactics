using Reactics.Battle.Map;
using Unity.Entities;

namespace Reactics.Battle
{
    public interface IUtilityEffect : IEffect { }
    public interface IUtilityEffect<T> : IUtilityEffect, IEffect<T> where T : unmanaged
    {
    }

    /// <summary>
    /// /// Special Effect for chaining together actions.
    /// </summary>
    public struct LinearEffect : IUtilityEffect<MapBodyTarget>, IUtilityEffect<Point>, IUtilityEffect<MapBodyDirection>
    {
        public int effect;
        //[SerializeNodeIndex(typeof(T))]
        public int next;

        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer)
        {
            throw new System.NotImplementedException();
        }

        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyTarget target, EntityCommandBuffer entityCommandBuffer)
        {
            throw new System.NotImplementedException();
        }

        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyDirection target, EntityCommandBuffer entityCommandBuffer)
        {
            throw new System.NotImplementedException();
        }
    }
}