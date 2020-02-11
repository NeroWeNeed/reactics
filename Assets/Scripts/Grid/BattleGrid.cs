using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrid : ScriptableObject
{

    [SerializeField]
    private Vector2Int size;
    [SerializeField]
    private BattleGridTile[] tiles;

    public IList<BattleGridTile> Tiles
    {
        get;
        private set;
    }


    public Vector2Int Size
    {
        get => size;
    }

    public int Width
    {
        get => size.x;
    }
    public int Height
    {
        get => size.y;
    }
    private void Start()
    {
        Tiles = Array.AsReadOnly(tiles);
    }
    public BattleGridTile GetTile(Vector2Int coordinates)
    {
        return GetTile(coordinates.x, coordinates.y);
    }
    public BattleGridTile GetTile(int x, int y)
    {
        if (x >= 0 && x < Width)
        {
            if (y >= 0 && y < Height)
            {
                return Tiles[y * (Width + 1) + x];
            }
            else
                throw new ArgumentOutOfRangeException($"Y Coordinate '{y}' must be within range 0 (inclusive) and {Height} exclusive.");
        }
        else
            throw new ArgumentOutOfRangeException($"X Coordinate '{x}' must be within range 0 (inclusive) and {Width} exclusive.");
    }

}