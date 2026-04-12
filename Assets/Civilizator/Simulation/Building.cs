namespace Civilizator.Simulation
{
    /// <summary>
    /// Represents a building instance in the world.
    /// Tracks position, kind, upgrade level, and construction progress.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// The position (anchor tile) of this building.
        /// </summary>
        public GridPos Position { get; }

        /// <summary>
        /// The kind of building (Central, House, Tower, etc.).
        /// </summary>
        public BuildingKind Kind { get; }

        /// <summary>
        /// The upgrade level: 0 (none) or 1 (max upgrade in V1).
        /// </summary>
        public int UpgradeLevel { get; set; }

        /// <summary>
        /// Whether this building is currently under construction.
        /// </summary>
        public bool IsUnderConstruction { get; set; }

        /// <summary>
        /// Construction progress: total delivered build resources (integer).
        /// For a new building: progress goes from 0 to required cost.
        /// For an upgrade: progress goes from 0 to required upgrade cost.
        /// </summary>
        public int ConstructionProgress { get; set; }

        /// <summary>
        /// Creates a new building instance at the given position with the given kind.
        /// Initial state: upgrade level 0, not under construction, progress 0.
        /// </summary>
        public Building(GridPos position, BuildingKind kind)
        {
            Position = position;
            Kind = kind;
            UpgradeLevel = 0;
            IsUnderConstruction = false;
            ConstructionProgress = 0;
        }

        /// <summary>
        /// Returns the resource kind required to construct or upgrade this building.
        /// Civil buildings (House, Plantation, Farm, CattleFarm, Quarry) require Logs.
        /// Tower requires Ore.
        /// Central building requires Logs (though construction of Central is not typical in gameplay).
        /// </summary>
        public ResourceKind GetConstructionResourceKind()
        {
            return Kind switch
            {
                BuildingKind.Tower => ResourceKind.Ore,
                _ => ResourceKind.Logs
            };
        }

        /// <summary>
        /// Returns the required amount of resources to complete the next construction phase.
        /// - New building: returns cost for construction from level 0.
        /// - Upgrade: returns cost for upgrade from current level to next.
        /// 
        /// Returns 0 if no further construction is possible (e.g., already at max upgrade).
        /// </summary>
        public int GetRequiredConstructionAmount()
        {
            if (IsUnderConstruction)
            {
                // Currently under construction (initial build)
                return BuildingCostHelper.GetBuildCost(Kind);
            }

            // Check if we can still upgrade
            if (UpgradeLevel < 1)
            {
                return BuildingCostHelper.GetUpgradeCost(Kind);
            }

            // Already at max upgrade, no more construction needed
            return 0;
        }

        /// <summary>
        /// Increments construction progress by the given amount.
        /// Progress is capped at the required amount (never exceeds it).
        /// </summary>
        public void DeliverBuildResources(int amount)
        {
            if (amount < 0)
                throw new System.ArgumentException("Delivery amount must be non-negative.");

            int required = GetRequiredConstructionAmount();
            if (required == 0)
                return; // No construction in progress

            ConstructionProgress = System.Math.Min(ConstructionProgress + amount, required);
        }
    }
}
