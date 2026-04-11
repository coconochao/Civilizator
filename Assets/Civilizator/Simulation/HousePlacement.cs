namespace Civilizator.Simulation
{
    /// <summary>
    /// Finds the nearest empty tile for house placement relative to the central building.
    /// </summary>
    public static class HousePlacement
    {
        /// <summary>
        /// Find the nearest empty tile where a house can be placed, relative to the central building.
        /// Uses Manhattan distance to search in expanding rings from the central anchor.
        /// Returns the anchor position for a valid 2×2 house, or null if no valid placement exists.
        /// </summary>
        public static GridPos? FindNearestEmptyTile(
            GridPos centralAnchor,
            System.Collections.Generic.IEnumerable<Building> buildings)
        {
            if (!centralAnchor.IsInBounds())
                return null;

            int mapWidth = GridPos.MapWidth;
            int mapHeight = GridPos.MapHeight;
            int houseSize = BuildingKindHelpers.GetFootprintSize(BuildingKind.House);

            // Start searching from Manhattan distance 0 and expand outward
            // We'll check all tiles at each distance level in a consistent order (top-left to bottom-right)
            for (int distance = 0; distance < mapWidth + mapHeight; distance++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        GridPos candidate = new GridPos(x, y);
                        int manhattanDist = GridPos.Manhattan(centralAnchor, candidate);
                        
                        if (manhattanDist != distance)
                            continue;

                        // Check if a house can be placed at this anchor position
                        if (BuildingPlacementValidator.CanPlaceBuilding(buildings, BuildingKind.House, candidate))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }
    }
}
