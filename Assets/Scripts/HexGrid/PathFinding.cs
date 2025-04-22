using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // Cache these directions to avoid recreating them each time
    public static readonly (int dx, int dy)[] DIRECTIONS_EVEN = { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) };
    public static readonly (int dx, int dy)[] DIRECTIONS_ODD = { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) };

    // Movement costs - explicitly set to 1 to prioritize fewest cells
    private const int MOVEMENT_COST = 1;

    // Public parameters that can be adjusted in Unity Inspector
    [Tooltip("Heuristic weight - higher values prioritize paths toward the goal")]
    [Range(1, 5)]
    public int heuristicWeight = 2; // Increased to prioritize direct paths

    /// <summary>
    /// Finds the shortest path between start and end hexagonal tiles using A* algorithm
    /// Optimized to prioritize paths with fewest cells
    /// </summary>
    /// <param name="grid">2D array of hexagonal tiles</param>
    /// <param name="start">Starting tile</param>
    /// <param name="end">Target tile</param>
    /// <param name="maxSearchDepth">Optional parameter to limit search depth (defaults to int.MaxValue)</param>
    /// <returns>List of tiles representing the path or null if no path exists</returns>
    public static List<HexTile> FindPath(HexTile[,] grid, HexTile start, HexTile end, int maxSearchDepth = int.MaxValue)
    {
        if (grid == null || start == null || end == null)
            return null;

        // Debug the matrix
        DebugMatrix(grid);

        // If start or end tiles are occupied, no path is possible
        if (end.IsOccupied)
            return null;

        // If start and end are the same, return just the start tile
        if (start == end)
            return new List<HexTile> { start };

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Priority queue for open set with efficient ordering
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var openSetTracker = new HashSet<HexTile>(); // For O(1) Contains() operations
        var closedSet = new HashSet<HexTile>(); // Track already evaluated nodes

        // Maps for tracking
        var cameFrom = new Dictionary<HexTile, HexTile>();
        var gScore = new Dictionary<HexTile, int>();
        var fScore = new Dictionary<HexTile, int>();

        // Initialize scores
        gScore[start] = 0;
        int initialFScore = CalculateHeuristic(start, end);
        fScore[start] = initialFScore;
        openSet.Add(new PathNode(start, initialFScore, 0));
        openSetTracker.Add(start);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxSearchDepth)
        {
            iterations++;

            // Get the node with lowest F score
            var currentNode = openSet.Min;
            HexTile current = currentNode.Tile;

            // Path found
            if (current == end)
            {
                //Debug.Log($"Path found with {gScore[current]} steps");
                var path = ReconstructPath(cameFrom, start, end);
                return path;
            }

            // Manage the open sets
            openSet.Remove(currentNode);
            openSetTracker.Remove(current);
            closedSet.Add(current);

            // Explore all possible neighbors
            foreach (var neighbor in GetValidNeighbors(current, grid, closedSet))
            {
                // Skip occupied tiles and already processed tiles
                if (neighbor.IsOccupied || closedSet.Contains(neighbor))
                    continue;

                // Calculate new path score - always increment by 1 to ensure fewest cells
                int tentativeGScore = gScore[current] + MOVEMENT_COST;

                // If this path is better than any previously found or node not yet discovered
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    // Update the best path
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    // Calculate F score with weighted heuristic to prioritize direct paths
                    int newFScore = tentativeGScore + CalculateHeuristic(neighbor, end) * 2;
                    fScore[neighbor] = newFScore;

                    if (!openSetTracker.Contains(neighbor))
                    {
                        var newNode = new PathNode(neighbor, newFScore, tentativeGScore);
                        openSet.Add(newNode);
                        openSetTracker.Add(neighbor);
                    }
                    else
                    {
                        // Update existing node with new scores
                        var existingNodes = new List<PathNode>();
                        foreach (var node in openSet)
                        {
                            if (node.Tile == neighbor)
                                existingNodes.Add(node);
                        }

                        foreach (var oldNode in existingNodes)
                        {
                            openSet.Remove(oldNode);
                        }

                        openSet.Add(new PathNode(neighbor, newFScore, tentativeGScore));
                    }
                }
            }
        }

        // No path found or depth limit reached
        Debug.LogWarning($"No path found after {iterations} iterations");
        return null;
    }

    public static List<HexTile> FindingPathOccupied(HexTile[,] grid, HexTile start, HexTile end, int maxSearchDepth = int.MaxValue)
    {
        Debug.Log("Finding path with occupied tiles start: ["+ start.X+","+start.Y + "]   [" + end.X+","+end.Y+"]");
        if (grid == null || start == null || end == null)
            return null;

        // Debug the matrix
        DebugMatrix(grid);

        // If start and end are the same, return just the start tile
        if (start == end)
            return new List<HexTile> { start };

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Priority queue for open set with efficient ordering
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var openSetTracker = new HashSet<HexTile>(); // For O(1) Contains() operations
        var closedSet = new HashSet<HexTile>(); // Track already evaluated nodes

        // Maps for tracking
        var cameFrom = new Dictionary<HexTile, HexTile>();
        var gScore = new Dictionary<HexTile, int>();
        var fScore = new Dictionary<HexTile, int>();

        // Initialize scores
        gScore[start] = 0;
        int initialFScore = CalculateHeuristic(start, end);
        fScore[start] = initialFScore;
        openSet.Add(new PathNode(start, initialFScore, 0));
        openSetTracker.Add(start);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxSearchDepth)
        {
            iterations++;

            // Get the node with lowest F score
            var currentNode = openSet.Min;
            HexTile current = currentNode.Tile;

            // Path found
            if (current == end)
            {
                var path = ReconstructPath(cameFrom, start, end);
                return path;
            }

            // Manage the open sets
            openSet.Remove(currentNode);
            openSetTracker.Remove(current);
            closedSet.Add(current);

            // Explore all possible neighbors
            foreach (var neighbor in GetValidNeighbors(current, grid, closedSet))
            {
                // Skip already processed tiles
                if (closedSet.Contains(neighbor))
                    continue;

                // Calculate new path score - always increment by 1 to ensure fewest cells
                int tentativeGScore = gScore[current] + MOVEMENT_COST;

                // If this path is better than any previously found or node not yet discovered
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    // Update the best path
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    // Calculate F score with weighted heuristic to prioritize direct paths
                    int newFScore = tentativeGScore + CalculateHeuristic(neighbor, end) * 2;
                    fScore[neighbor] = newFScore;

                    if (!openSetTracker.Contains(neighbor))
                    {
                        var newNode = new PathNode(neighbor, newFScore, tentativeGScore);
                        openSet.Add(newNode);
                        openSetTracker.Add(neighbor);
                    }
                    else
                    {
                        // Update existing node with new scores
                        var existingNodes = new List<PathNode>();
                        foreach (var node in openSet)
                        {
                            if (node.Tile == neighbor)
                                existingNodes.Add(node);
                        }

                        foreach (var oldNode in existingNodes)
                        {
                            openSet.Remove(oldNode);
                        }

                        openSet.Add(new PathNode(neighbor, newFScore, tentativeGScore));
                    }
                }
            }
        }

        // No path found or depth limit reached
        Debug.LogWarning($"No path found after {iterations} iterations");
        return null;
    }

    /// <summary>
    /// Gets valid neighbors for a hex tile, filtering out invalid positions
    /// </summary>
    private static List<HexTile> GetValidNeighbors(HexTile current, HexTile[,] grid, HashSet<HexTile> closedSet)
    {
        var neighbors = new List<HexTile>();

        // Just return the pre-calculated adjacent tiles if available
        if (current.AdjacentTiles != null && current.AdjacentTiles.Count > 0)
        {
            foreach (var neighbor in current.AdjacentTiles)
            {
                if (neighbor != null && !neighbor.IsOccupied && !closedSet.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            return neighbors;
        }

        // Fallback: Calculate adjacent tiles manually using directions
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Choose direction set based on y-coordinate parity
        var directions = current.Y % 2 == 0 ? DIRECTIONS_EVEN : DIRECTIONS_ODD;

        foreach (var dir in directions)
        {
            int nx = current.X + dir.dx;
            int ny = current.Y + dir.dy;

            // Check if within grid bounds
            if (nx >= 0 && nx < rows && ny >= 0 && ny < cols)
            {
                HexTile neighbor = grid[nx, ny];
                if (neighbor != null && !neighbor.IsOccupied && !closedSet.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Calculates the heuristic distance between two hex tiles
    /// Enhanced to better prioritize direct paths
    /// </summary>
    private static int CalculateHeuristic(HexTile a, HexTile b)
    {
        // Convert to cube coordinates for better hex distance calculation
        int ax = a.X;
        int ay = a.Y;
        int az = -ax - ay;

        int bx = b.X;
        int by = b.Y;
        int bz = -bx - by;

        // Manhattan distance in cube coordinates
        return Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz)));
    }

    /// <summary>
    /// Reconstructs the path from end to start using the cameFrom map
    /// </summary>
    private static List<HexTile> ReconstructPath(Dictionary<HexTile, HexTile> cameFrom, HexTile start, HexTile end)
    {
        var path = new List<HexTile>();
        HexTile current = end;

        while (current != start)
        {
            path.Add(current);
            if (!cameFrom.ContainsKey(current))
            {
                Debug.LogError("Path reconstruction error: broken path chain");
                return null;
            }
            current = cameFrom[current];
        }
        path.Add(start); // Add the start tile at the end

        path.Reverse();

        // Debug path length
        //Debug.Log($"Final path contains {path.Count} tiles from {start.X},{start.Y} to {end.X},{end.Y}");

        return path;
    }

    /// <summary>
    /// Debugs the matrix by logging the tiles
    /// </summary>
    private static void DebugMatrix(HexTile[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        //Debug.Log($"Pathfinding on grid with dimensions: {rows}x{cols}");
    }

    /// <summary>
    /// Represents a node in the pathfinding algorithm
    /// </summary>
    private class PathNode
    {
        public HexTile Tile { get; }
        public int FScore { get; }
        public int GScore { get; }

        public PathNode(HexTile tile, int fScore, int gScore)
        {
            Tile = tile;
            FScore = fScore;
            GScore = gScore;
        }
    }

    /// <summary>
    /// Enhanced comparer for PathNode to prioritize paths with fewest cells
    /// </summary>
    private class PathNodeComparer : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            // Compare by F score first (lowest F score is prioritized)
            int fComparison = x.FScore.CompareTo(y.FScore);
            if (fComparison != 0)
                return fComparison;

            // If F scores are equal, prefer paths with lower G scores
            // (This is the opposite of the original implementation and 
            // ensures we prioritize shorter paths)
            int gComparison = x.GScore.CompareTo(y.GScore);
            if (gComparison != 0)
                return gComparison;

            // If still equal, compare by reference to ensure stability
            return x.Tile.GetHashCode().CompareTo(y.Tile.GetHashCode());
        }
    }

    /// <summary>
    /// Get path as Vector3 positions for visualization or movement
    /// </summary>
    public static List<Vector3> GetPathPositions(List<HexTile> path)
    {
        if (path == null || path.Count == 0)
            return null;

        var positions = new List<Vector3>();
        foreach (var tile in path)
        {
            positions.Add(tile.transform.position);
        }

        return positions;
    }
}