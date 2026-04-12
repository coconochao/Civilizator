using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages quarry support for ore nodes and gathering rate calculations.
    /// 
    /// Quarry rules:
    /// - Quarry must overlap at least one Ore natural node
    /// - A quarry does not spawn ore, but enables indefinite collection of ore
    /// - Base quarry: allows gathering at half speed (2× time) for ore past depletion
    /// - Upgraded quarry: returns to normal speed (1×) for ore past depletion
    /// </summary>
    public static class QuarrySupport
    {
        /// <summary>
        /// Determines if an ore node is supported by any quarry building.
        /// A node is supported if at least one quarry's footprint overlaps the node position.
        /// </summary>
        public static bool IsNodeSupportedByQuarry(NaturalNode oreNode, IEnumerable<Building> quarries)
        {
            if (oreNode.Type != NaturalNodeType.Ore)
                throw new ArgumentException("Only ore nodes can be quarry-supported", nameof(oreNode));

            return quarries.Any(q => BuildingFootprintContainsPosition(q.Position, oreNode.Position));
        }

        /// <summary>
        /// Gets the gathering rate multiplier for ore gathering.
        /// Returns 1.0 for normal gathering (non-depleted or no quarry support).
        /// Returns 0.5 for base quarry support (half speed, 2× time).
        /// Returns 1.0 for upgraded quarry support (normal speed).
        /// </summary>
        public static float GetOreGatheringRateMultiplier(
            NaturalNode oreNode,
            bool isDepletedPastZero,
            Building supportingQuarry)
        {
            // No depletion issue or no quarry support -> normal rate
            if (!isDepletedPastZero || supportingQuarry == null)
                return 1.0f;

            // Node is depleted and has quarry support
            if (supportingQuarry.UpgradeLevel > 0)
            {
                // Upgraded quarry: return to normal rate
                return 1.0f;
            }

            // Base quarry: half speed (2× gathering time)
            return 0.5f;
        }

        /// <summary>
        /// Gets the gathering time multiplier for ore gathering.
        /// This is the inverse of the rate multiplier.
        /// Returns 1.0 for normal gathering.
        /// Returns 2.0 for base quarry support (2× time).
        /// Returns 1.0 for upgraded quarry support.
        /// </summary>
        public static float GetOreGatheringTimeMultiplier(
            NaturalNode oreNode,
            bool isDepletedPastZero,
            Building supportingQuarry)
        {
            float rateMultiplier = GetOreGatheringRateMultiplier(oreNode, isDepletedPastZero, supportingQuarry);
            return rateMultiplier > 0 ? 1.0f / rateMultiplier : 1.0f;
        }

        /// <summary>
        /// Finds the first quarry that supports the given ore node.
        /// Returns null if no quarry supports the node.
        /// </summary>
        public static Building FindSupportingQuarry(
            NaturalNode oreNode,
            IEnumerable<Building> quarries)
        {
            if (oreNode.Type != NaturalNodeType.Ore)
                throw new ArgumentException("Only ore nodes can be quarry-supported", nameof(oreNode));

            return quarries.FirstOrDefault(q => BuildingFootprintContainsPosition(q.Position, oreNode.Position));
        }

        /// <summary>
        /// Checks if a building's footprint contains a given position.
        /// Footprint is determined by building kind.
        /// </summary>
        private static bool BuildingFootprintContainsPosition(GridPos buildingAnchor, GridPos position)
        {
            // For now, assume 2×2 footprint (standard for all buildings)
            // TODO: Use BuildingKindHelpers if there are variable footprints
            return position.X >= buildingAnchor.X && position.X < buildingAnchor.X + 2 &&
                   position.Y >= buildingAnchor.Y && position.Y < buildingAnchor.Y + 2;
        }
    }
}
