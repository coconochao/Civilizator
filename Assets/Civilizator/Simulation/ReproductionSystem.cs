using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages reproduction mechanics for the simulation.
    /// Controls global reproduction rate and handles child spawning logic.
    /// </summary>
    public static class ReproductionSystem
    {
        /// <summary>
        /// Global reproduction rate parameter controlled by the player.
        /// Represents the probability (0.0 to 1.0) that a breeding pair will produce a child per cycle.
        /// </summary>
        public static float ReproductionRate { get; set; } = 0.5f;

        /// <summary>
        /// Random number generator for reproduction decisions.
        /// Can be seeded for deterministic testing.
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Sets the random seed for deterministic reproduction behavior.
        /// Useful for testing and reproducible scenarios.
        /// </summary>
        /// <param name="seed">Seed value for the random number generator</param>
        public static void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Processes reproduction for all eligible houses in the simulation.
        /// Called once per simulation cycle.
        /// </summary>
        /// <param name="agents">List of all agents in the simulation</param>
        /// <param name="buildings">List of all buildings in the simulation</param>
        /// <returns>List of new child agents that were spawned</returns>
        public static List<Agent> ProcessReproduction(List<Agent> agents, List<Building> buildings)
        {
            var newChildren = new List<Agent>();

            // Find all houses that have exactly 2 assigned adults
            var eligibleHouses = FindEligibleHouses(buildings, agents);

            foreach (var house in eligibleHouses)
            {
                // Roll for reproduction based on the global rate
                if (_random.NextDouble() < ReproductionRate)
                {
                    var child = CreateChildForHouse(house, agents);
                    newChildren.Add(child);
                }
            }

            return newChildren;
        }

        /// <summary>
        /// Finds all houses that have exactly 2 assigned adult residents.
        /// These houses are eligible for reproduction.
        /// </summary>
        /// <param name="buildings">List of all buildings</param>
        /// <param name="agents">List of all agents</param>
        /// <returns>List of eligible houses</returns>
        private static List<Building> FindEligibleHouses(List<Building> buildings, List<Agent> agents)
        {
            var eligibleHouses = new List<Building>();

            foreach (var building in buildings)
            {
                if (building.Kind != BuildingKind.House)
                    continue;

                // Count assigned adult residents
                var adultCount = 0;
                foreach (var residentId in building.AdultResidentIds)
                {
                    var agent = agents.FirstOrDefault(a => a.Id == residentId);
                    if (agent != null && agent.LifeStage == LifeStage.Adult && agent.IsAlive)
                    {
                        adultCount++;
                    }
                }

                // House is eligible if it has exactly 2 assigned adults
                if (adultCount == 2)
                {
                    eligibleHouses.Add(building);
                }
            }

            return eligibleHouses;
        }

        /// <summary>
        /// Creates a new child agent for the specified house.
        /// The child is positioned at the house location and assigned to the house.
        /// </summary>
        /// <param name="house">The house where the child will be born</param>
        /// <param name="agents">List of all agents (used to generate unique ID)</param>
        /// <returns>New child agent</returns>
        private static Agent CreateChildForHouse(Building house, List<Agent> agents)
        {
            // Generate unique ID for the child
            var newId = agents.Count > 0 ? agents.Max(a => a.Id) + 1 : 1;

            // Create child at house position
            var child = new Agent(house.Anchor, newId)
            {
                LifeStage = LifeStage.Child,
                HitPoints = 10
            };

            // Assign child to the house
            house.AssignChildResident(child.Id);

            return child;
        }
    }
}