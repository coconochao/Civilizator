using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    /// <summary>
    /// Drives the simulation with delta time from the presentation layer.
    /// This ensures the simulation is not tied to frame rate and responds correctly to
    /// variable Time.timeScale. Provides a single tick pipeline for simulation progression.
    /// </summary>
    public class SimulationTickDriver : MonoBehaviour
    {
        /// <summary>
        /// The world simulation state.
        /// </summary>
        public World World { get; set; }

        /// <summary>
        /// Read-only façade for UI to access simulation state.
        /// </summary>
        public SimulationFacade Facade { get; private set; }

        public SimulationTickDriver()
        {
            World = new World();
            World.Initialize();
            Facade = new SimulationFacade(World);
        }   

        private void Start()
        {
            // Initialize the world if not already set (allows injection for testing)
            World ??= new World();
            World.Initialize();
            
            // Create the façade for UI read-only access
            Facade = new SimulationFacade(World);
        }

        private void Update()
        {
            if (World != null)
            {
                // Single tick pipeline: advance simulation by delta time
                World.SimulationStep(Time.deltaTime);
            }
        }

        /// <summary>
        /// Get the current cycle without modifying state.
        /// </summary>
        public int CurrentCycle => World?.Clock.CurrentCycle ?? 0;

        /// <summary>
        /// Get total simulation seconds elapsed.
        /// </summary>
        public float TotalSimulationSeconds => World?.Clock.TotalSimulationSeconds ?? 0f;
    }
}
