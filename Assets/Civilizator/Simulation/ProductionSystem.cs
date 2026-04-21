using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Implements production loop logic for producer agents.
    /// Handles node selection, pathfinding, and gathering positioning.
    /// </summary>
    public static class ProductionSystem
    {
        /// <summary>
        /// Finds the nearest relevant natural node for an agent based on their profession.
        /// Uses Manhattan distance for selection.
        /// </summary>
        /// <param name="agent">The agent searching for a node</param>
        /// <param name="nodes">All available natural nodes</param>
        /// <returns>The nearest gatherable node, or null if none available</returns>
        public static NaturalNode FindNearestRelevantNode(Agent agent, IEnumerable<NaturalNode> nodes)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            ResourceKind requiredResource = GetRequiredResourceForProfession(agent.Profession);
            
            // Filter nodes by type and gatherability
            var validNodes = nodes
                .Where(n => GetResourceKindForNodeType(n.Type) == requiredResource)
                .Where(n => n.IsGatherable(hasQuarrySupport: false)) // Quarry support checked separately
                .ToList();

            if (validNodes.Count == 0)
                return null;

            // Find node with minimum Manhattan distance
            NaturalNode nearest = null;
            int minDistance = int.MaxValue;

            foreach (var node in validNodes)
            {
                int distance = GridPos.Manhattan(agent.Position, node.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
                // If tie, keep first encountered (deterministic order)
            }

            return nearest;
        }

        /// <summary>
        /// Checks if agent is on the same tile as the target node and can start gathering.
        /// </summary>
        public static bool IsOnSameTileAsNode(Agent agent, NaturalNode node)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return agent.Position == node.Position;
        }

        /// <summary>
        /// Gets the resource kind that a profession gathers during production.
        /// </summary>
        public static ResourceKind GetRequiredResourceForProfession(Profession profession)
        {
            return profession switch
            {
                Profession.Woodcutter => ResourceKind.Logs,
                Profession.Miner => ResourceKind.Ore,
                Profession.Hunter => ResourceKind.Meat,
                Profession.Farmer => ResourceKind.PlantFood,
                _ => throw new ArgumentException($"Profession {profession} is not a producer profession")
            };
        }

        /// <summary>
        /// Checks if a profession is a producer that gathers resources from natural nodes.
        /// </summary>
        public static bool IsProducerProfession(Profession profession)
        {
            return profession is 
                Profession.Woodcutter or 
                Profession.Miner or 
                Profession.Hunter or 
                Profession.Farmer;
        }
        
        /// <summary>
        /// Maps NaturalNodeType to the corresponding ResourceKind that it produces.
        /// </summary>
        private static ResourceKind GetResourceKindForNodeType(NaturalNodeType nodeType)
        {
            return nodeType switch
            {
                NaturalNodeType.Tree => ResourceKind.Logs,
                NaturalNodeType.Ore => ResourceKind.Ore,
                NaturalNodeType.Animal => ResourceKind.Meat,
                NaturalNodeType.Plant => ResourceKind.PlantFood,
                _ => throw new ArgumentException($"Unknown node type {nodeType}")
            };
        }

        /// <summary>
        /// Processes gathering for an agent that is on the same tile as a node.
        /// Gathers at rate of 1 unit per second adjusted by agent productivity.
        /// Continues until agent carry is full or node is depleted.
        /// </summary>
        /// <param name="agent">Gathering agent</param>
        /// <param name="node">Target node</param>
        /// <param name="deltaTime">Time elapsed since last update</param>
        /// <param name="gatherAccumulator">Accumulated fractional gather progress (modified in place)</param>
        /// <returns>Number of units gathered this tick</returns>
        public static int ProcessGathering(Agent agent, NaturalNode node, float deltaTime, ref float gatherAccumulator)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (deltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            if (!IsOnSameTileAsNode(agent, node))
                return 0;

            float productivity = agent.GetProductivityMultiplier();
            float gatherRatePerSecond = productivity; // 1 per second base, scaled by productivity

            gatherAccumulator += deltaTime * gatherRatePerSecond;

            int unitsGathered = 0;
            int carryCapacity = agent.GetCarryCapacity();
            int currentCarry = agent.CarriedResources;

            while (gatherAccumulator >= 1.0f && currentCarry < carryCapacity && node.Remaining > 0)
            {
                gatherAccumulator -= 1.0f;
                node.Gather(1);
                currentCarry++;
                unitsGathered++;
            }

            agent.CarriedResources = currentCarry;
            return unitsGathered;
        }

        /// <summary>
        /// Checks if agent is positioned at the central building storage area.
        /// Central building is anchored at (50,50) with 3x3 footprint.
        /// </summary>
        public static bool IsAtCentralStorage(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            
            // Central building is at anchor (50,50) with 3x3 footprint
            return agent.Position.X >= 49 && agent.Position.X <= 51 &&
                   agent.Position.Y >= 49 && agent.Position.Y <= 51;
        }

        /// <summary>
        /// Deposits all carried resources from agent into central storage.
        /// Clears agent's carried resources after successful deposit.
        /// </summary>
        /// <returns>Number of units actually deposited</returns>
        public static int DepositCarriedResources(Agent agent, CentralStorage storage)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (!IsAtCentralStorage(agent))
                return 0;
            if (agent.CarriedResources <= 0)
                return 0;

            ResourceKind resource = GetRequiredResourceForProfession(agent.Profession);
            int amount = agent.CarriedResources;
            
            storage.Deposit(resource, amount);
            agent.CarriedResources = 0;

            return amount;
        }

        /// <summary>
        /// Determines if agent should switch from production loop to improvement loop.
        /// Returns true when:
        /// 1. No valid gatherable nodes are available OR
        /// 2. Resource stock is above the stop threshold
        /// </summary>
        public static bool ShouldSwitchToImprovement(Agent agent, NaturalNode nearestNode, int currentStock, int maxStock)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (!IsProducerProfession(agent.Profession))
                throw new ArgumentException("Only producer professions can switch between production/improvement");

            // No nodes available → must improve
            if (nearestNode == null)
                return true;

            // Stock is above stop threshold → improve
            float normalizedStock = (float)currentStock / maxStock;
            float stopThreshold = ProducerThresholds.GetStopThreshold(agent.Profession);
            
            return normalizedStock >= stopThreshold;
        }

        /// <summary>
        /// Finds the nearest relevant building site that requires resources for improvement/upgrade.
        /// Uses Manhattan distance for selection.
        /// </summary>
        /// <param name="agent">The agent searching for a building target</param>
        /// <param name="buildings">All available buildings</param>
        /// <returns>Nearest building requiring work required by this profession, or null if none available</returns>
        public static Building FindNearestImprovementTarget(Agent agent, IEnumerable<Building> buildings)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));

            BuildingKind requiredBuilding = GetRequiredBuildingForProfession(agent.Profession);
            return FindNearestImprovementTarget(agent, buildings, requiredBuilding);
        }

        /// <summary>
        /// Finds the nearest relevant building site that requires resources for improvement/upgrade.
        /// Uses Manhattan distance for selection.
        /// </summary>
        /// <param name="agent">The agent searching for a building target</param>
        /// <param name="buildings">All available buildings</param>
        /// <param name="requiredBuilding">The building kind this agent should help improve</param>
        /// <returns>Nearest building requiring work required by this profession, or null if none available</returns>
        public static Building FindNearestImprovementTarget(Agent agent, IEnumerable<Building> buildings, BuildingKind requiredBuilding)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (buildings == null)
                throw new ArgumentNullException(nameof(buildings));

            // Filter buildings: correct type, under construction or can be upgraded
            var validTargets = buildings
                .Where(b => b.Kind == requiredBuilding)
                .Where(b => b.IsUnderConstruction || b.UpgradeLevel < 1)
                .Where(b => !b.IsConstructionPhaseComplete())
                .ToList();

            if (validTargets.Count == 0)
                return null;

            // Find building with minimum Manhattan distance to agent
            Building nearest = null;
            int minDistance = int.MaxValue;

            foreach (var building in validTargets)
            {
                int distance = GridPos.Manhattan(agent.Position, building.Anchor);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = building;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Withdraws resources from central storage for building improvement.
        /// Withdraws up to agent's carry capacity or remaining required resources.
        /// </summary>
        /// <returns>Amount actually withdrawn</returns>
        public static int WithdrawResourcesForImprovement(Agent agent, Building target, CentralStorage storage)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (!IsAtCentralStorage(agent))
                return 0;

            ResourceKind requiredResource = target.GetConstructionResourceKind();
            int remainingRequired = target.GetRequiredConstructionAmount() - target.ConstructionProgress;
            int carryCapacity = agent.GetCarryCapacity();
            int withdrawAmount = Math.Min(Math.Min(carryCapacity, remainingRequired), storage.GetStock(requiredResource));

            if (withdrawAmount <= 0)
                return 0;

            storage.Withdraw(requiredResource, withdrawAmount);
            agent.CarriedResources = withdrawAmount;

            return withdrawAmount;
        }

        /// <summary>
        /// Checks if agent is positioned at the target building footprint.
        /// </summary>
        public static bool IsAtBuildingSite(Agent agent, Building target)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            int size = target.GetFootprintSize();
            return agent.Position.X >= target.Anchor.X && agent.Position.X < target.Anchor.X + size &&
                   agent.Position.Y >= target.Anchor.Y && agent.Position.Y < target.Anchor.Y + size;
        }

        /// <summary>
        /// Delivers carried resources to building site and starts build-time timer.
        /// Applies productivity multiplier for build time calculation.
        /// </summary>
        /// <returns>Amount actually delivered</returns>
        public static int DeliverResourcesToBuilding(Agent agent, Building target, SimulationClock clock)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (!IsAtBuildingSite(agent, target))
                return 0;
            if (agent.CarriedResources <= 0)
                return 0;

            int delivered = agent.CarriedResources;
            target.DeliverBuildResources(delivered, agent.GetProductivityMultiplier());
            agent.CarriedResources = 0;

            return delivered;
        }

        /// <summary>
        /// Gets the building kind that a profession improves during improvement loop.
        /// </summary>
        public static BuildingKind GetRequiredBuildingForProfession(Profession profession)
        {
            return profession switch
            {
                Profession.Woodcutter => BuildingKind.Plantation,
                Profession.Miner => BuildingKind.Quarry,
                Profession.Hunter => BuildingKind.CattleFarm,
                Profession.Farmer => BuildingKind.Farm,
                _ => throw new ArgumentException($"Profession {profession} does not have an associated improvement building")
            };
        }
    }
}
