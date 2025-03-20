using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexTile : MonoBehaviour
{
    public int X; // Vị trí trong lưới (tọa độ X)
    public int Y; // Vị trí trong lưới (tọa độ Y)
    public Vector3 WorldPosition;
    public bool IsOccupied; // Ô này có nhân vật không?
    public List<HexTile> AdjacentTiles = new List<HexTile>(); // Danh sách các ô liền kề

    private HexGridInteraction hexGridInteraction;

    private void Start()
    {
        hexGridInteraction = FindObjectOfType<HexGridInteraction>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (hexGridInteraction != null)
            {
                //hexGridInteraction.HighlightHexTile(this);
            }
        }
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetAdjacentTiles(List<HexTile> adjacentTiles)
    {
        // Clear the current list
        AdjacentTiles.Clear();

        // Add all the new adjacent tiles
        AdjacentTiles.AddRange(adjacentTiles);

        // Visualize the new adjacency relationships
        VisualizeAdjacency();
    }

    public void CalculateAdjacentTiles(HexTile[,] grid)
    {
        int width = grid.GetLength(0);  // This is the X dimension (columns)
        int height = grid.GetLength(1); // This is the Y dimension (rows)

        // Clear the current list of adjacent tiles
        AdjacentTiles.Clear();

        // Get the appropriate direction set based on the x-coordinate parity
        (int dx, int dy)[] directions = X % 2 == 0 ? Pathfinding.DIRECTIONS_EVEN : Pathfinding.DIRECTIONS_ODD;

        foreach (var (dx, dy) in directions)
        {
            int neighborX = X + dx;
            int neighborY = Y + dy;

            // Check if coordinates are valid
            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
            {
                HexTile neighbor = grid[neighborX, neighborY];
                if (neighbor != null)
                {
                    AdjacentTiles.Add(neighbor);
                }
            }
        }

        // Visualize the adjacency relationships
        VisualizeAdjacency();
    }

    /// <summary>
    /// Debugs the grid by logging the tiles
    /// </summary>
    private void DebugGrid(HexTile[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        Debug.Log("Grid received by CalculateAdjacentTiles:");
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

    /// <summary>
    /// Visualizes the adjacency relationships by drawing lines between adjacent tiles
    /// </summary>
    private void VisualizeAdjacency()
    {
        foreach (var neighbor in AdjacentTiles)
        {
            Debug.DrawLine(this.WorldPosition, neighbor.WorldPosition, Color.red, 2f);
        }
    }
}
