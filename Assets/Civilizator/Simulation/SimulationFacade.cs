using System.Linq;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Read-only façade for UI to access simulation state.
    /// Provides a clean interface that prevents UI from directly modifying simulation state.
    /// </summary>
    public class SimulationFacade
    {
        private readonly World world;

        public SimulationFacade(World world)
        {
            this.world = world;
        }

        /// <summary>
        /// Current simulation cycle.
        /// </summary>
        public int CurrentCycle => world.Clock.CurrentCycle;

        /// <summary>
        /// Total simulation seconds elapsed.
        /// </summary>
        public float TotalSimulationSeconds => world.Clock.TotalSimulationSeconds;

        /// <summary>
        /// Current central storage stocks.
        /// </summary>
        public (int logs, int ore, int meat, int plantFood) CentralStocks => world.Storage.GetAllStocks();

        /// <summary>
        /// Total population count.
        /// </summary>
        public int TotalPopulation => world.Agents.Count;

        /// <summary>
        /// Population breakdown by life stage.
        /// </summary>
        public (int children, int adults, int elders) PopulationByStage
        {
            get
            {
                int children = 0, adults = 0, elders = 0;
                foreach (var agent in world.Agents)
                {
                    switch (agent.LifeStage)
                    {
                        case LifeStage.Child:
                            children++;
                            break;
                        case LifeStage.Adult:
                            adults++;
                            break;
                        case LifeStage.Elder:
                            elders++;
                            break;
                    }
                }
                return (children, adults, elders);
            }
        }

        /// <summary>
        /// Housing statistics.
        /// </summary>
        public (int assignedAdults, int unassignedAdults, int totalHouses) HousingStats
        {
            get
            {
                int assignedAdults = world.Agents.Count(a => a.LifeStage != LifeStage.Child && a.AssignedHouseId.HasValue);
                int totalAdults = world.Agents.Count(a => a.LifeStage != LifeStage.Child);
                int totalHouses = world.Buildings.Count(b => b.Kind == BuildingKind.House);
                return (assignedAdults, totalAdults - assignedAdults, totalHouses);
            }
        }

        /// <summary>
        /// Profession allocation (actual counts).
        /// </summary>
        public (int woodcutters, int miners, int hunters, int farmers, int builders, int soldiers) ProfessionCounts
        {
            get
            {
                int woodcutters = 0, miners = 0, hunters = 0, farmers = 0, builders = 0, soldiers = 0;
                foreach (var agent in world.Agents)
                {
                    switch (agent.Profession)
                    {
                        case Profession.Woodcutter:
                            woodcutters++;
                            break;
                        case Profession.Miner:
                            miners++;
                            break;
                        case Profession.Hunter:
                            hunters++;
                            break;
                        case Profession.Farmer:
                            farmers++;
                            break;
                        case Profession.Builder:
                            builders++;
                            break;
                        case Profession.Soldier:
                            soldiers++;
                            break;
                    }
                }
                return (woodcutters, miners, hunters, farmers, builders, soldiers);
            }
        }

        /// <summary>
        /// Profession target percentages.
        /// </summary>
        public (float woodcutter, float miner, float hunter, float farmer, float builder, float soldier) ProfessionTargets
        {
            get
            {
                var targets = world.ProfessionTargets.GetTargetsCopy();
                return (targets[0], targets[1], targets[2], targets[3], targets[4], targets[5]);
            }
        }

        /// <summary>
        /// Average productivity across all agents.
        /// </summary>
        public float AverageProductivity
        {
            get
            {
                if (world.Agents.Count == 0)
                    return 0f;

                float total = 0f;
                foreach (var agent in world.Agents)
                {
                    total += agent.GetProductivityMultiplier();
                }
                return total / world.Agents.Count;
            }
        }

        /// <summary>
        /// Number of agents currently starving (productivity at 0%).
        /// </summary>
        public int StarvingAgentCount => world.Agents.Count(a => a.EatingState.StarvationPenalty >= 1.0f);

        /// <summary>
        /// Whether the game is over.
        /// </summary>
        public bool IsGameOver => world.GameOver.IsGameOver;

        /// <summary>
        /// Game over reason (if game is over).
        /// </summary>
        public GameOverState.GameOverReason GameOverReason => world.GameOver.Reason;

        /// <summary>
        /// Reproduction rate setting.
        /// </summary>
        public float ReproductionRate => world.ReproductionSettings.ReproductionRate;

        /// <summary>
        /// Soldier patrol/improve split percentage.
        /// </summary>
        public float SoldierPatrolPercentage => world.SoldierControls.PatrolTargetShare;

        /// <summary>
        /// Tower build emphasis setting.
        /// </summary>
        public float TowerBuildEmphasis => world.SoldierControls.TowerBuildEmphasis;

        /// <summary>
        /// Number of staffed towers (towers with soldiers inside).
        /// </summary>
        public int StaffedTowerCount
        {
            get
            {
                int count = 0;
                var towers = world.Buildings.Where(b => b.Kind == BuildingKind.Tower);
                foreach (var tower in towers)
                {
                    var occupiedTiles = new System.Collections.Generic.List<GridPos>();
                    tower.GetOccupiedTiles(occupiedTiles);
                    foreach (var tile in occupiedTiles)
                    {
                        if (world.Agents.Any(a => a.Position == tile && a.Profession == Profession.Soldier))
                        {
                            count++;
                            break;
                        }
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Number of soldiers in patrol mode vs improve mode.
        /// </summary>
        public (int patrol, int improve) SoldierModeCounts
        {
            get
            {
                int patrol = 0, improve = 0;
                foreach (var agent in world.Agents)
                {
                    if (agent.Profession == Profession.Soldier)
                    {
                        if (agent.SoldierMode == SoldierMode.Patrolling)
                            patrol++;
                        else
                            improve++;
                    }
                }
                return (patrol, improve);
            }
        }
    }
}
