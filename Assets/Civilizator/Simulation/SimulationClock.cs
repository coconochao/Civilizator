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
    }
}
