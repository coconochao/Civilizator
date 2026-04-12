namespace Civilizator.Simulation
{
    /// <summary>
    /// Helper class for building construction and upgrade costs.
    /// Centralizes cost definitions as specified in SPEC.md.
    /// </summary>
    public static class BuildingCostHelper
    {
        // Construction costs
        public const int CivilBuildingBuildCost = 100;  // Logs for House, Plantation, Farm, CattleFarm, Quarry
        public const int TowerBuildCost = 100;          // Ore for Tower

        // Upgrade costs
        public const int CivilBuildingUpgradeCost = 100;  // Logs for all civil buildings
        public const int TowerUpgradeCost = 100;          // Ore for Tower upgrade

        /// <summary>
        /// Returns the cost (in resources) to build this building kind from scratch.
        /// </summary>
        public static int GetBuildCost(BuildingKind kind)
        {
            return kind switch
            {
                BuildingKind.Tower => TowerBuildCost,
                BuildingKind.Central => CivilBuildingBuildCost,
                BuildingKind.House => CivilBuildingBuildCost,
                BuildingKind.Plantation => CivilBuildingBuildCost,
                BuildingKind.Farm => CivilBuildingBuildCost,
                BuildingKind.CattleFarm => CivilBuildingBuildCost,
                BuildingKind.Quarry => CivilBuildingBuildCost,
                _ => throw new System.ArgumentException($"Unknown building kind: {kind}")
            };
        }

        /// <summary>
        /// Returns the cost (in resources) to upgrade this building kind.
        /// Follows the same logic as build cost (civil = Logs, Tower = Ore).
        /// </summary>
        public static int GetUpgradeCost(BuildingKind kind)
        {
            return kind switch
            {
                BuildingKind.Tower => TowerUpgradeCost,
                BuildingKind.Central => CivilBuildingUpgradeCost,
                BuildingKind.House => CivilBuildingUpgradeCost,
                BuildingKind.Plantation => CivilBuildingUpgradeCost,
                BuildingKind.Farm => CivilBuildingUpgradeCost,
                BuildingKind.CattleFarm => CivilBuildingUpgradeCost,
                BuildingKind.Quarry => CivilBuildingUpgradeCost,
                _ => throw new System.ArgumentException($"Unknown building kind: {kind}")
            };
        }
    }
}
