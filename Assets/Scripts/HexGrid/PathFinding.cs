using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // Define the directional offsets for hexagonal grid
    static readonly (int, int)[] DIRECTIONS_EVEN = { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) };
    static readonly (int, int)[] DIRECTIONS_ODD = { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) };

    public static List<HexTile> FindPath(HexTile[,] grid, HexTile start, HexTile end)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        var openSet = new SortedSet<(int fScore, int count, HexTile tile)>(Comparer<(int, int, HexTile)>.Create((a, b) =>
        {
            if (a.Item1 != b.Item1) return a.Item1.CompareTo(b.Item1);
            return a.Item2.CompareTo(b.Item2);
        }));

        var cameFrom = new Dictionary<HexTile, HexTile>();
        var gScore = new Dictionary<HexTile, int> { [start] = 0 };
        var fScore = new Dictionary<HexTile, int> { [start] = Heuristic(start, end) };

        openSet.Add((fScore[start], 0, start));

        while (openSet.Count > 0)
        {
            HexTile current = openSet.Min.tile;
            openSet.Remove(openSet.Min);

            if (current == end)
            {
                // Reconstruct path
                var path = new List<HexTile>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            (int, int)[] directions = current.X % 2 == 0 ? DIRECTIONS_EVEN : DIRECTIONS_ODD;

            foreach ((int dx, int dy) in directions)
            {
                int neighborX = current.X + dx;
                int neighborY = current.Y + dy;

                // Kiểm tra tọa độ hợp lệ
                if (neighborX < 0 || neighborX >= rows || neighborY < 0 || neighborY >= cols)
                    continue;

                HexTile neighbor = grid[neighborX, neighborY];

                // Kiểm tra ô đã bị chiếm hay chưa
                if (neighbor.IsOccupied)
                    continue;

                int tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                    openSet.Add((fScore[neighbor], openSet.Count, neighbor));
                }
            }
        }

        return null; // No path found
    }

    static int Heuristic(HexTile a, HexTile b)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return dx + Math.Max(0, (dy - dx) / 2); // Cân bằng khoảng cách trên lưới lục giác
    }

}
