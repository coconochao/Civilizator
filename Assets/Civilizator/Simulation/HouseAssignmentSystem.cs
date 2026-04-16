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
        /// Fills house vacancies when adult/elder residents die.
        /// For each house:
        /// - Removes dead adult residents
        /// - Assigns a random unassigned adult to fill each vacancy (up to 2 adult residents per house)
        /// Agents must be Adult life stage, unassigned, and alive to fill vacancies.
        /// </summary>
        public void FillVacanciesFromDeaths(List<Agent> agents, List<Building> buildings)
        {
            if (agents == null || buildings == null)
                return;

            foreach (var house in buildings)
            {
                // Only process houses
                if (house.Kind != BuildingKind.House)
                    continue;

                // Remove dead adult residents from the house
                var deadResidentIds = new List<int>();
                foreach (var residentId in house.AdultResidentIds)
                {
                    // Find the agent with this resident ID
                    var resident = agents.FirstOrDefault(a => GetAgentId(a) == residentId);
                    if (resident == null || !resident.IsAlive)
                    {
                        deadResidentIds.Add(residentId);
                    }
                }

                // Remove dead residents and clear their house assignment
                foreach (var deadId in deadResidentIds)
                {
                    house.RemoveAdultResident(deadId);
                    
                    // Find the agent and clear their house assignment
                    var deadAgent = agents.FirstOrDefault(a => GetAgentId(a) == deadId);
                    if (deadAgent != null)
                    {
                        deadAgent.AssignedHouseId = null;
                    }
                }

                // Fill vacancies with random unassigned adults
                while (house.AdultResidentIds.Count < 2)
                {
                    var unassignedAdults = agents
                        .Where(a => a.LifeStage == LifeStage.Adult 
                                 && !a.IsHouseAssigned 
                                 && a.IsAlive)
                        .ToList();

                    if (unassignedAdults.Count == 0)
                        break;

                    // Pick a random unassigned adult
                    int randomIndex = _rng.Next(0, unassignedAdults.Count);
                    var chosenAdult = unassignedAdults[randomIndex];
                    int agentId = GetAgentId(chosenAdult);

                    // Assign to the house
                    if (house.AssignAdultResident(agentId))
                    {
                        chosenAdult.AssignedHouseId = agentId;
                    }
                    else
                    {
                        // If assignment failed, break to avoid infinite loop
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Assigns a newly adult agent (Child→Adult transition) to an available house.
        /// Searches for houses with open adult slots (less than 2 adult residents).
        /// If available, randomly selects one house and assigns the agent.
        /// If no houses have open slots, the agent remains unassigned.
        /// 
        /// This method is intended to be called when an agent's life stage changes from Child to Adult.
        /// </summary>
        /// <param name="agent">The agent that just became an Adult. Must be Adult life stage.</param>
        /// <param name="buildings">List of buildings to search for available houses.</param>
        /// <returns>True if the agent was successfully assigned to a house; false otherwise.</returns>
        public bool AssignNewAdultToHouse(Agent agent, List<Building> buildings)
        {
            if (agent == null || buildings == null)
                return false;

            if (agent.LifeStage != LifeStage.Adult)
                return false;

            // Find houses with open adult slots (less than 2 adult residents)
            var availableHouses = buildings
                .Where(b => b.Kind == BuildingKind.House && b.AdultResidentIds.Count < 2)
                .ToList();

            if (availableHouses.Count == 0)
                return false;

            // Randomly select one house
            int randomIndex = _rng.Next(0, availableHouses.Count);
            var selectedHouse = availableHouses[randomIndex];

            // Assign the agent to the selected house
            int agentId = GetAgentId(agent);
            if (selectedHouse.AssignAdultResident(agentId))
            {
                agent.AssignedHouseId = agentId;
                return true;
            }

            return false;
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
