using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Core.Map {
    public interface IMap<ITile, ISpawnGroup> where ITile : IMapTile where ISpawnGroup : IMapSpawnGroup {
        string Name { get; }
        ushort Width { get; }
        ushort Length { get; }
        ITile GetTile(Point point);
        ITile GetTile(ushort x, ushort y);
        ITile GetTile(int x, int y);
        int TileCount { get; }
        Point GetSpawnGroupPoint(int spawnGroup, int index);
        int GetSpawnGroupPointCount(int spawnGroup);
        int SpawnGroupCount { get; }
        int Elevation { get; }
    }
    public interface IMapTile {
        short Elevation { get; }
        bool Inaccessible { get; }
    }
    public interface IMapSpawnGroup {
        Point this[int index] { get; }
        int Count { get; }
    }

    public interface IMapTileEffectHandler {
        void OnEnter(Entity entity, EntityCommandBuffer entityCommandBuffer, Point position);
        void OnExit(Entity entity, EntityCommandBuffer entityCommandBuffer, Point position);
        void OnTick(Entity entity, EntityCommandBuffer entityCommandBuffer, Point position);
    }

    public interface IMapTileEffectProvider {
        int KeyCount { get; }
        //Identifier GetKey(int index);


    }
    public interface IMapTileEffectProviderConfigurationBlock {

    }


}