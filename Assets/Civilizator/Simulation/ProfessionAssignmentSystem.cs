using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Handles profession assignment for new agents based on target distribution.
    /// When a new agent spawns, it chooses the most undernumbered profession
    /// (the one with the largest gap between target % and actual %).
    /// </summary>
    public static class ProfessionAssignmentSystem
    {
        private static Random _random = new Random();

        /// <summary>
        /// Sets the random seed for deterministic testing.
        /// </summary>
        public static void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Determines the most undernumbered profession for a new agent.
        /// Compares actual distribution vs target distribution and returns
        /// the profession with the largest deficit (target% - actual%).
        /// If multiple professions have the same deficit, picks randomly among them.
        /// </summary>
        /// <param name="agents">Current list of agents (used to calculate actual distribution).</param>
        /// <param name="targets">Target profession distribution percentages.</param>
        /// <returns>The profession that is most undernumbered.</returns>
        public static Profession GetMostUndernumberedProfession(List<Agent> agents, ProfessionTargets targets)
        {
            if (agents == null || agents.Count == 0)
            {
                // No agents yet, pick a random profession weighted by targets
                return PickByTargetWeight(targets);
            }

            // Calculate actual profession distribution
            int totalAgents = agents.Count;
            float[] actualPercentages = new float[6];
            
            foreach (var agent in agents)
            {
                if (agent.IsAlive)
                {
                    actualPercentages[(int)agent.Profession]++;
                }
            }

            // Convert to percentages
            for (int i = 0; i < 6; i++)
            {
                actualPercentages[i] /= totalAgents;
            }

            // Find profession with maximum deficit (target - actual)
            float maxDeficit = float.MinValue;
            List<Profession> candidates = new List<Profession>();

            for (int i = 0; i < 6; i++)
            {
                Profession profession = (Profession)i;
                float target = targets.GetTarget(profession);
                float actual = actualPercentages[i];
                float deficit = target - actual;

                if (deficit > maxDeficit)
                {
                    maxDeficit = deficit;
                    candidates.Clear();
                    candidates.Add(profession);
                }
                else if (Math.Abs(deficit - maxDeficit) < 0.0001f)
                {
                    // Tie - add to candidates
                    candidates.Add(profession);
                }
            }

            // If only one candidate, return it; otherwise pick randomly
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            return candidates[_random.Next(candidates.Count)];
        }

        /// <summary>
        /// Assigns a profession to a new agent based on the current distribution.
        /// </summary>
        /// <param name="agent">The agent to assign a profession to.</param>
        /// <param name="otherAgents">Current list of agents (used to calculate actual distribution).</param>
        /// <param name="targets">Target profession distribution percentages.</param>
        public static void AssignProfession(Agent agent, List<Agent> otherAgents, ProfessionTargets targets)
        {
            Profession profession = GetMostUndernumberedProfession(otherAgents, targets);
            agent.Profession = profession;
            if (profession == Profession.Soldier)
            {
                agent.SoldierMode = SoldierMode.Patrolling;
            }
        }

        /// <summary>
        /// Picks a profession based on target weights (used when no agents exist yet).
        /// </summary>
        private static Profession PickByTargetWeight(ProfessionTargets targets)
        {
            float roll = (float)_random.NextDouble();
            float cumulative = 0f;

            for (int i = 0; i < 6; i++)
            {
                Profession profession = (Profession)i;
                float target = targets.GetTarget(profession);
                cumulative += target;

                if (roll < cumulative)
                {
                    return profession;
                }
            }

            // Fallback (shouldn't reach here if targets sum to 1.0)
            return Profession.Woodcutter;
        }

        /// <summary>
        /// Calculates the actual percentage of agents with a given profession.
        /// </summary>
        public static float GetActualPercentage(List<Agent> agents, Profession profession)
        {
            if (agents == null || agents.Count == 0)
                return 0f;

            int count = 0;
            foreach (var agent in agents)
            {
                if (agent.IsAlive && agent.Profession == profession)
                {
                    count++;
                }
            }

            return (float)count / agents.Count;
        }

        /// <summary>
        /// Counts agents by profession.
        /// </summary>
        public static int[] GetProfessionCounts(List<Agent> agents)
        {
            int[] counts = new int[6];
            if (agents == null)
                return counts;

            foreach (var agent in agents)
            {
                if (agent.IsAlive)
                {
                    counts[(int)agent.Profession]++;
                }
            }

            return counts;
        }
    }
}
