namespace Civilizator.Simulation
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a building instance in the world.
    /// A building occupies a footprint starting at an anchor position.
    /// For houses, also tracks resident capacity (adults and children).
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Unique identifier for this building.
        /// Assigned at creation and never changes.
        /// </summary>
        public int Id { get; }

        public BuildingKind Kind { get; }
        public GridPos Anchor { get; }
        public bool IsUnderConstruction { get; set; }
        public int UpgradeLevel { get; set; }

        /// <summary>
        /// Current hit points for the building.
        /// Towers and the central building initialize this to the shared combat HP value.
        /// Other building types default to 0 for now and can be set by later combat tasks.
        /// </summary>
        public int HitPoints { get; set; }

        /// <summary>
        /// Construction progress: total delivered build resources (integer).
        /// For a new building: progress goes from 0 to required cost.
        /// For an upgrade: progress goes from 0 to required upgrade cost.
        /// </summary>
        public int ConstructionProgress { get; set; }

        /// <summary>
        /// Build-time end timestamp (simulation time in seconds).
        /// The building's current construction phase is not complete until:
        /// 1. ConstructionProgress >= required amount, AND
        /// 2. SimulationClock.TotalSimulationSeconds >= BuildTimeEndSeconds
        /// 
        /// Set to 0 when no build-time is active.
        /// </summary>
        public float BuildTimeEndSeconds { get; set; }

        /// <summary>
        /// Reference to the simulation clock for build-time calculations.
        /// Can be null if only progress-based completion is used.
        /// </summary>
        public SimulationClock SimulationClock { get; set; }

        /// <summary>
        /// IDs of adult and elder agents assigned to this house.
        /// Only used for houses; max capacity is 2.
        /// </summary>
        public List<int> AdultResidentIds { get; } = new List<int>();

        /// <summary>
        /// IDs of child agents assigned to this house.
        /// Only used for houses; unlimited capacity.
        /// </summary>
        public List<int> ChildResidentIds { get; } = new List<int>();

        /// <summary>
        /// Tracks whether this house has already had residents assigned upon completion.
        /// Used to prevent duplicate assignment when the house becomes complete.
        /// Only meaningful for houses.
        /// </summary>
        public bool WasAssignedOnCompletion { get; set; } = false;

        public Building(BuildingKind kind, GridPos anchor, int id = 0)
        {
            Id = id;
            Kind = kind;
            Anchor = anchor;
            IsUnderConstruction = false;
            UpgradeLevel = 0;
            HitPoints = kind == BuildingKind.Tower || kind == BuildingKind.Central
                ? TowerCombatSystem.TowerMaxHitPoints
                : 0;
            ConstructionProgress = 0;
            BuildTimeEndSeconds = 0f;
            SimulationClock = null;
        }

        /// <summary>
        /// Get the footprint size (width and height) for this building.
        /// </summary>
        public int GetFootprintSize() => BuildingKindHelpers.GetFootprintSize(Kind);

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
        /// 
        /// If this delivery completes the required amount, a build-time is scheduled:
        /// build_time (seconds) = delivered_units * (1 / productivity_multiplier)
        /// 
        /// The building's construction phase is not complete until both:
        /// 1. Progress >= required amount
        /// 2. The scheduled build-time expires
        /// 
        /// If SimulationClock is not set, completion gates are based on progress alone.
        /// </summary>
        public void DeliverBuildResources(int amount, float productivityMultiplier = 1f)
        {
            if (amount < 0)
                throw new System.ArgumentException("Delivery amount must be non-negative.");
            
            if (productivityMultiplier <= 0)
                throw new System.ArgumentException("Productivity multiplier must be positive.");

            int required = GetRequiredConstructionAmount();
            if (required == 0)
                return; // No construction in progress

            int previousProgress = ConstructionProgress;
            ConstructionProgress = System.Math.Min(ConstructionProgress + amount, required);

            // If this delivery completed the required amount and we have a clock, schedule build-time
            if (SimulationClock != null && previousProgress < required && ConstructionProgress >= required)
            {
                // Build-time = delivered_units * (1 / productivity_multiplier)
                float buildTimeSeconds = amount * (1f / productivityMultiplier);
                float currentSimTime = SimulationClock.TotalSimulationSeconds;
                BuildTimeEndSeconds = currentSimTime + buildTimeSeconds;
            }
        }

        /// <summary>
        /// Check if the building's current construction phase is complete.
        /// 
        /// Completion requires:
        /// 1. ConstructionProgress >= required amount
        /// 2. Build-time (if scheduled) has expired (if SimulationClock is set)
        /// </summary>
        public bool IsConstructionPhaseComplete()
        {
            int required = GetRequiredConstructionAmount();
            
            // No construction in progress
            if (required == 0)
                return true;

            // Progress not yet met
            if (ConstructionProgress < required)
                return false;

            // Progress met, check if build-time is satisfied (if clock is available)
            if (SimulationClock != null && BuildTimeEndSeconds > 0f)
            {
                float currentSimTime = SimulationClock.TotalSimulationSeconds;
                return currentSimTime >= BuildTimeEndSeconds;
            }

            // No build-time gate active or no clock set; completion based on progress alone
            return true;
        }

        /// <summary>
        /// Check if a given tile is occupied by this building's footprint.
        /// The building occupies tiles from anchor to anchor + footprint size.
        /// </summary>
        public bool OccupiesTile(GridPos tile)
        {
            int size = GetFootprintSize();
            return tile.X >= Anchor.X && tile.X < Anchor.X + size &&
                   tile.Y >= Anchor.Y && tile.Y < Anchor.Y + size;
        }

        /// <summary>
        /// Get all tiles occupied by this building.
        /// </summary>
        public void GetOccupiedTiles(System.Collections.Generic.List<GridPos> result)
        {
            result.Clear();
            int size = GetFootprintSize();
            for (int x = Anchor.X; x < Anchor.X + size && x < GridPos.MapWidth; x++)
            {
                for (int y = Anchor.Y; y < Anchor.Y + size && y < GridPos.MapHeight; y++)
                {
                    result.Add(new GridPos(x, y));
                }
            }
        }

        /// <summary>
        /// Returns the current count of adult residents in this house.
        /// Only meaningful for houses; other building types return 0.
        /// </summary>
        public int GetAdultCount() => AdultResidentIds.Count;

        /// <summary>
        /// Returns the current count of child residents in this house.
        /// Only meaningful for houses; other building types return 0.
        /// </summary>
        public int GetChildCount() => ChildResidentIds.Count;

        /// <summary>
        /// Checks if this house has an available adult slot.
        /// Returns true if adult count is less than 2; false otherwise.
        /// Only meaningful for houses.
        /// </summary>
        public bool HasAvailableAdultSlot() => AdultResidentIds.Count < 2;

        /// <summary>
        /// Assigns an adult resident to this house.
        /// Does nothing if the house is already at capacity (2 adults).
        /// Returns true if assignment succeeded, false if house is full.
        /// </summary>
        public bool AssignAdultResident(int agentId)
        {
            if (!HasAvailableAdultSlot())
                return false;

            if (!AdultResidentIds.Contains(agentId))
            {
                AdultResidentIds.Add(agentId);
            }
            return true;
        }

        /// <summary>
        /// Removes an adult resident from this house.
        /// </summary>
        public void RemoveAdultResident(int agentId)
        {
            AdultResidentIds.Remove(agentId);
        }

        /// <summary>
        /// Assigns a child resident to this house.
        /// Returns true (children have unlimited capacity).
        /// </summary>
        public bool AssignChildResident(int agentId)
        {
            if (!ChildResidentIds.Contains(agentId))
            {
                ChildResidentIds.Add(agentId);
            }
            return true;
        }

        /// <summary>
        /// Removes a child resident from this house.
        /// </summary>
        public void RemoveChildResident(int agentId)
        {
            ChildResidentIds.Remove(agentId);
        }
    }

    /// <summary>
    /// Validates building placement according to the build rules.
    /// </summary>
    public static class BuildingPlacementValidator
    {
        /// <summary>
        /// Check if a building can be placed at the given anchor position.
        /// Rules:
        /// - Footprint must not extend outside the map.
        /// - Must not overlap with any existing building.
        /// - Must have at least 1 tile gap from other buildings (Chebyshev distance).
        /// - Resource facilities must overlap at least one tile with a matching natural node.
        /// </summary>
        public static bool CanPlaceBuilding(
            System.Collections.Generic.IEnumerable<Building> buildings,
            BuildingKind kind,
            GridPos anchor,
            System.Collections.Generic.IEnumerable<NaturalNode> naturalNodes = null)
        {
            int footprintSize = BuildingKindHelpers.GetFootprintSize(kind);

            // Check if footprint extends outside the map
            if (anchor.X + footprintSize > GridPos.MapWidth || anchor.Y + footprintSize > GridPos.MapHeight)
                return false;

            // Check for overlap and 1 tile gap from existing buildings
            foreach (var existing in buildings)
            {
                if (BuildingsOverlapOrTooClose(anchor, footprintSize, existing.Anchor, existing.GetFootprintSize()))
                    return false;
            }

            // Check resource facility node matching if applicable
            if (BuildingKindHelpers.IsResourceFacility(kind))
            {
                if (naturalNodes == null)
                    return false; // Cannot place resource facility without checking nodes

                var requiredNodeType = BuildingKindHelpers.GetRequiredNodeType(kind).Value;
                if (!HasMatchingNodeOverlap(anchor, footprintSize, naturalNodes, requiredNodeType))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if a building footprint overlaps with at least one natural node of the required type.
        /// </summary>
        private static bool HasMatchingNodeOverlap(
            GridPos anchor,
            int footprintSize,
            System.Collections.Generic.IEnumerable<NaturalNode> naturalNodes,
            NaturalNodeType requiredType)
        {
            foreach (var node in naturalNodes)
            {
                if (node.Type != requiredType)
                    continue;

                // Check if the node position is within the building's footprint
                if (node.Position.X >= anchor.X && node.Position.X < anchor.X + footprintSize &&
                    node.Position.Y >= anchor.Y && node.Position.Y < anchor.Y + footprintSize)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if two building footprints overlap or are too close (less than 1 tile gap).
        /// A 1 tile gap means the Chebyshev distance between the closest tiles must be >= 2.
        /// </summary>
        private static bool BuildingsOverlapOrTooClose(GridPos anchor1, int size1, GridPos anchor2, int size2)
        {
            // Footprints occupy tiles [anchor.X, anchor.X + size), [anchor.Y, anchor.Y + size)
            // In inclusive coordinates: [anchor.X, anchor.X + size - 1]

            int b1MinX = anchor1.X;
            int b1MaxX = anchor1.X + size1 - 1;
            int b1MinY = anchor1.Y;
            int b1MaxY = anchor1.Y + size1 - 1;

            int b2MinX = anchor2.X;
            int b2MaxX = anchor2.X + size2 - 1;
            int b2MinY = anchor2.Y;
            int b2MaxY = anchor2.Y + size2 - 1;

            // Check for overlap
            bool overlapsX = b1MinX <= b2MaxX && b2MinX <= b1MaxX;
            bool overlapsY = b1MinY <= b2MaxY && b2MinY <= b1MaxY;

            if (overlapsX && overlapsY)
                return true; // Overlapping

            // Calculate Chebyshev distance between the closest tiles of the two buildings
            int distX = 0;
            if (b1MaxX < b2MinX)
            {
                // Building 1 is to the left of Building 2
                distX = b2MinX - b1MaxX;
            }
            else if (b2MaxX < b1MinX)
            {
                // Building 2 is to the left of Building 1
                distX = b1MinX - b2MaxX;
            }

            int distY = 0;
            if (b1MaxY < b2MinY)
            {
                // Building 1 is below Building 2
                distY = b2MinY - b1MaxY;
            }
            else if (b2MaxY < b1MinY)
            {
                // Building 2 is below Building 1
                distY = b1MinY - b2MaxY;
            }

            int chebyshevDistance = System.Math.Max(distX, distY);
            return chebyshevDistance < 2; // Too close if Chebyshev distance < 2 (less than 1 tile gap)
        }
    }
}
