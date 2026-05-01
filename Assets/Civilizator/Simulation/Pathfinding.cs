using System.Collections.Generic;
using System.Linq;

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
        /// Finds a path to a destination tile even when the destination itself is blocked.
        /// This is used for entering building anchors, which are occupied in the occupancy grid.
        /// If the destination is passable, this behaves like <see cref="FindPath(GridPos, GridPos, GridOccupancy)"/>.
        /// </summary>
        public static List<GridPos> FindPathToOccupiedTarget(GridPos start, GridPos target, GridOccupancy occupancy)
        {
            var directPath = FindPathIgnoringBlockedStart(start, target, occupancy);
            if (directPath.Count > 0)
                return directPath;

            if (!occupancy.IsPassable(start))
            {
                var blockedStartPath = FindPathFromBlockedStart(start, target, occupancy);
                if (blockedStartPath.Count > 0)
                    return blockedStartPath;
            }

            var bestPath = new List<GridPos>();
            int bestLength = int.MaxValue;

            foreach (var neighbor in GetFourWayNeighbors(target))
            {
                if (!occupancy.IsPassable(neighbor))
                    continue;

                var candidatePath = FindPathIgnoringBlockedStart(start, neighbor, occupancy);
                if (candidatePath.Count == 0)
                    continue;

                if (candidatePath.Count < bestLength)
                {
                    bestPath = candidatePath;
                    bestLength = candidatePath.Count;
                }
            }

            if (bestPath.Count == 0)
                return bestPath;

            bestPath.Add(target);
            return bestPath;
        }

        /// <summary>
        /// Finds a path when the start tile is inside a blocked footprint.
        /// The path first walks through blocked tiles in the connected component
        /// until it reaches a passable exit tile, then continues normally.
        /// </summary>
        private static List<GridPos> FindPathFromBlockedStart(GridPos start, GridPos target, GridOccupancy occupancy)
        {
            var result = new List<GridPos>();

            if (occupancy.IsPassable(start))
                return result;

            var queue = new Queue<GridPos>();
            var visited = new HashSet<GridPos>();
            var parent = new Dictionary<GridPos, GridPos>();

            queue.Enqueue(start);
            visited.Add(start);

            int bestLength = int.MaxValue;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in GetFourWayNeighbors(current))
                {
                    if (occupancy.IsPassable(neighbor))
                    {
                        var pathToExit = ReconstructPath(parent, start, current);
                        pathToExit.Add(neighbor);

                        var remainder = FindPathToOccupiedTarget(neighbor, target, occupancy);
                        if (remainder.Count == 0)
                            continue;

                        var fullPath = new List<GridPos>(pathToExit);
                        fullPath.AddRange(remainder.Skip(1));

                        if (fullPath.Count < bestLength)
                        {
                            result = fullPath;
                            bestLength = fullPath.Count;
                        }
                    }
                    else if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reconstructs a path from the start tile to the given current tile
        /// using the parent map built while traversing blocked tiles.
        /// </summary>
        private static List<GridPos> ReconstructPath(
            Dictionary<GridPos, GridPos> parent,
            GridPos start,
            GridPos current)
        {
            var path = new List<GridPos>();
            var node = current;
            path.Insert(0, node);

            while (node != start)
            {
                node = parent[node];
                path.Insert(0, node);
            }

            return path;
        }

        /// <summary>
        /// Finds a path while allowing the start tile to be occupied.
        /// This is useful when the agent currently stands inside a building footprint.
        /// </summary>
        private static List<GridPos> FindPathIgnoringBlockedStart(GridPos start, GridPos target, GridOccupancy occupancy)
        {
            var path = new List<GridPos>();

            if (start == target)
            {
                path.Add(start);
                return path;
            }

            if (!occupancy.IsPassable(target))
            {
                return path;
            }

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
                    var node = target;
                    while (node != start)
                    {
                        path.Insert(0, node);
                        node = parent[node];
                    }
                    path.Insert(0, start);
                    return path;
                }

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

            return path;
        }

        /// <summary>
        /// Finds the nearest reachable tile to a target position using Manhattan distance.
        /// If multiple tiles are equidistant, deterministically picks the one with lowest X first, then lowest Y.
        /// </summary>
        /// <param name="start">Starting grid position of the agent</param>
        /// <param name="targetCenter">The center position to find nearest tile to (e.g., a natural node)</param>
        /// <param name="occupancy">Grid occupancy model to check passable tiles</param>
        /// <returns>The nearest passable tile reachable from start, or null if no reachable tile exists</returns>
        public static GridPos? FindNearestReachableTile(GridPos start, GridPos targetCenter, GridOccupancy occupancy)
        {
            if (!occupancy.IsPassable(start))
            {
                return null;
            }

            var visited = new HashSet<GridPos>();
            var queue = new Queue<GridPos>();
            GridPos? nearest = null;
            int nearestDistance = int.MaxValue;

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Calculate Manhattan distance from current tile to target center
                int distance = GridPos.Manhattan(current, targetCenter);

                // Update nearest if this tile is closer, or equidistant but smaller (X, then Y)
                if (distance < nearestDistance || 
                    (distance == nearestDistance && (nearest == null || IsSmaller(current, nearest.Value))))
                {
                    nearest = current;
                    nearestDistance = distance;
                }

                // Explore 4-way neighbors
                var neighbors = GetFourWayNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && occupancy.IsPassable(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Deterministic tie-breaker: returns true if pos1 is "smaller" than pos2.
        /// Compares X first, then Y (both ascending).
        /// </summary>
        private static bool IsSmaller(GridPos pos1, GridPos pos2)
        {
            if (pos1.X != pos2.X)
                return pos1.X < pos2.X;
            return pos1.Y < pos2.Y;
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
