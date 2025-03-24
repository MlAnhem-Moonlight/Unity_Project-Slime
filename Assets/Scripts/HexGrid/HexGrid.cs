using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] public int hexWidth;
    [SerializeField] public int hexHeight;
    [SerializeField] public int hexSize;
    [SerializeField] public GameObject hexPrefab;
    [SerializeField] public HexOrientation orientation;

    public List<HexTile> HexTiles = new List<HexTile>();
    public HexTile[,] Tiles;
    public HexTile[,] AdjacentTilesGrid; // New 2D array to store adjacent tiles

    public delegate void GridGeneratedHandler();
    public event GridGeneratedHandler OnGridGenerated;

    private bool isGridGenerated = false;

    // Start is called before the first frame update
    void Start()
    {
        GenerateHexGrid2D();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;
                for (int s = 0; s < HexMatrix.Corners(orientation, hexSize).Length; s++)
                {
                    Gizmos.DrawLine(
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[s % 6],
                        centrePosition + HexMatrix.Corners(orientation, hexSize)[(s + 1) % 6]
                    );
                }
            }
        }
    }

    public List<HexTile> GetAdjacentTiles(HexTile tile)
    {
        List<HexTile> adjacentTiles = new List<HexTile>();
        foreach (HexTile adjacentTile in tile.AdjacentTiles)
        {
            if (!adjacentTile.IsOccupied)
            {
                adjacentTiles.Add(adjacentTile);
            }
        }
        return adjacentTiles;
    }
    void GenerateHexGrid2D()
    {
        Tiles = new HexTile[hexWidth, hexHeight]; // 2D array of HexTiles
        AdjacentTilesGrid = new HexTile[hexWidth, hexHeight]; // 2D array to store adjacent tiles

        // First create all tiles
        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                Vector3 centrePosition = HexMatrix.Center(hexSize, x, y, orientation) + transform.position;
                GameObject hexTileObject = Instantiate(hexPrefab, centrePosition, Quaternion.identity, transform);
                HexTile tile = hexTileObject.GetComponent<HexTile>();

                if (tile == null)
                {
                    Debug.LogError("HexPrefab does not have a HexTile component.");
                    continue;
                }

                tile.SetPosition(x, y);
                tile.WorldPosition = centrePosition;
                tile.IsOccupied = false;

                Tiles[x, y] = tile; // Assign to array
                HexTiles.Add(tile); // Add to list
            }
        }

        // Calculate adjacent tiles for each tile
        for (int y = 0; y < hexHeight; y++)
        {
            for (int x = 0; x < hexWidth; x++)
            {
                CalculateAdjacentTiles(Tiles[x, y], Tiles);
            }
        }

        isGridGenerated = true;
        //Debug.Log("Grid generation complete, invoking OnGridGenerated.");
        OnGridGenerated?.Invoke();
    }

    // New method to calculate adjacent tiles for a specific tile
    void CalculateAdjacentTiles(HexTile tile, HexTile[,] grid)
    {
        int x = tile.X;
        int y = tile.Y;
        List<HexTile> adjacentTiles = new List<HexTile>();

        // Define the neighbor coordinates based on orientation
        // For pointy top hexes
        if (orientation == HexOrientation.PointyTop)
        {
            // Neighbor offsets for pointy-top (odd-r offset)
            int[,] directions = new int[,] {
                { 1, 0 },    // East
                { 0, 1 },    // Southeast
                { -1, 1 },   // Southwest
                { -1, 0 },   // West
                { -1, -1 },  // Northwest
                { 0, -1 }    // Northeast
            };

            // Adjust offsets for even rows
            for (int i = 0; i < 6; i++)
            {
                int nx, ny;

                if (y % 2 == 0) // Even row
                {
                    nx = x + directions[i, 0];
                    ny = y + directions[i, 1];
                }
                else // Odd row
                {
                    // For odd rows, East, West stay the same, others shift right
                    if (i == 0 || i == 3) // East, West
                    {
                        nx = x + directions[i, 0];
                        ny = y + directions[i, 1];
                    }
                    else // Others shift right
                    {
                        nx = x + directions[i, 0] + 1;
                        ny = y + directions[i, 1];
                    }
                }

                if (nx >= 0 && nx < hexWidth && ny >= 0 && ny < hexHeight && grid[nx, ny] != null)
                {
                    adjacentTiles.Add(grid[nx, ny]);
                }
            }
        }
        else // Flat top hexes
        {
            // Neighbor offsets for flat-top (odd-q offset)
            int[,] directions = new int[,] {
                { 1, 0 },     // Northeast
                { 1, 1 },     // Southeast
                { 0, 1 },     // South
                { -1, 1 },    // Southwest
                { -1, 0 },    // Northwest
                { 0, -1 }     // North
            };

            // Adjust offsets for even columns
            for (int i = 0; i < 6; i++)
            {
                int nx, ny;

                if (x % 2 == 0) // Even column
                {
                    nx = x + directions[i, 0];
                    ny = y + directions[i, 1];
                }
                else // Odd column
                {
                    // For odd columns, North, South stay the same, others shift down
                    if (i == 2 || i == 5) // South, North
                    {
                        nx = x + directions[i, 0];
                        ny = y + directions[i, 1];
                    }
                    else // Others shift down
                    {
                        nx = x + directions[i, 0];
                        ny = y + directions[i, 1] - 1;
                    }
                }

                if (nx >= 0 && nx < hexWidth && ny >= 0 && ny < hexHeight && grid[nx, ny] != null)
                {
                    adjacentTiles.Add(grid[nx, ny]);
                }
            }
        }

        // Set the adjacent tiles on the current tile
        tile.SetAdjacentTiles(adjacentTiles);

        // Store the adjacent tiles in the 2D array
        AdjacentTilesGrid[x, y] = tile;

        // Debug the adjacent tiles
        //DebugAdjacentTiles(tile, adjacentTiles);
    }

    // Debug method to log adjacent tiles
    private void DebugAdjacentTiles(HexTile tile, List<HexTile> adjacentTiles)
    {
        string message = $"Tile ({tile.X}, {tile.Y}) has {adjacentTiles.Count} adjacent tiles: ";
        foreach (var adjacent in adjacentTiles)
        {
            message += $"({adjacent.X}, {adjacent.Y}) ";
        }
        Debug.Log(message);
    }

    // Debug method to visualize the grid
    private void DebugGrid(HexTile[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        //Debug.Log("Grid received by CalculateAdjacentTiles:");
        for (int y = 0; y < height; y++)
        {
            string row = "";
            for (int x = 0; x < width; x++)
            {
                HexTile tile = grid[x, y];
                if (tile != null)
                {
                    row += tile.IsOccupied ? "1 " : "0 ";
                }
                else
                {
                    row += "X "; // Indicate a null tile
                }
            }
            Debug.Log(row);
        }
    }

    public bool IsGridGenerated()
    {
        return isGridGenerated;
    }
}

public enum HexOrientation
{
    PointyTop,
    FlatTop
}
