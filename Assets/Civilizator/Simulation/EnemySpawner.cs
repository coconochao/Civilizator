using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Spawns enemies on the map edge on a fixed cadence.
    /// 
    /// Spawn interpretation for V1:
    /// - Cycle 10: 1 enemy is spawned
    /// - Cycle 20: 1 additional enemy is spawned
    /// - Cycle 30: 1 additional enemy is spawned
    /// 
    /// In other words, the total number of spawned enemies is cumulative:
    /// total spawned by cycle N = floor(N / 10), for N >= 10.
    /// </summary>
    public class EnemySpawner
    {
        public const int FirstSpawnCycle = 10;
        public const int SpawnIntervalCycles = 10;

        private readonly SimulationClock _clock;
        private readonly List<GridPos> _edgeTiles;
        private int _spawnedEnemyCount;
        private int _edgeCursor;
        private int _lastProcessedCycle = -1;

        public EnemySpawner(SimulationClock clock)
            : this(clock, 0)
        {
        }

        public EnemySpawner(SimulationClock clock, int edgeCursorSeed)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _edgeTiles = BuildEdgeTiles();
            _spawnedEnemyCount = 0;
            if (_edgeTiles.Count == 0)
            {
                _edgeCursor = 0;
            }
            else
            {
                int normalizedSeed = edgeCursorSeed == int.MinValue ? 0 : Math.Abs(edgeCursorSeed);
                _edgeCursor = normalizedSeed % _edgeTiles.Count;
            }
        }

        /// <summary>
        /// Returns the total number of enemies that should have spawned by the given cycle.
        /// This is a cumulative count, not a per-wave count.
        /// </summary>
        public static int GetCumulativeSpawnCountForCycle(int currentCycle)
        {
            if (currentCycle < FirstSpawnCycle)
                return 0;

            return currentCycle / SpawnIntervalCycles;
        }

        /// <summary>
        /// Spawns enemies due for the current cycle.
        /// Calling this multiple times in the same cycle returns no additional enemies.
        /// </summary>
        public List<Enemy> SpawnIfDue()
        {
            var spawned = new List<Enemy>();

            int currentCycle = _clock.CurrentCycle;
            if (currentCycle == _lastProcessedCycle)
                return spawned;

            _lastProcessedCycle = currentCycle;

            int desiredSpawnCount = GetCumulativeSpawnCountForCycle(currentCycle);
            int spawnCount = desiredSpawnCount - _spawnedEnemyCount;
            if (spawnCount <= 0)
                return spawned;

            for (int i = 0; i < spawnCount; i++)
            {
                spawned.Add(new Enemy(GetNextEdgeSpawnPosition()));
            }

            _spawnedEnemyCount = desiredSpawnCount;
            return spawned;
        }

        /// <summary>
        /// Returns the next edge tile in a deterministic round-robin order.
        /// All returned tiles are on the map edge and have at least one in-bounds
        /// 4-way neighbor, which keeps them valid for later movement tasks.
        /// </summary>
        public GridPos GetNextEdgeSpawnPosition()
        {
            if (_edgeTiles.Count == 0)
                throw new InvalidOperationException("No valid edge tiles exist for enemy spawning.");

            GridPos spawnPosition = _edgeTiles[_edgeCursor % _edgeTiles.Count];
            _edgeCursor++;
            return spawnPosition;
        }

        private static List<GridPos> BuildEdgeTiles()
        {
            var tiles = new List<GridPos>();
            var seen = new HashSet<GridPos>();

            void AddEdgeTile(int x, int y)
            {
                var pos = new GridPos(x, y);
                if (IsValidEdgeSpawnTile(pos) && seen.Add(pos))
                    tiles.Add(pos);
            }

            for (int x = 0; x < GridPos.MapWidth; x++)
            {
                AddEdgeTile(x, 0);
            }

            for (int y = 1; y < GridPos.MapHeight - 1; y++)
            {
                AddEdgeTile(GridPos.MapWidth - 1, y);
            }

            for (int x = GridPos.MapWidth - 1; x >= 0; x--)
            {
                AddEdgeTile(x, GridPos.MapHeight - 1);
            }

            for (int y = GridPos.MapHeight - 2; y >= 1; y--)
            {
                AddEdgeTile(0, y);
            }

            return tiles;
        }

        private static bool IsValidEdgeSpawnTile(GridPos position)
        {
            bool onEdge = position.X == 0 ||
                          position.X == GridPos.MapWidth - 1 ||
                          position.Y == 0 ||
                          position.Y == GridPos.MapHeight - 1;

            if (!onEdge)
                return false;

            // Ensure the tile has at least one 4-way in-bounds neighbor.
            bool hasNorthNeighbor = position.Y > 0;
            bool hasSouthNeighbor = position.Y < GridPos.MapHeight - 1;
            bool hasWestNeighbor = position.X > 0;
            bool hasEastNeighbor = position.X < GridPos.MapWidth - 1;

            return hasNorthNeighbor || hasSouthNeighbor || hasWestNeighbor || hasEastNeighbor;
        }
    }
}
