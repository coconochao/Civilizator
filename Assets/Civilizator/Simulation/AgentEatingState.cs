namespace Civilizator.Simulation
{
    /// <summary>
    /// Tracks eating and starvation state for an agent.
    /// Manages the starvation productivity penalty and eating-related timers.
    /// </summary>
    public class AgentEatingState
    {
        /// <summary>
        /// Starvation penalty: -25% productivity per failed eating cycle (additive).
        /// </summary>
        public const float StarvationPenaltyPerCycle = 0.25f;

        /// <summary>
        /// Current starvation penalty as a decimal (e.g., 0.25 = -25%, 0.50 = -50%).
        /// Stacks additively, capped at 1.0 (100% penalty = 0% productivity).
        /// </summary>
        public float StarvationPenalty { get; set; } = 0f;

        /// <summary>
        /// Adds starvation penalty from a failed eating cycle.
        /// Capped at 1.0 (100% penalty).
        /// </summary>
        public void ApplyStarvationPenalty()
        {
            StarvationPenalty += StarvationPenaltyPerCycle;
            if (StarvationPenalty > 1.0f)
                StarvationPenalty = 1.0f;
        }

        /// <summary>
        /// Removes all starvation penalty when an agent successfully eats.
        /// </summary>
        public void ResetStarvationPenalty()
        {
            StarvationPenalty = 0f;
        }

        /// <summary>
        /// Checks if the agent has died due to starvation (productivity = 0%).
        /// </summary>
        public bool IsDeadFromStarvation => StarvationPenalty >= 1.0f;
    }
}
