using System;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages automatic house assignments for agents.
    /// When a house construction is completed, assigns up to 2 random unassigned adults to it.
    /// Supports deterministic RNG seeding for testing.
    /// </summary>
    public class HouseAssignmentSystem
    {
        private Random _rng;
        private Dictionary<Agent, int> _agentIds = new Dictionary<Agent, int>();
        private int _nextAgentId = 1;

        /// <summary>
        /// Creates a new HouseAssignmentSystem with a specified seed for deterministic behavior.
        /// </summary>
        public HouseAssignmentSystem(int seed = 0)
        {
            _rng = new Random(seed);
        }

        /// <summary>
        /// Gets or creates a unique ID for an agent.
        /// </summary>
        private int GetAgentId(Agent agent)
        {
            if (!_agentIds.TryGetValue(agent, out int id))
            {
                id = _nextAgentId++;
                _agentIds[agent] = id;
            }
            return id;
        }

        /// <summary>
        /// Processes all buildings and assigns agents to newly completed houses.
        /// For each house that has just completed construction:
        /// - Finds all unassigned adults (agents with AssignedHouseId == null and LifeStage == Adult)
        /// - Randomly selects up to 2 of them
        /// - Assigns them to the house
        /// - Updates the agents' AssignedHouseId
        /// 
        /// Buildings are checked for completion. If a building was just completed (and is a house),
        /// the assignment logic triggers. To prevent duplicate assignments, the completion state
        /// should be cleared after calling this method (e.g., by setting a flag on the building
        /// or by tracking previously-processed buildings externally).
        /// </summary>
        public void AssignAdultsToCompletedHouses(List<Agent> agents, List<Building> buildings)
        {
            if (agents == null || buildings == null)
                return;

            foreach (var building in buildings)
            {
                // Only process houses that have completed construction
                if (building.Kind != BuildingKind.House)
                    continue;

                if (!building.IsConstructionPhaseComplete())
                    continue;

                // Check if this house already has assignments (to prevent duplicate assignments)
                if (building.AdultResidentIds.Count >= 2)
                    continue;

                // Find unassigned adults
                var unassignedAdults = agents
                    .Where(a => a.LifeStage == LifeStage.Adult 
                             && !a.IsHouseAssigned 
                             && a.IsAlive)
                    .ToList();

                if (unassignedAdults.Count == 0)
                    continue;

                // Determine how many adults to assign (min of 2 and available count)
                int adultsToAssign = Math.Min(2, unassignedAdults.Count);

                // Randomly shuffle the list and take the first N adults
                ShuffleList(unassignedAdults);
                
                for (int i = 0; i < adultsToAssign; i++)
                {
                    var adult = unassignedAdults[i];
                    int agentId = GetAgentId(adult);
                    
                    // Assign the adult to the house
                    if (building.AssignAdultResident(agentId))
                    {
                        adult.AssignedHouseId = agentId;
                    }
                }
            }
        }

        /// <summary>
        /// Finds adults that should be assigned to the given completed house.
        /// Returns a list of up to 2 random unassigned adults.
        /// Agents must be Adult life stage, unassigned, and alive.
        /// </summary>
        public List<Agent> FindUnassignedAdultsForHouse(List<Agent> agents, int countNeeded = 2)
        {
            if (agents == null || countNeeded <= 0)
                return new List<Agent>();

            var unassignedAdults = agents
                .Where(a => a.LifeStage == LifeStage.Adult 
                         && !a.IsHouseAssigned 
                         && a.IsAlive)
                .ToList();

            int adultsToReturn = Math.Min(countNeeded, unassignedAdults.Count);
            
            if (adultsToReturn == 0)
                return new List<Agent>();

            // Shuffle and return the requested number
            ShuffleList(unassignedAdults);
            return unassignedAdults.Take(adultsToReturn).ToList();
        }

        /// <summary>
        /// Assigns a list of agents to a house, updating both the house and agent state.
        /// </summary>
        public void AssignAgentsToHouse(List<Agent> agentsToAssign, Building house)
        {
            if (agentsToAssign == null || house == null)
                return;

            if (house.Kind != BuildingKind.House)
                return;

            foreach (var agent in agentsToAssign)
            {
                if (agent == null || agent.LifeStage != LifeStage.Adult)
                    continue;

                int agentId = GetAgentId(agent);
                // Try to assign to the house
                if (house.AssignAdultResident(agentId))
                {
                    // Update agent's house assignment
                    agent.AssignedHouseId = agentId;
                }
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle to randomize a list in-place.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = _rng.Next(0, i + 1);
                
                // Swap
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Sets a new seed for the RNG to enable deterministic behavior in tests.
        /// </summary>
        public void SetSeed(int seed)
        {
            _rng = new Random(seed);
        }
    }
}
