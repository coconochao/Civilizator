using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Generates patrol positions for soldiers around the built area.
    /// Patrol positions are sampled from a diamond perimeter with a one-tile
    /// buffer beyond the current built area, then assigned round-robin to soldiers.
    /// </summary>
    public static class SoldierPatrolSystem
    {
        /// <summary>
        /// Gets the patrol positions around the current built area.
        /// The positions are sampled from the perimeter diamond with a one-tile
        /// buffer beyond the built area and filtered to exclude occupied building tiles.
        /// </summary>
        public static List<GridPos> GetPatrolPositions(
            Building centralBuilding,
            IEnumerable<Building> buildings)
        {
            if (centralBuilding == null)
                throw new ArgumentNullException(nameof(centralBuilding));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));
            if (centralBuilding.Kind != BuildingKind.Central)
                throw new ArgumentException("Patrol positions require the central building as the reference.", nameof(centralBuilding));

            int builtAreaRadius = BuiltAreaSystem.GetBuiltAreaRadius(centralBuilding, buildings);
            int patrolRadius = builtAreaRadius + 2;
            GridPos center = GetCentralFootprintCenter(centralBuilding);

            var occupiedTiles = new HashSet<GridPos>();
            foreach (var building in buildings)
            {
                if (building == null)
                    continue;

                var tiles = new List<GridPos>();
                building.GetOccupiedTiles(tiles);
                foreach (var tile in tiles)
                {
                    occupiedTiles.Add(tile);
                }
            }

            var results = new List<GridPos>();
            foreach (var candidate in GetDiamondPerimeter(center, patrolRadius))
            {
                if (occupiedTiles.Contains(candidate))
                    continue;

                results.Add(candidate);
            }

            return results;
        }

        /// <summary>
        /// Assigns patrol positions to soldiers in round-robin order.
        /// Returns a mapping from soldier ID to assigned patrol tile.
        /// </summary>
        public static Dictionary<int, GridPos> AssignPatrolPositions(
            IReadOnlyList<Agent> soldiers,
            Building centralBuilding,
            IEnumerable<Building> buildings)
        {
            if (soldiers == null)
                throw new ArgumentNullException(nameof(soldiers));

            var patrolPositions = GetPatrolPositions(centralBuilding, buildings);
            var assignments = new Dictionary<int, GridPos>();

            if (patrolPositions.Count == 0)
                return assignments;

            for (int i = 0; i < soldiers.Count; i++)
            {
                Agent soldier = soldiers[i];
                if (soldier == null)
                    continue;

                assignments[soldier.Id] = patrolPositions[i % patrolPositions.Count];
            }

            return assignments;
        }

        /// <summary>
        /// Returns the center tile of the central building footprint.
        /// </summary>
        private static GridPos GetCentralFootprintCenter(Building centralBuilding)
        {
            int size = centralBuilding.GetFootprintSize();
            int offset = (size - 1) / 2;
            return new GridPos(centralBuilding.Anchor.X + offset, centralBuilding.Anchor.Y + offset);
        }

        /// <summary>
        /// Samples the perimeter of a Manhattan diamond centered on the given tile.
        /// The returned positions are ordered top-to-bottom and left-to-right within each row.
        /// </summary>
        private static IEnumerable<GridPos> GetDiamondPerimeter(GridPos center, int radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius));

            var seen = new HashSet<GridPos>();
            var ordered = new List<GridPos>();
            for (int dy = -radius; dy <= radius; dy++)
            {
                int dx = radius - System.Math.Abs(dy);
                int rawY = center.Y + dy;

                if (dx == 0)
                {
                    TryAddCandidate(seen, ordered, center.X, rawY);
                }
                else
                {
                    TryAddCandidate(seen, ordered, center.X - dx, rawY);
                    TryAddCandidate(seen, ordered, center.X + dx, rawY);
                }
            }

            foreach (var tile in ordered)
            {
                yield return tile;
            }
        }

        /// <summary>
        /// Adds a candidate tile if it is within map bounds.
        /// </summary>
        private static void TryAddCandidate(HashSet<GridPos> seen, List<GridPos> ordered, int rawX, int rawY)
        {
            if (rawX < 0 || rawX >= GridPos.MapWidth || rawY < 0 || rawY >= GridPos.MapHeight)
                return;

            var candidate = new GridPos(rawX, rawY);
            if (seen.Add(candidate))
            {
                ordered.Add(candidate);
            }
        }
    }
}
