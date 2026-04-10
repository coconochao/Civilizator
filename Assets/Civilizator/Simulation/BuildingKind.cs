namespace Civilizator.Simulation
{
    /// <summary>
    /// Types of buildings that can be placed in the world.
    /// Each building kind has an associated footprint size.
    /// </summary>
    public enum BuildingKind
    {
        Central,
        House,
        Tower,
        Plantation,
        Farm,
        CattleFarm,
        Quarry
    }

    /// <summary>
    /// Helpers for building kinds.
    /// </summary>
    public static class BuildingKindHelpers
    {
        /// <summary>
        /// Returns the footprint size (width and height in tiles) for the given building kind.
        /// Central building is 3×3; all others are 2×2.
        /// </summary>
        public static int GetFootprintSize(BuildingKind kind)
        {
            return kind switch
            {
                BuildingKind.Central => 3,
                BuildingKind.House => 2,
                BuildingKind.Tower => 2,
                BuildingKind.Plantation => 2,
                BuildingKind.Farm => 2,
                BuildingKind.CattleFarm => 2,
                BuildingKind.Quarry => 2,
                _ => throw new System.ArgumentException($"Unknown building kind: {kind}")
            };
        }
    }
}
