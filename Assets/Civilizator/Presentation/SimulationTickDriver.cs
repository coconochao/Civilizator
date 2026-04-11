using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    /// <summary>
    /// Drives the simulation clock with delta time from the presentation layer.
    /// This ensures the simulation is not tied to frame rate and responds correctly to
    /// variable Time.timeScale.
    /// </summary>
    public class SimulationTickDriver : MonoBehaviour
    {
        /// <summary>
        /// The shared simulation clock instance.
        /// </summary>
        public SimulationClock Clock { get; set; }

        private void Start()
        {
            // Initialize the clock if not already set (allows injection for testing)
            Clock ??= new SimulationClock();
        }

        private void Update()
        {
            if (Clock != null)
            {
                Clock.Advance(Time.deltaTime);
            }
        }

        /// <summary>
        /// Get the current cycle without modifying state.
        /// </summary>
        public int CurrentCycle => Clock?.CurrentCycle ?? 0;

        /// <summary>
        /// Get total simulation seconds elapsed.
        /// </summary>
        public float TotalSimulationSeconds => Clock?.TotalSimulationSeconds ?? 0f;
    }
}
