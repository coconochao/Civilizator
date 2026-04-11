using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Implements 4-way BFS (Breadth-First Search) pathfinding on a grid.
    /// Finds the shortest path from a start tile to a target tile, moving only N/E/S/W.
    /// </summary>
    public static class Pathfinding
    {
        /// <summary>
        /// Finds the shortest path from start to target using BFS.
        /// </summary>
        /// <param name="start">Starting grid position</param>
        /// <param name="target">Target grid position</param>
        /// <param name="occupancy">Grid occupancy model to check passable tiles</param>
        /// <returns>List of GridPos from start to target (inclusive), or empty list if no path exists</returns>
        public static List<GridPos> FindPath(GridPos start, GridPos target, GridOccupancy occupancy)
        {
            var path = new List<GridPos>();

            // If start equals target, return immediate path
            if (start == target)
            {
                path.Add(start);
                return path;
            }

            // If start or target is not passable, no path exists
            if (!occupancy.IsPassable(start) || !occupancy.IsPassable(target))
            {
                return path;
            }

            // BFS to find shortest path
            var queue = new Queue<GridPos>();
            var visited = new HashSet<GridPos>();
            var parent = new Dictionary<GridPos, GridPos>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == target)
                {
                    // Reconstruct path from target back to start
                    var node = target;
                    while (node != start)
                    {
                        path.Insert(0, node);
                        node = parent[node];
                    }
                    path.Insert(0, start);
                    return path;
                }

                // Explore 4-way neighbors: N, E, S, W
                var neighbors = GetFourWayNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && occupancy.IsPassable(neighbor))
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // No path found
            return path;
        }

        /// <summary>
        /// Gets the four neighboring tiles (North, East, South, West) for a given position.
        /// </summary>
        private static List<GridPos> GetFourWayNeighbors(GridPos pos)
        {
            var neighbors = new List<GridPos>(4)
            {
                new GridPos(pos.X, pos.Y - 1), // North
                new GridPos(pos.X + 1, pos.Y), // East
                new GridPos(pos.X, pos.Y + 1), // South
                new GridPos(pos.X - 1, pos.Y)  // West
            };
            return neighbors;
        }
    }
}
