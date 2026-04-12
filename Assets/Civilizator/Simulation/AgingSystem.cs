using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages aging of agents over time.
    /// Tracks when each agent should transition to the next life stage.
    /// Progression: Child (10 cycles) → Adult (60 cycles) → Elder (10 cycles) → Death.
    /// </summary>
    public class AgingSystem
    {
        private readonly Dictionary<Agent, int> _agingCounters = new Dictionary<Agent, int>();

        /// <summary>
        /// Register a new agent in the aging system.
        /// Sets aging counter based on the agent's current life stage.
        /// </summary>
        public void RegisterAgent(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));

            int durationCycles = LifeStageHelpers.GetAgingDuration(agent.LifeStage);
            _agingCounters[agent] = durationCycles;
        }

        /// <summary>
        /// Unregister an agent from the aging system (e.g., when they die).
        /// </summary>
        public void UnregisterAgent(Agent agent)
        {
            if (agent != null)
                _agingCounters.Remove(agent);
        }

        /// <summary>
        /// Advance aging by one cycle.
        /// Returns list of agents that transitioned to a new life stage.
        /// Agents who reached Elder stage and complete their final cycle are marked for death.
        /// </summary>
        public List<Agent> AdvanceCycle()
        {
            var transitionedAgents = new List<Agent>();
            var agentsToUpdate = new List<Agent>(_agingCounters.Keys);

            foreach (var agent in agentsToUpdate)
            {
                if (!_agingCounters.ContainsKey(agent))
                    continue;

                _agingCounters[agent]--;

                // Check if agent transitions to next stage
                if (_agingCounters[agent] <= 0)
                {
                    if (agent.LifeStage == LifeStage.Elder)
                    {
                        // Agent reaches end of life (death)
                        agent.HitPoints = 0;
                        UnregisterAgent(agent);
                        transitionedAgents.Add(agent);
                    }
                    else
                    {
                        // Agent transitions to next stage
                        agent.LifeStage = LifeStageHelpers.GetNextStage(agent.LifeStage);
                        transitionedAgents.Add(agent);
                        
                        // Reset counter for new stage
                        int newDurationCycles = LifeStageHelpers.GetAgingDuration(agent.LifeStage);
                        _agingCounters[agent] = newDurationCycles;
                    }
                }
            }

            return transitionedAgents;
        }

        /// <summary>
        /// Gets the remaining cycles until the agent transitions to the next stage.
        /// Returns 0 if agent is not registered.
        /// </summary>
        public int GetRemainingCycles(Agent agent)
        {
            if (agent == null || !_agingCounters.ContainsKey(agent))
                return 0;

            return _agingCounters[agent];
        }

        /// <summary>
        /// Gets the total number of registered agents.
        /// </summary>
        public int GetRegisteredAgentCount()
        {
            return _agingCounters.Count;
        }

        /// <summary>
        /// Gets all registered agents.
        /// </summary>
        public IEnumerable<Agent> GetRegisteredAgents()
        {
            return _agingCounters.Keys;
        }

        /// <summary>
        /// Clears all agents from the aging system.
        /// </summary>
        public void Clear()
        {
            _agingCounters.Clear();
        }
    }
}
