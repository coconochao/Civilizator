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

        private bool _hasInitialized;

        public SimulationTickDriver()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            EnsureInitialized();
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

        private void EnsureInitialized()
        {
            if (_hasInitialized)
            {
                return;
            }

            World ??= new World();
            World.Initialize();
            World.InitializeGameSetup();
            Facade = new SimulationFacade(World);
            _hasInitialized = true;
        }
    }
}
