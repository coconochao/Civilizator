namespace Civilizator.Simulation
{
    /// <summary>
    /// Manages simulation time progression.
    /// 1 cycle = 60 simulation seconds.
    /// The clock accumulates fractional seconds and increments cycles when reaching 60 seconds.
    /// </summary>
    public class SimulationClock
    {
        /// <summary>
        /// Number of simulation seconds per cycle.
        /// </summary>
        public const float SecondsPerCycle = 60f;

        /// <summary>
        /// Current cycle number (starts at 0).
        /// </summary>
        public int CurrentCycle { get; private set; } = 0;

        /// <summary>
        /// Accumulated seconds within the current cycle (0 to SecondsPerCycle).
        /// </summary>
        public float AccumulatedSeconds { get; private set; } = 0f;

        /// <summary>
        /// Advance the simulation clock by the given delta time in seconds.
        /// </summary>
        /// <param name="deltaTime">The time elapsed in simulation seconds.</param>
        public void Advance(float deltaTime)
        {
            if (deltaTime < 0)
                return;

            AccumulatedSeconds += deltaTime;

            while (AccumulatedSeconds >= SecondsPerCycle)
            {
                AccumulatedSeconds -= SecondsPerCycle;
                CurrentCycle++;
            }
        }

        /// <summary>
        /// Reset the clock to cycle 0, accumulated 0 seconds.
        /// </summary>
        public void Reset()
        {
            CurrentCycle = 0;
            AccumulatedSeconds = 0f;
        }

        /// <summary>
        /// Get the total simulation time elapsed (in seconds).
        /// </summary>
        public float TotalSimulationSeconds => CurrentCycle * SecondsPerCycle + AccumulatedSeconds;

        /// <summary>
        /// Returns the simulation time at which an action starting now and lasting for a given duration will complete.
        /// Duration is rounded up to the next second boundary.
        /// </summary>
        /// <param name="durationSeconds">Action duration in seconds (fractional allowed).</param>
        /// <returns>Absolute simulation time when action completes.</returns>
        public float GetActionEndTime(float durationSeconds)
        {
            float startTime = TotalSimulationSeconds;
            float endTime = startTime + durationSeconds;
            return endTime;
        }

        /// <summary>
        /// Check if an action scheduled to end at a specific simulation time has completed.
        /// </summary>
        /// <param name="endTime">Absolute simulation time when action should end.</param>
        /// <returns>True if current time >= endTime.</returns>
        public bool IsActionComplete(float endTime)
        {
            return TotalSimulationSeconds >= endTime;
        }
    }
}
