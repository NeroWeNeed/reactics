using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class BattleGridManager : MonoBehaviour
{
    [SerializeField]
    private BattleGrid grid;

    GameObject Map;

    public BattleGrid Grid
    {
        get => grid;
    }
    [SerializeField]
    private int tileSize = 10;

    [SerializeField]
    private Texture2D defaultTexture;
    public int TileSize
    {
        get => tileSize;
    }
    [SerializeField]
    private Texture2D tilemap;

    public Texture2D Tilemap
    {
        get => Tilemap;
    }

    public int RealWidth
    {
        get => grid.Width * tileSize;
    }
    public int RealHeight
    {
        get => grid.Height * tileSize;
    }

    public Vector3 Center()
    {
        return new Vector3(transform.position.x + RealWidth / 2, transform.position.y, transform.position.z + RealHeight / 2);
    }

    void Start()
    {
        //mapRenderer = gameObject.AddComponent<Reactics.Battle.Map.MapRenderer>() as Reactics.Battle.Map.MapRenderer;
        //mapRenderer.Map.Width = 128;
        //GetComponent<MapRenderer>().GenerateMesh();
        //GetComponent<MeshFilter>().mesh = GenerateMesh(grid);
        //GetComponent<Renderer>().sharedMaterial.mainTexture = GenerateTexture(grid);
    }
    private void OnRenderObject()
    {

        //GetComponent<MeshFilter>().mesh = GenerateMesh(grid);
    }
    private Mesh GenerateMesh(BattleGrid grid)
    {

        int vertexCount = (grid.Width + 1) * (grid.Height + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        int[] triangles = new int[grid.Width * grid.Height * 6];
        int x, y, index;
        for (y = 0; y <= grid.Height; y++)
        {
            for (x = 0; x <= grid.Width; x++)
            {
                index = y * (grid.Width + 1) + x;
                vertices[index] = new Vector3(x * TileSize, 0, y * TileSize);
                uv[index] = new Vector2((float)x / (grid.Width), (float)y / (grid.Height));
                normals[index] = Vector3.up;
            }
        }
        for (y = 0; y < grid.Height; y++)
        {
            for (x = 0; x < grid.Width; x++)
            {
                index = (y * grid.Width + x) * 6;
                triangles[index] = y * (grid.Width + 1) + x;
                triangles[index + 1] = y * (grid.Width + 1) + x + (grid.Width + 1);
                triangles[index + 2] = y * (grid.Width + 1) + x + (grid.Width + 2);
                triangles[index + 3] = y * (grid.Width + 1) + x;
                triangles[index + 4] = y * (grid.Width + 1) + x + (grid.Width + 2);
                triangles[index + 5] = y * (grid.Width + 1) + x + 1;
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.normals = normals;

        return mesh;


        /*
        int vertexCount = (size.x + 1) * (size.z + 1);
        tiles = new ProceduralGridTile[size.x * size.y * size.z];

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        textures = new Texture2D[size.y];
        int[] triangles = new int[size.x * size.z * 6];
        int x, y, z, index;

        for (index = 0; index < tiles.Length; index++)
        {
            if (tileCollection.tiles[index] == null)
                tiles[index] = ProceduralGridTile.CreateInstance<ProceduralGridTile>();
            else
                tiles[index] = tileCollection.tiles[index];
        }

        for (z = 0; z <= size.z; z++)
        {
            for (x = 0; x <= size.x; x++)
            {
                index = z * (size.z + 1) + x;
                vertices[index] = new Vector3(x * tileSize, 0, z * tileSize);
                uv[index] = new Vector2((float)x / (size.x), (float)z / (size.z));
                normals[index] = Vector3.up;
            }
        }
        for (z = 0; z < size.z; z++)
        {
            for (x = 0; x < size.x; x++)
            {
                index = (z * size.x + x) * 6;
                triangles[index] = z * (size.x + 1) + x;
                triangles[index + 1] = z * (size.x + 1) + x + (size.x + 1);
                triangles[index + 2] = z * (size.x + 1) + x + (size.x + 2);
                triangles[index + 3] = z * (size.x + 1) + x;
                triangles[index + 4] = z * (size.x + 1) + x + (size.x + 2);
                triangles[index + 5] = z * (size.x + 1) + x + 1;
            }
        }

        for (y = 0; y < size.y; y++)
        {
            Texture2D texture = new Texture2D(tilemap.width / 4 * size.x, tilemap.height / 4 * size.z, tilemap.format, false);
            ProceduralGridTileType type;
            Vector2Int tilemapTileSize = new Vector2Int(tilemap.width / 4, tilemap.height / 4);
            for (z = 0; z < size.z; z++)
            {
                for (x = 0; x < size.x; x++)
                {
                    type = tiles[(y * (size.x) * (size.z)) + z * (size.z) + x].type;
                    Graphics.CopyTexture(tilemap, 0, 0, tilemapTileSize.x * (((int)type) % 4), tilemapTileSize.y * (((int)type) / 4), tilemapTileSize.x, tilemapTileSize.y, texture, 0, 0, x * tilemapTileSize.x, z * tilemapTileSize.y);
                }
            }
            textures[y] = texture;
        }
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.normals = normals;
        meshFilter.mesh = mesh;
        redraw(this, CurrentDepth);

        GetComponent<MeshCollider>().sharedMesh = mesh;

        OnLayerChange.AddListener(redraw);
        */

    }


    private Texture2D GenerateTexture(BattleGrid grid)
    {

        //Get Texture Tile Size
        int size = -1;
        int tempSize;
        System.Nullable<UnityEngine.TextureFormat> format = null;
        TextureFormat tempFormat;
        if (grid.Tiles != null)
        {
            foreach (var item in grid.Tiles)
            {
                if (item == null)
                    continue;
                tempSize = System.Math.Min(item.Texture.width, item.Texture.height);
                tempFormat = item.Texture.format;
                if (size == -1)
                    size = tempSize;
                else if (size != tempSize)
                {
                    throw new System.ArgumentException("Tile Sizes are different");
                };
                if (!format.HasValue)
                {
                    format = tempFormat;
                }
                else if (format.Value != tempFormat)
                {
                    throw new System.ArgumentException("Tile Formats are different");
                };
            }
        }
        if (size == -1)
            size = 64;
        if (!format.HasValue)
            format = defaultTexture.format;

        Texture2D texture = new Texture2D(size * grid.Width, size * grid.Height, format.Value, false);
        Texture2D tempTexture;
        int x, y, variation = 0, verticalOrientation = 0, horizontalOrientation = 0, index;
        BattleGridTile tile;
        for (y = 0; y < grid.Height; y++)
        {
            for (x = 0; x < grid.Width; x++)
            {
                index = y * (grid.Width + 1) + x;
                if (grid.Tiles != null && index < grid.Tiles.Count)
                {
                    tile = grid.Tiles[index];
                    variation = Random.Range(1, (int)System.Math.Max(tile.Texture.width, tile.Texture.height) / System.Math.Min(tile.Texture.width, tile.Texture.height)) - 1;
                    verticalOrientation = (tile.Texture.width < tile.Texture.height) ? 1 : 0;
                    horizontalOrientation = (tile.Texture.width > tile.Texture.height) ? 1 : 0;
                    tempTexture = tile.Texture;
                }
                else
                {
                    tempTexture = defaultTexture;
                }
                Graphics.CopyTexture(tempTexture, 0, 0, size * variation * horizontalOrientation, size * verticalOrientation * variation, size, size, texture, 0, 0, x * size, y * size);
            }

        }
        return texture;


    }

}
