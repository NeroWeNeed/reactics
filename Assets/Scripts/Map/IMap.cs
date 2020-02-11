namespace Reactics.Battle
{

    public interface IMapHeader
    {

        string Name { get; }
        ushort Width { get; }
        ushort Length { get; }
        int Elevation { get; }
    }
    public interface IMapTiles
    {
        Tile this[ushort x, ushort y] { get; }
        Tile this[Point point] { get; }
        Tile GetTile(ushort x, ushort y);
        Tile GetTile(Point point);
        int TileCount { get; }
    }
    public interface IMapSpawnGroups
    {
        SpawnGroup GetSpawnGroup(int index);
        int SpawnGroupCount { get; }
    }
    public interface IMap : IMapHeader, IMapTiles, IMapSpawnGroups { }
}
