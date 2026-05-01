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

        /// <summary>
        /// Aging system for managing agent life stage transitions.
        /// </summary>
        private AgingSystem AgingSystem { get; set; }

        /// <summary>
        /// Facility spawner for spawning resources from facilities.
        /// </summary>
        private FacilitySpawner FacilitySpawner { get; set; }

        /// <summary>
        /// Profession switch system for balancing profession distribution.
        /// </summary>
        private ProfessionSwitchSystem ProfessionSwitchSystem { get; set; }

        /// <summary>
        /// Soldier mode switch system for patrol/improve mode balancing.
        /// </summary>
        private SoldierModeSwitchSystem SoldierModeSwitchSystem { get; set; }

        /// <summary>
        /// House assignment system for managing housing assignments.
        /// </summary>
        private HouseAssignmentSystem HouseAssignmentSystem { get; set; }

        /// <summary>
        /// Enemy spawner for spawning enemies over time.
        /// </summary>
        private EnemySpawner EnemySpawner { get; set; }

        /// <summary>
        /// Per-agent action state tracking (production, improvement, eating, patrol).
        /// </summary>
        private Dictionary<Agent, AgentActionState> AgentActionStates { get; set; }

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
            AgingSystem = new AgingSystem();
            FacilitySpawner = new FacilitySpawner(Clock);
            ProfessionSwitchSystem = new ProfessionSwitchSystem(ProfessionTargets);
            SoldierModeSwitchSystem = new SoldierModeSwitchSystem();
            EnemySpawner = new EnemySpawner(Clock);
            HouseAssignmentSystem = new HouseAssignmentSystem();
            AgentActionStates = new Dictionary<Agent, AgentActionState>();
        }

        /// <summary>
        /// Initialize the world with generated natural nodes.
        /// </summary>
        public void Initialize(int seed = 42)
        {
            NaturalNodes = WorldGenerator.GenerateNodes(seed);
        }

        /// <summary>
        /// Initialize the world with a starting setup for gameplay.
        /// Creates central building, initial population, and registers systems.
        /// </summary>
        public void InitializeGameSetup()
        {
            // Create central building at center of map
            GridPos centralPos = new GridPos(48, 48);
            AddBuilding(BuildingKind.Central, centralPos);

            // Give the starting population enough food to avoid immediate starvation loops.
            Storage.Deposit(ResourceKind.Meat, 100);
            Storage.Deposit(ResourceKind.PlantFood, 100);

            // Spawn initial population - mix of professions
            SpawnInitialPopulation();

            // Register all agents in aging system
            foreach (var agent in Agents)
            {
                AgingSystem.RegisterAgent(agent);
                AgentActionStates[agent] = new AgentActionState();
            }

            ApplyPlayerSettings();
        }

        /// <summary>
        /// Applies the current player-facing settings objects to the live simulation systems.
        /// UI code can call this after mutating <see cref="ReproductionSettings"/> or <see cref="SoldierControls"/>.
        /// </summary>
        public void ApplyPlayerSettings()
        {
            ReproductionSystem.ApplySettings(ReproductionSettings);
            SoldierControls.ApplyToSimulation(SoldierModeSwitchSystem);
        }

        /// <summary>
        /// Applies only the current reproduction settings to the live reproduction system.
        /// </summary>
        public void ApplyReproductionSettings()
        {
            ReproductionSystem.ApplySettings(ReproductionSettings);
        }

        /// <summary>
        /// Applies only the current soldier controls to the live soldier mode switch system.
        /// </summary>
        public void ApplySoldierControls()
        {
            SoldierControls.ApplyToSimulation(SoldierModeSwitchSystem);
        }

        /// <summary>
        /// Spawns the initial population with a balanced mix of professions.
        /// </summary>
        private void SpawnInitialPopulation()
        {
            GridPos centralPos = new GridPos(49, 49); // Near central building

            // Spawn 2 of each profession (12 total adults)
            for (int i = 0; i < 2; i++)
            {
                AddAgent(new GridPos(centralPos.X + i, centralPos.Y), Profession.Woodcutter, LifeStage.Adult);
                AddAgent(new GridPos(centralPos.X - i, centralPos.Y + 1), Profession.Miner, LifeStage.Adult);
                AddAgent(new GridPos(centralPos.X + i + 1, centralPos.Y + 1), Profession.Hunter, LifeStage.Adult);
                AddAgent(new GridPos(centralPos.X - i - 1, centralPos.Y + 2), Profession.Farmer, LifeStage.Adult);
                AddAgent(new GridPos(centralPos.X + i + 2, centralPos.Y + 2), Profession.Builder, LifeStage.Adult);
                AddAgent(new GridPos(centralPos.X - i - 2, centralPos.Y + 3), Profession.Soldier, LifeStage.Adult);
            }
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

            int previousCycle = Clock.CurrentCycle;

            // Advance the clock
            Clock.Advance(deltaTime);

            // Track cycle changes for per-cycle updates
            bool cycleChanged = Clock.CurrentCycle > previousCycle;

            // Update all simulation systems
            UpdateAgents(deltaTime);
            UpdateEnemies(deltaTime);
            UpdateCombat(deltaTime);
            CheckGameOverConditions();

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

            // Process aging transitions
            var transitionedAgents = AgingSystem.AdvanceCycle();
            HandleAgingTransitions(transitionedAgents);

            // Spawn resources from facilities
            var newSpawned = FacilitySpawner.SpawnIfNewCycle(Buildings, SpawnedResources);
            SpawnedResources.AddRange(newSpawned);

            // Spawn enemies
            var newEnemies = EnemySpawner.SpawnIfDue();
            Enemies.AddRange(newEnemies);

            // Process reproduction
            var newChildren = ReproductionSystem.ProcessReproduction(Agents, Buildings);
            foreach (var child in newChildren)
            {
                Agents.Add(child);
                AgingSystem.RegisterAgent(child);
                AgentActionStates[child] = new AgentActionState();
            }

            // Process profession switching
            ProfessionSwitchSystem.AdvanceCycle(Agents);

            // Process soldier mode switching
            SoldierModeSwitchSystem.AdvanceCycle(Agents);

            // Handle house assignments
            HandleHouseAssignments();

            // Remove dead agents
            RemoveDeadAgents();

            // Check game over conditions
            CheckGameOverConditions();
        }

        /// <summary>
        /// Handles life stage transitions from aging system.
        /// </summary>
        private void HandleAgingTransitions(List<Agent> transitionedAgents)
        {
            foreach (var agent in transitionedAgents)
            {
                if (agent.LifeStage == LifeStage.Adult && !agent.IsHouseAssigned)
                {
                    // Child became adult - try to assign house
                    HouseAssignmentSystem.AssignNewAdultToHouse(agent, Buildings);
                }
                else if (!agent.IsAlive)
                {
                    // Agent died from old age
                    AgentActionStates.Remove(agent);
                }
            }
        }

        /// <summary>
        /// Handles house assignments for newly completed houses and vacancies.
        /// </summary>
        private void HandleHouseAssignments()
        {
            // Check for newly completed houses
            var newlyCompletedHouses = Buildings.Where(b => 
                b.Kind == BuildingKind.House && 
                !b.IsUnderConstruction && 
                !b.WasAssignedOnCompletion).ToList();

            foreach (var house in newlyCompletedHouses)
            {
                HouseAssignmentSystem.AssignAdultsToCompletedHouses(Agents, Buildings);
                house.WasAssignedOnCompletion = true;
            }

            // Fill vacancies from deaths
            HouseAssignmentSystem.FillVacanciesFromDeaths(Agents, Buildings);
        }

        /// <summary>
        /// Removes dead agents from the world.
        /// </summary>
        private void RemoveDeadAgents()
        {
            var deadAgents = Agents.Where(a => !a.IsAlive).ToList();
            foreach (var agent in deadAgents)
            {
                AgingSystem.UnregisterAgent(agent);
                AgentActionStates.Remove(agent);
                Agents.Remove(agent);
            }
        }

        /// <summary>
        /// Checks game over conditions and sets game over state if met.
        /// </summary>
        private void CheckGameOverConditions()
        {
            var central = GetCentralBuilding();
            if (central != null && central.HitPoints <= 0)
            {
                GameOver.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);
                return;
            }

            if (Agents.Count == 0 || Agents.All(a => !a.IsAlive))
            {
                GameOver.MarkGameOver(GameOverState.GameOverReason.EveryoneDead);
                return;
            }
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

            // Initialize construction state for non-Central buildings
            if (kind != BuildingKind.Central)
            {
                building.IsUnderConstruction = true;
                building.SimulationClock = Clock;
            }
            
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

        /// <summary>
        /// Get the action state for a specific agent (for testing/debugging).
        /// </summary>
        public AgentActionState GetAgentActionState(Agent agent)
        {
            return AgentActionStates.TryGetValue(agent, out var state) ? state : null;
        }

        /// <summary>
        /// Get debug information about an agent's current state.
        /// </summary>
        public string GetAgentDebugInfo(Agent agent)
        {
            var state = GetAgentActionState(agent);
            if (state == null)
                return $"{agent}: No action state";

            var pathInfo = state.CurrentPath != null && state.CurrentPath.Count > 0
                ? $"Path: {state.CurrentPath.Count} tiles, index {state.PathIndex}, dest {state.CurrentPath[state.CurrentPath.Count - 1]}"
                : "No path";

            var targetInfo = state.CurrentTargetBuilding != null
                ? $"Target: {state.CurrentTargetBuilding.Kind} at {state.CurrentTargetBuilding.Anchor}"
                : "No target building";

            return $"{agent} | {pathInfo} | {targetInfo} | Carried: {agent.CarriedResources} | MoveAcc: {state.MoveAccumulator:F2} | GatherAcc: {state.GatherAccumulator:F2}";
        }

        /// <summary>
        /// Updates all agents with their action loops.
        /// </summary>
        private void UpdateAgents(float deltaTime)
        {
            var central = GetCentralBuilding();
            if (central == null)
                return;

            UpdateAgentsByPriority(Agents.Where(a => a.IsAlive && a.Profession == Profession.Builder), deltaTime, central);
            UpdateAgentsByPriority(Agents.Where(a => a.IsAlive && a.Profession != Profession.Builder), deltaTime, central);
        }

        /// <summary>
        /// Updates a set of agents while preserving their individual state.
        /// </summary>
        private void UpdateAgentsByPriority(IEnumerable<Agent> agents, float deltaTime, Building central)
        {
            foreach (var agent in agents)
            {
                if (!AgentActionStates.TryGetValue(agent, out var actionState))
                {
                    actionState = new AgentActionState();
                    AgentActionStates[agent] = actionState;
                }

                UpdateAgentAction(agent, actionState, deltaTime, central);
            }
        }

        /// <summary>
        /// Updates a single agent's action based on their profession.
        /// </summary>
        private void UpdateAgentAction(Agent agent, AgentActionState state, float deltaTime, Building central)
        {
            GridPos centralPos = central.Anchor;
            switch (agent.Profession)
            {
                case Profession.Woodcutter:
                case Profession.Miner:
                case Profession.Hunter:
                case Profession.Farmer:
                    UpdateProducerAgent(agent, state, deltaTime, central);
                    break;
                case Profession.Builder:
                    UpdateBuilderAgent(agent, state, deltaTime, central);
                    break;
                case Profession.Soldier:
                    UpdateSoldierAgent(agent, state, deltaTime, central);
                    break;
            }
        }

        /// <summary>
        /// Updates producer agents (woodcutter, miner, hunter, farmer).
        /// </summary>
        private void UpdateProducerAgent(Agent agent, AgentActionState state, float deltaTime, Building central)
        {
            GridPos centralPos = central.Anchor;
            // Check if agent needs to eat
            if (!agent.HasEatenThisCycle && state.EatingAction == null)
            {
                state.EatingAction = new EatingAction(agent, centralPos);
                state.EatingAction.InitializePath(Occupancy);
            }

            // Process eating if needed
            if (state.EatingAction != null)
            {
                state.EatingAction.Update(deltaTime, Storage);
                if (state.EatingAction.IsComplete)
                {
                    state.EatingAction = null;
                }
                return; // Eating takes priority
            }

            // Production vs improvement decision
            var nearestNode = ProductionSystem.FindNearestRelevantNode(agent, NaturalNodes);
            bool shouldImprove = ProductionSystem.ShouldSwitchToImprovement(
                agent, nearestNode, 
                Storage.GetStock(ProductionSystem.GetRequiredResourceForProfession(agent.Profession)),
                1000); // Max stock for threshold calculation

            if (!shouldImprove)
            {
                // Production loop
                UpdateProductionLoop(agent, state, deltaTime, nearestNode, central);
            }
            else
            {
                // Improvement loop
                UpdateImprovementLoop(agent, state, deltaTime, central, null,
                    ProductionSystem.GetRequiredBuildingForProfession(agent.Profession));
            }
        }

        /// <summary>
        /// Updates the production loop for an agent.
        /// </summary>
        private void UpdateProductionLoop(Agent agent, AgentActionState state, float deltaTime, NaturalNode targetNode, Building central)
        {
            GridPos centralPos = central.Anchor;
            if (targetNode == null)
            {
                // No nodes available, switch to improvement
                UpdateImprovementLoop(agent, state, deltaTime, central, null,
                    ProductionSystem.GetRequiredBuildingForProfession(agent.Profession));
                return;
            }

            bool atTargetNode = ProductionSystem.IsOnSameTileAsNode(agent, targetNode);

            // If at node and still have room, gather
            if (atTargetNode && agent.CarriedResources < agent.GetCarryCapacity() && targetNode.Remaining > 0)
            {
                float gatherAccumulator = state.GatherAccumulator;
                ProductionSystem.ProcessGathering(agent, targetNode, deltaTime, ref gatherAccumulator);
                state.GatherAccumulator = gatherAccumulator;
            }
            else
            {
                // If carry is full or node depleted, go to central
                if (atTargetNode && (agent.CarriedResources >= agent.GetCarryCapacity() || targetNode.Remaining <= 0))
                {
                    if (state.CurrentPath == null || state.CurrentPath.Count == 0)
                    {
                        state.CurrentPath = Pathfinding.FindPathToOccupiedTarget(agent.Position, centralPos, Occupancy);
                        state.PathIndex = 0;
                    }
                    AdvanceAlongPath(agent, state, deltaTime);
                }
                else
                {
                    // Move to node
                    if (state.CurrentPath == null || state.CurrentPath.Count == 0)
                    {
                        state.CurrentPath = Pathfinding.FindPathToOccupiedTarget(agent.Position, targetNode.Position, Occupancy);
                        state.PathIndex = 0;
                    }
                    AdvanceAlongPath(agent, state, deltaTime);
                }
            }

            // If at central with resources, deposit
            if (agent.CarriedResources > 0 && ProductionSystem.IsAtCentralStorage(agent, central))
            {
                ProductionSystem.DepositCarriedResources(agent, Storage, central);
            }
        }

        /// <summary>
        /// Updates the improvement loop for an agent.
        /// </summary>
        private void UpdateImprovementLoop(
            Agent agent,
            AgentActionState state,
            float deltaTime,
            Building central,
            Building targetBuilding,
            BuildingKind targetKind)
        {
            GridPos centralPos = central.Anchor;
            if (targetBuilding != null && state.CurrentTargetBuilding != targetBuilding)
            {
                state.CurrentTargetBuilding = targetBuilding;
                state.CurrentPath = null;
                state.PathIndex = 0;
            }

            // If carrying resources, deliver to building
            if (agent.CarriedResources > 0)
            {
                if (state.CurrentTargetBuilding != null && ProductionSystem.IsAtBuildingSite(agent, state.CurrentTargetBuilding))
                {
                    ProductionSystem.DeliverResourcesToBuilding(agent, state.CurrentTargetBuilding, Clock);
                }
                else
                {
                    // Move to target building
                    if (state.CurrentTargetBuilding == null || state.CurrentPath == null || state.CurrentPath.Count == 0)
                    {
                        state.CurrentTargetBuilding = targetBuilding ?? ProductionSystem.FindNearestImprovementTarget(agent, Buildings, targetKind);
                        if (state.CurrentTargetBuilding != null)
                        {
                            state.CurrentPath = Pathfinding.FindPathToOccupiedTarget(agent.Position, state.CurrentTargetBuilding.Anchor, Occupancy);
                            state.PathIndex = 0;
                        }
                    }
                    AdvanceAlongPath(agent, state, deltaTime);
                }
            }
            else
            {
                // Withdraw resources from central
                if (state.CurrentTargetBuilding == null)
                {
                    state.CurrentTargetBuilding = targetBuilding ?? ProductionSystem.FindNearestImprovementTarget(agent, Buildings, targetKind);
                }

                if (state.CurrentTargetBuilding != null)
                {
                    if (ProductionSystem.IsAtCentralStorage(agent, central))
                    {
                        ProductionSystem.WithdrawResourcesForImprovement(agent, state.CurrentTargetBuilding, Storage, central);
                    }
                    else
                    {
                        // Move to central
                        if (state.CurrentPath == null || state.CurrentPath.Count == 0)
                        {
                            state.CurrentPath = Pathfinding.FindPathToOccupiedTarget(agent.Position, centralPos, Occupancy);
                            state.PathIndex = 0;
                        }
                        AdvanceAlongPath(agent, state, deltaTime);
                    }
                }
            }
        }

        /// <summary>
        /// Updates builder agents.
        /// </summary>
        private void UpdateBuilderAgent(Agent agent, AgentActionState state, float deltaTime, Building central)
        {
            GridPos centralPos = central.Anchor;
            // Check if agent needs to eat
            if (!agent.HasEatenThisCycle && state.EatingAction == null)
            {
                state.EatingAction = new EatingAction(agent, centralPos);
                state.EatingAction.InitializePath(Occupancy);
            }

            // Process eating if needed
            if (state.EatingAction != null)
            {
                state.EatingAction.Update(deltaTime, Storage);
                if (state.EatingAction.IsComplete)
                {
                    state.EatingAction = null;
                }
                return;
            }

            // Builders only do improvement work
            var target = BuilderSystem.FindBestImprovementTarget(agent, Agents, Buildings, Storage, ProfessionTargets);
            UpdateImprovementLoop(agent, state, deltaTime, central, target, target?.Kind ?? BuildingKind.House);
        }

        /// <summary>
        /// Updates soldier agents.
        /// </summary>
        private void UpdateSoldierAgent(Agent agent, AgentActionState state, float deltaTime, Building central)
        {
            GridPos centralPos = central.Anchor;
            if (agent.SoldierMode == SoldierMode.Patrolling)
            {
                // Patrol mode: keep moving through the perimeter patrol route.
                var patrolPositions = SoldierPatrolSystem.GetPatrolPositions(central, Buildings);
                if (patrolPositions.Count == 0)
                {
                    return;
                }

                if (!state.PatrolPosition.HasValue || (agent.Position == state.PatrolPosition.Value && (state.CurrentPath == null || state.CurrentPath.Count == 0)))
                {
                    if (!state.PatrolPosition.HasValue)
                    {
                        state.PatrolRouteIndex = agent.Id % patrolPositions.Count;
                    }
                    else
                    {
                        state.PatrolRouteIndex = (state.PatrolRouteIndex + 1) % patrolPositions.Count;
                    }

                    GridPos nextPatrolPosition = patrolPositions[state.PatrolRouteIndex];
                    if (nextPatrolPosition == agent.Position && patrolPositions.Count > 1)
                    {
                        state.PatrolRouteIndex = (state.PatrolRouteIndex + 1) % patrolPositions.Count;
                        nextPatrolPosition = patrolPositions[state.PatrolRouteIndex];
                    }

                    state.PatrolPosition = nextPatrolPosition;
                    state.CurrentPath = null;
                    state.PathIndex = 0;
                }

                if (state.PatrolPosition.HasValue)
                {
                    if (agent.Position != state.PatrolPosition.Value)
                    {
                        if (state.CurrentPath == null || state.CurrentPath.Count == 0)
                        {
                            state.CurrentPath = Pathfinding.FindPath(agent.Position, state.PatrolPosition.Value, Occupancy);
                            state.PathIndex = 0;
                        }
                        AdvanceAlongPath(agent, state, deltaTime);
                    }
                }
            }
            else
            {
                // Improve mode: build towers
                UpdateImprovementLoop(agent, state, deltaTime, central, null, BuildingKind.Tower);
            }
        }

        /// <summary>
        /// Advances agent along their current path.
        /// </summary>
        private void AdvanceAlongPath(Agent agent, AgentActionState state, float deltaTime)
        {
            if (state.CurrentPath == null || state.CurrentPath.Count == 0)
                return;

            state.MoveAccumulator += deltaTime;
            float moveSpeed = 1.0f; // 1 tile per second

            while (state.MoveAccumulator >= moveSpeed && state.PathIndex < state.CurrentPath.Count - 1)
            {
                state.MoveAccumulator -= moveSpeed;
                state.PathIndex++;
                agent.Position = state.CurrentPath[state.PathIndex];
            }

            if (state.PathIndex >= state.CurrentPath.Count - 1)
            {
                state.CurrentPath = null;
                state.PathIndex = 0;
            }
        }

        /// <summary>
        /// Updates enemy AI movement.
        /// </summary>
        private void UpdateEnemies(float deltaTime)
        {
            var central = GetCentralBuilding();
            if (central == null)
                return;

            foreach (var enemy in Enemies)
            {
                if (!enemy.IsAlive)
                    continue;

                EnemyAISystem.UpdateEnemy(enemy, Agents, Buildings, central, Occupancy, deltaTime);
            }
        }

        /// <summary>
        /// Updates combat for all combatants.
        /// </summary>
        private void UpdateCombat(float deltaTime)
        {
            // Tower attacks
            var central = GetCentralBuilding();
            if (central != null)
            {
                TowerCombatSystem.UpdateTowerCombat(Buildings, Agents, Enemies, Clock, deltaTime);
            }

            // Soldier attacks
            CombatSystem.UpdateSoldierCombat(Agents, Enemies, Clock, deltaTime);

            // Enemy attacks
            EnemyCombatSystem.UpdateEnemyCombat(Enemies, Agents, central, Clock, deltaTime);
        }
    }

    /// <summary>
    /// Tracks action state for an agent during simulation.
    /// </summary>
    public class AgentActionState
    {
        public List<GridPos> CurrentPath { get; set; }
        public int PathIndex { get; set; }
        public float MoveAccumulator { get; set; }
        public float GatherAccumulator { get; set; }
        public EatingAction EatingAction { get; set; }
        public Building CurrentTargetBuilding { get; set; }
        public GridPos? PatrolPosition { get; set; }
        public int PatrolRouteIndex { get; set; }

        public AgentActionState()
        {
            CurrentPath = new List<GridPos>();
            PathIndex = 0;
            MoveAccumulator = 0f;
            GatherAccumulator = 0f;
        }
    }
}
