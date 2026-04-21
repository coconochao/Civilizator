using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages periodic switching between soldier patrol and improvement modes.
    /// Uses a target patrolling share with a threshold and cooldown to avoid rapid oscillation.
    /// </summary>
    public class SoldierModeSwitchSystem
    {
        private readonly Dictionary<Agent, int> _lastSwitchCycle = new Dictionary<Agent, int>();
        private int _currentCycle = 0;

        /// <summary>
        /// Default target share for soldiers in patrolling mode.
        /// </summary>
        public const float DefaultPatrolTargetShare = 0.5f;

        /// <summary>
        /// Threshold (percentage points) used to decide when a soldier mode split is far enough
        /// from the target to trigger a switch.
        /// </summary>
        public const float DefaultSwitchThreshold = 0.15f;

        /// <summary>
        /// Cooldown (in cycles) between switches for the same soldier.
        /// </summary>
        public const int DefaultSwitchCooldownCycles = 5;

        private float _patrolTargetShare;
        private float _switchThreshold;
        private int _switchCooldownCycles;

        public SoldierModeSwitchSystem()
        {
            _patrolTargetShare = DefaultPatrolTargetShare;
            _switchThreshold = DefaultSwitchThreshold;
            _switchCooldownCycles = DefaultSwitchCooldownCycles;
        }

        /// <summary>
        /// Sets the target share of soldiers that should be patrolling.
        /// </summary>
        public void SetPatrolTargetShare(float share)
        {
            if (share < 0f || share > 1f)
                throw new ArgumentOutOfRangeException(nameof(share), "Target share must be between 0 and 1");

            _patrolTargetShare = share;
        }

        /// <summary>
        /// Sets the threshold for switching soldier modes.
        /// </summary>
        public void SetSwitchThreshold(float threshold)
        {
            if (threshold < 0f || threshold > 1f)
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1");

            _switchThreshold = threshold;
        }

        /// <summary>
        /// Sets the cooldown period between switches for the same soldier.
        /// </summary>
        public void SetSwitchCooldownCycles(int cycles)
        {
            if (cycles < 0)
                throw new ArgumentOutOfRangeException(nameof(cycles), "Cooldown must be non-negative");

            _switchCooldownCycles = cycles;
        }

        /// <summary>
        /// Advances the system by one cycle.
        /// Performs at most one soldier mode switch when the current split drifts
        /// too far from the target share.
        /// </summary>
        public bool AdvanceCycle(List<Agent> agents)
        {
            _currentCycle++;

            if (agents == null || agents.Count == 0)
                return false;

            int[] counts = GetModeCounts(agents);
            int totalSoldiers = counts[(int)SoldierMode.Patrolling] + counts[(int)SoldierMode.Improving];
            if (totalSoldiers == 0)
                return false;

            float actualPatrolShare = (float)counts[(int)SoldierMode.Patrolling] / totalSoldiers;

            if (actualPatrolShare > _patrolTargetShare + _switchThreshold)
            {
                Agent soldierToSwitch = FindSoldierToSwitch(agents, SoldierMode.Patrolling);
                if (soldierToSwitch != null)
                {
                    soldierToSwitch.SoldierMode = SoldierMode.Improving;
                    _lastSwitchCycle[soldierToSwitch] = _currentCycle;
                    return true;
                }
            }
            else if (actualPatrolShare < _patrolTargetShare - _switchThreshold)
            {
                Agent soldierToSwitch = FindSoldierToSwitch(agents, SoldierMode.Improving);
                if (soldierToSwitch != null)
                {
                    soldierToSwitch.SoldierMode = SoldierMode.Patrolling;
                    _lastSwitchCycle[soldierToSwitch] = _currentCycle;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current cycle number.
        /// </summary>
        public int GetCurrentCycle() => _currentCycle;

        /// <summary>
        /// Resets the system state.
        /// </summary>
        public void Reset()
        {
            _lastSwitchCycle.Clear();
            _currentCycle = 0;
        }

        /// <summary>
        /// Gets the last switch cycle for a soldier (0 if never switched).
        /// </summary>
        public int GetLastSwitchCycle(Agent agent)
        {
            return _lastSwitchCycle.TryGetValue(agent, out int cycle) ? cycle : 0;
        }

        /// <summary>
        /// Gets the number of soldiers who have switched modes.
        /// </summary>
        public int GetSwitchedAgentCount()
        {
            return _lastSwitchCycle.Count;
        }

        /// <summary>
        /// Returns the count of living soldiers by mode.
        /// Index 0 = patrolling, index 1 = improving.
        /// </summary>
        public static int[] GetModeCounts(List<Agent> agents)
        {
            int[] counts = new int[2];
            if (agents == null)
                return counts;

            foreach (var agent in agents)
            {
                if (agent == null || !agent.IsAlive || agent.Profession != Profession.Soldier)
                    continue;

                counts[(int)agent.SoldierMode]++;
            }

            return counts;
        }

        /// <summary>
        /// Gets the current share of soldiers that are patrolling.
        /// </summary>
        public static float GetPatrolShare(List<Agent> agents)
        {
            int[] counts = GetModeCounts(agents);
            int totalSoldiers = counts[(int)SoldierMode.Patrolling] + counts[(int)SoldierMode.Improving];
            if (totalSoldiers == 0)
                return 0f;

            return (float)counts[(int)SoldierMode.Patrolling] / totalSoldiers;
        }

        private Agent FindSoldierToSwitch(List<Agent> agents, SoldierMode sourceMode)
        {
            foreach (var agent in agents)
            {
                if (agent == null || !agent.IsAlive || agent.Profession != Profession.Soldier)
                    continue;

                if (agent.SoldierMode != sourceMode)
                    continue;

                if (CanSwitchAgent(agent))
                    return agent;
            }

            return null;
        }

        private bool CanSwitchAgent(Agent agent)
        {
            if (!_lastSwitchCycle.TryGetValue(agent, out int lastSwitch))
                return true;

            return (_currentCycle - lastSwitch) >= _switchCooldownCycles;
        }
    }
}
