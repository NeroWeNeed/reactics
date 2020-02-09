namespace Reactics.Battle
{
    public interface IMap
    {
        string Name { get; }
        ushort Width { get; }
        ushort Length { get; }
        int Elevation { get; }
        int TileCount { get; }
        Tile this[ushort x, ushort y] { get; }
        Tile this[Point point] { get; }
        Tile GetTile(ushort x, ushort y);
        Tile GetTile(Point point);
        SpawnGroup GetSpawnGroup(int index);
        int SpawnGroupCount { get; }
    }
}
