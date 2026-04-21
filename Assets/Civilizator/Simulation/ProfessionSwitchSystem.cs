using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages periodic profession switching to correct distribution imbalances.
    /// Implements threshold-based switching with cooldown to prevent rapid oscillations.
    /// </summary>
    public class ProfessionSwitchSystem
    {
        private readonly ProfessionTargets _targets;
        private readonly Dictionary<Agent, int> _lastSwitchCycle = new Dictionary<Agent, int>();
        private int _currentCycle = 0;

        /// <summary>
        /// Threshold (percentage points) for switching professions.
        /// If discrepancy > threshold, switch one agent from most over to most under.
        /// </summary>
        public const float DefaultSwitchThreshold = 0.15f; // 15 percentage points

        /// <summary>
        /// Cooldown (in cycles) between profession switches for the same agent.
        /// Prevents rapid oscillations.
        /// </summary>
        public const int DefaultSwitchCooldownCycles = 5;

        private float _switchThreshold;
        private int _switchCooldownCycles;

        public ProfessionSwitchSystem(ProfessionTargets targets)
        {
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
            _switchThreshold = DefaultSwitchThreshold;
            _switchCooldownCycles = DefaultSwitchCooldownCycles;
        }

        /// <summary>
        /// Sets the threshold for profession switching.
        /// </summary>
        public void SetSwitchThreshold(float threshold)
        {
            if (threshold < 0f || threshold > 1f)
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1");
            _switchThreshold = threshold;
        }

        /// <summary>
        /// Sets the cooldown period between switches for the same agent.
        /// </summary>
        public void SetSwitchCooldownCycles(int cycles)
        {
            if (cycles < 0)
                throw new ArgumentOutOfRangeException(nameof(cycles), "Cooldown must be non-negative");
            _switchCooldownCycles = cycles;
        }

        /// <summary>
        /// Advances the system by one cycle.
        /// Checks if any switching is needed and performs at most one switch.
        /// </summary>
        /// <param name="agents">List of agents to consider for switching.</param>
        /// <returns>True if a switch was performed, false otherwise.</returns>
        public bool AdvanceCycle(List<Agent> agents)
        {
            _currentCycle++;

            if (agents == null || agents.Count == 0)
                return false;

            // Calculate actual distribution
            int[] counts = ProfessionAssignmentSystem.GetProfessionCounts(agents);
            int totalAgents = agents.Count;

            // Find most overrepresented and underrepresented professions
            Profession? mostOver = null;
            Profession? mostUnder = null;
            float maxOverage = 0f;
            float maxDeficit = 0f;

            for (int i = 0; i < 6; i++)
            {
                Profession p = (Profession)i;
                float target = _targets.GetTarget(p);
                float actual = (float)counts[i] / totalAgents;
                float discrepancy = actual - target;

                if (discrepancy > maxOverage)
                {
                    maxOverage = discrepancy;
                    mostOver = p;
                }

                if (discrepancy < -maxDeficit)
                {
                    maxDeficit = -discrepancy;
                    mostUnder = p;
                }
            }

            // Check if switching is needed
            if (mostOver.HasValue && mostUnder.HasValue && 
                maxOverage > _switchThreshold && maxDeficit > _switchThreshold)
            {
                // Find an agent to switch from mostOver to mostUnder
                Agent agentToSwitch = FindAgentToSwitch(agents, mostOver.Value, mostUnder.Value);
                
                if (agentToSwitch != null)
                {
                    // Perform the switch
                    agentToSwitch.Profession = mostUnder.Value;
                    if (mostUnder.Value == Profession.Soldier)
                    {
                        agentToSwitch.SoldierMode = SoldierMode.Patrolling;
                    }
                    _lastSwitchCycle[agentToSwitch] = _currentCycle;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds an agent to switch from source profession to target profession.
        /// Prefers agents who haven't switched recently and are not assigned to houses.
        /// </summary>
        private Agent FindAgentToSwitch(List<Agent> agents, Profession source, Profession target)
        {
            foreach (var agent in agents)
            {
                if (agent.Profession == source && CanSwitchAgent(agent))
                {
                    return agent;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if an agent can switch professions (cooldown check).
        /// </summary>
        private bool CanSwitchAgent(Agent agent)
        {
            if (!_lastSwitchCycle.TryGetValue(agent, out int lastSwitch))
                return true; // Never switched before

            return (_currentCycle - lastSwitch) >= _switchCooldownCycles;
        }

        /// <summary>
        /// Gets the current cycle number.
        /// </summary>
        public int GetCurrentCycle() => _currentCycle;

        /// <summary>
        /// Resets the system state (clears last switch tracking).
        /// </summary>
        public void Reset()
        {
            _lastSwitchCycle.Clear();
            _currentCycle = 0;
        }

        /// <summary>
        /// Gets the last switch cycle for an agent (0 if never switched).
        /// </summary>
        public int GetLastSwitchCycle(Agent agent)
        {
            return _lastSwitchCycle.TryGetValue(agent, out int cycle) ? cycle : 0;
        }

        /// <summary>
        /// Gets the number of agents who have switched professions.
        /// </summary>
        public int GetSwitchedAgentCount()
        {
            return _lastSwitchCycle.Count;
        }
    }
}
