using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // Cache these directions to avoid recreating them each time
    private static readonly (int dx, int dy)[] DIRECTIONS_EVEN = { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) };
    private static readonly (int dx, int dy)[] DIRECTIONS_ODD = { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) };

    // Movement costs - can be adjusted based on game requirements
    private const int STRAIGHT_COST = 10;

    // Public parameters that can be adjusted in Unity Inspector
    [Tooltip("Heuristic weight - higher values prioritize paths toward the goal")]
    [Range(1, 5)]
    public int heuristicWeight = 1;

    /// <summary>
    /// Finds the shortest path between start and end hexagonal tiles
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

        // Maps for tracking
        var cameFrom = new Dictionary<HexTile, HexTile>();
        var gScore = new Dictionary<HexTile, int>();

        // Initialize scores
        gScore[start] = 0;
        int initialFScore = CalculateHeuristic(start, end);
        openSet.Add(new PathNode(start, initialFScore, 0));
        openSetTracker.Add(start);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxSearchDepth)
        {
            iterations++;

            // Get the node with lowest F score
            var currentNode = openSet.Min;
            HexTile current = currentNode.Tile;

            // Manage the open sets
            openSet.Remove(currentNode);
            openSetTracker.Remove(current);

            // Path found
            if (current == end)
            {
                return ReconstructPath(cameFrom, end);
            }

            // Get the appropriate direction set based on the x-coordinate parity
            (int dx, int dy)[] directions = current.X % 2 == 0 ? DIRECTIONS_EVEN : DIRECTIONS_ODD;

            // Explore all possible neighbors
            foreach (var (dx, dy) in directions)
            {
                int neighborX = current.X + dx;
                int neighborY = current.Y + dy;

                // Check if coordinates are valid
                if (neighborX < 0 || neighborX >= rows || neighborY < 0 || neighborY >= cols)
                    continue;

                HexTile neighbor = grid[neighborX, neighborY];

                // Skip occupied tiles
                if (neighbor.IsOccupied)
                    continue;

                // Apply movement cost - this can be expanded to consider terrain types
                int movementCost = STRAIGHT_COST;

                // Calculate new path score
                int tentativeGScore = gScore[current] + movementCost;

                // If this path is better than any previously found
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    // Update the best path
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    // Calculate and add to open set if not already there
                    int fScore = tentativeGScore + CalculateHeuristic(neighbor, end);

                    if (!openSetTracker.Contains(neighbor))
                    {
                        var newNode = new PathNode(neighbor, fScore, tentativeGScore);
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

                        openSet.Add(new PathNode(neighbor, fScore, tentativeGScore));
                    }
                }
            }
        }

        // No path found or depth limit reached
        return null;
    }

    /// <summary>
    /// Calculates the heuristic distance between two hex tiles
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
        return Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz))) * 10;
    }

    /// <summary>
    /// Reconstructs the path from end to start using the cameFrom map
    /// </summary>
    private static List<HexTile> ReconstructPath(Dictionary<HexTile, HexTile> cameFrom, HexTile end)
    {
        var path = new List<HexTile>();
        HexTile current = end;

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
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
    /// Comparer for PathNode to be used in the priority queue
    /// </summary>
    private class PathNodeComparer : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            // Compare by F score first
            int fComparison = x.FScore.CompareTo(y.FScore);
            if (fComparison != 0)
                return fComparison;

            // If F scores are equal, compare by G score (prefer higher G scores)
            int gComparison = y.GScore.CompareTo(x.GScore);
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
