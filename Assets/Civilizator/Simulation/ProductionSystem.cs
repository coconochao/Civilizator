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
    }
}
