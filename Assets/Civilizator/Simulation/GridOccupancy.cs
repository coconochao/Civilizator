using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages grid occupancy by buildings and other obstacles.
    /// Tracks which tiles are passable and which are blocked.
    /// Agents ignore each other for blocking (multi-occupancy allowed).
    /// </summary>
    public class GridOccupancy
    {
        private readonly bool[] passableTiles;
        private readonly int mapWidth;
        private readonly int mapHeight;

        public GridOccupancy(int mapWidth = GridPos.MapWidth, int mapHeight = GridPos.MapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            // Initialize all tiles as passable
            passableTiles = new bool[mapWidth * mapHeight];
            for (int i = 0; i < passableTiles.Length; i++)
            {
                passableTiles[i] = true;
            }
        }

        /// <summary>
        /// Check if a tile is passable (not blocked by buildings or obstacles).
        /// </summary>
        public bool IsPassable(GridPos tile)
        {
            if (!IsWithinBounds(tile))
                return false;
            return passableTiles[GetIndex(tile)];
        }

        /// <summary>
        /// Mark a tile as impassable (blocked).
        /// </summary>
        public void BlockTile(GridPos tile)
        {
            if (IsWithinBounds(tile))
            {
                passableTiles[GetIndex(tile)] = false;
            }
        }

        /// <summary>
        /// Mark a tile as passable (unblocked).
        /// </summary>
        public void UnblockTile(GridPos tile)
        {
            if (IsWithinBounds(tile))
            {
                passableTiles[GetIndex(tile)] = true;
            }
        }

        /// <summary>
        /// Block all tiles occupied by a building.
        /// </summary>
        public void BlockBuilding(Building building)
        {
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                BlockTile(tile);
            }
        }

        /// <summary>
        /// Unblock all tiles occupied by a building.
        /// </summary>
        public void UnblockBuilding(Building building)
        {
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                UnblockTile(tile);
            }
        }

        /// <summary>
        /// Reset all tiles to passable.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < passableTiles.Length; i++)
            {
                passableTiles[i] = true;
            }
        }

        private bool IsWithinBounds(GridPos tile)
        {
            return tile.X >= 0 && tile.X < mapWidth && tile.Y >= 0 && tile.Y < mapHeight;
        }

        private int GetIndex(GridPos tile)
        {
            return tile.Y * mapWidth + tile.X;
        }
    }
}
