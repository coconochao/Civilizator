using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Central simulation state container.
    /// Holds all simulation entities and coordinates the simulation step.
    /// </summary>
    public class World
    {
        /// <summary>
        /// The simulation clock.
        /// </summary>
        public SimulationClock Clock { get; private set; }

        /// <summary>
        /// Central building resource storage.
        /// </summary>
        public CentralStorage Storage { get; private set; }

        /// <summary>
        /// All agents in the simulation.
        /// </summary>
        public List<Agent> Agents { get; private set; }

        /// <summary>
        /// All buildings in the simulation.
        /// </summary>
        public List<Building> Buildings { get; private set; }

        /// <summary>
        /// All natural resource nodes.
        /// </summary>
        public List<NaturalNode> NaturalNodes { get; private set; }

        /// <summary>
        /// All spawned resources (from facilities).
        /// </summary>
        public List<SpawnedResource> SpawnedResources { get; private set; }

        /// <summary>
        /// All enemies in the simulation.
        /// </summary>
        public List<Enemy> Enemies { get; private set; }

        /// <summary>
        /// Grid occupancy tracking.
        /// </summary>
        public GridOccupancy Occupancy { get; private set; }

        /// <summary>
        /// Game over state tracking.
        /// </summary>
        public GameOverState GameOver { get; private set; }

        /// <summary>
        /// Profession targets configuration.
        /// </summary>
        public ProfessionTargets ProfessionTargets { get; private set; }

        /// <summary>
        /// Reproduction settings.
        /// </summary>
        public ReproductionSettings ReproductionSettings { get; private set; }

        /// <summary>
        /// Producer thresholds are managed statically via ProducerThresholds class.
        /// </summary>

        /// <summary>
        /// Soldier controls configuration.
        /// </summary>
        public SoldierControls SoldierControls { get; private set; }

        private int nextBuildingId = 1;

        public World()
        {
            Clock = new SimulationClock();
            Storage = new CentralStorage();
            Agents = new List<Agent>();
            Buildings = new List<Building>();
            NaturalNodes = new List<NaturalNode>();
            SpawnedResources = new List<SpawnedResource>();
            Enemies = new List<Enemy>();
            Occupancy = new GridOccupancy();
            GameOver = new GameOverState();
            ProfessionTargets = new ProfessionTargets();
            ReproductionSettings = new ReproductionSettings();
            SoldierControls = new SoldierControls();
        }

        /// <summary>
        /// Initialize the world with generated natural nodes.
        /// </summary>
        public void Initialize(int seed = 42)
        {
            NaturalNodes = WorldGenerator.GenerateNodes(seed);
        }

        /// <summary>
        /// Advance the simulation by the given delta time in seconds.
        /// This is the single entry point for simulation progression.
        /// </summary>
        /// <param name="deltaTime">Time elapsed in simulation seconds.</param>
        public void SimulationStep(float deltaTime)
        {
            if (GameOver.IsGameOver)
                return;

            // Advance the clock
            Clock.Advance(deltaTime);

            // Track cycle changes for per-cycle updates
            int previousCycle = Clock.CurrentCycle - 1;
            bool cycleChanged = Clock.CurrentCycle > previousCycle;

            // Update all simulation systems
            // Note: For V1, individual system updates are called here.
            // As systems are integrated, they will be invoked in this method.

            // Per-cycle updates
            if (cycleChanged)
            {
                OnCycleChanged();
            }
        }

        private void OnCycleChanged()
        {
            // Reset eating flags for all agents
            foreach (var agent in Agents)
            {
                agent.HasEatenThisCycle = false;
            }

            // Note: Full system integration (facility spawning, enemy spawning, reproduction,
            // profession switching, soldier mode switching) will be implemented in Phase V (T-210+)
            // The simulation step driver is now in place and ready for integration.
        }

        /// <summary>
        /// Add a new agent to the world.
        /// </summary>
        public Agent AddAgent(GridPos position, Profession profession, LifeStage lifeStage = LifeStage.Adult)
        {
            var agent = new Agent(position, profession, lifeStage);
            Agents.Add(agent);
            return agent;
        }

        /// <summary>
        /// Add a new building to the world.
        /// </summary>
        public Building AddBuilding(BuildingKind kind, GridPos anchor)
        {
            var building = new Building(kind, anchor, nextBuildingId++);
            Buildings.Add(building);
            
            // Update occupancy
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                Occupancy.BlockTile(tile);
            }

            return building;
        }

        /// <summary>
        /// Remove an agent from the world.
        /// </summary>
        public void RemoveAgent(Agent agent)
        {
            Agents.Remove(agent);
        }

        /// <summary>
        /// Remove a building from the world.
        /// </summary>
        public void RemoveBuilding(Building building)
        {
            Buildings.Remove(building);
            
            // Update occupancy
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                Occupancy.UnblockTile(tile);
            }
        }

        /// <summary>
        /// Get all agents of a specific profession.
        /// </summary>
        public List<Agent> GetAgentsByProfession(Profession profession)
        {
            return Agents.Where(a => a.Profession == profession).ToList();
        }

        /// <summary>
        /// Get all agents in a specific life stage.
        /// </summary>
        public List<Agent> GetAgentsByLifeStage(LifeStage stage)
        {
            return Agents.Where(a => a.LifeStage == stage).ToList();
        }

        /// <summary>
        /// Get the central building.
        /// </summary>
        public Building GetCentralBuilding()
        {
            return Buildings.FirstOrDefault(b => b.Kind == BuildingKind.Central);
        }
    }
}
