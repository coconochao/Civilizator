using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages spawning of resources from facilities (Plantation, Farm, CattleFarm).
    /// 
    /// Spawn rules:
    /// - Plantation: spawns Logs in its 2×2 footprint
    /// - Farm: spawns PlantFood in its 2×2 footprint
    /// - CattleFarm: spawns Meat in its 2×2 footprint
    /// 
    /// Base spawn rate: 1 resource per tile per cycle, if that tile has no uncollected spawned resource.
    /// Upgraded spawn rate: 2 resources per tile per cycle (implemented as two spawn attempts).
    /// 
    /// The spawner does not spawn in tiles that already contain uncollected resources (no duplicates per tile).
    /// </summary>
    public class FacilitySpawner
    {
        private readonly SimulationClock _clock;
        private int _lastSpawnCycle = -1;

        public FacilitySpawner(SimulationClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Spawn resources from facilities if a new cycle has started.
        /// Returns the list of newly spawned resources.
        /// </summary>
        public List<SpawnedResource> SpawnIfNewCycle(
            IEnumerable<Building> buildings,
            List<SpawnedResource> existingSpawned)
        {
            var newSpawned = new List<SpawnedResource>();

            // Only spawn once per cycle
            if (_clock.CurrentCycle == _lastSpawnCycle)
                return newSpawned;

            _lastSpawnCycle = _clock.CurrentCycle;

            // Get all constructed facilities
            var facilities = buildings
                .Where(b => BuildingKindHelpers.IsResourceFacility(b.Kind) && !b.IsUnderConstruction)
                .ToList();

            foreach (var facility in facilities)
            {
                var resourceKind = GetFacilityResourceKind(facility.Kind);
                int spawnAttempts = facility.UpgradeLevel > 0 ? 2 : 1;

                // Spawn resources in the facility's 2×2 footprint
                var footprintTiles = GetFootprintTiles(facility.Position);
                foreach (var tile in footprintTiles)
                {
                    for (int attempt = 0; attempt < spawnAttempts; attempt++)
                    {
                        // Only spawn if no uncollected resource already on this tile
                        bool tileOccupied = existingSpawned.Any(
                            r => r.Position == tile && !r.IsCollected && r.Kind == resourceKind);

                        if (!tileOccupied)
                        {
                            var spawned = new SpawnedResource(resourceKind, tile);
                            newSpawned.Add(spawned);
                        }
                    }
                }
            }

            return newSpawned;
        }

        /// <summary>
        /// Get the resource kind that a facility spawns.
        /// </summary>
        private static ResourceKind GetFacilityResourceKind(BuildingKind buildingKind)
        {
            return buildingKind switch
            {
                BuildingKind.Plantation => ResourceKind.Logs,
                BuildingKind.Farm => ResourceKind.PlantFood,
                BuildingKind.CattleFarm => ResourceKind.Meat,
                _ => throw new ArgumentException($"Building kind {buildingKind} is not a spawning facility")
            };
        }

        /// <summary>
        /// Get all tiles in a facility's 2×2 footprint.
        /// The position is the anchor tile (top-left).
        /// </summary>
        private static List<GridPos> GetFootprintTiles(GridPos anchor)
        {
            var tiles = new List<GridPos>();
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    tiles.Add(new GridPos(anchor.X + x, anchor.Y + y));
                }
            }
            return tiles;
        }
    }
}
