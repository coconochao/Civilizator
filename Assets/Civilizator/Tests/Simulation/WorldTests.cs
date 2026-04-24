using NUnit.Framework;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class WorldTests
    {
        [Test]
        public void WorldInitializesWithAllComponents()
        {
            var world = new World();

            Assert.IsNotNull(world.Clock);
            Assert.IsNotNull(world.Storage);
            Assert.IsNotNull(world.Agents);
            Assert.IsNotNull(world.Buildings);
            Assert.IsNotNull(world.NaturalNodes);
            Assert.IsNotNull(world.SpawnedResources);
            Assert.IsNotNull(world.Enemies);
            Assert.IsNotNull(world.Occupancy);
            Assert.IsNotNull(world.GameOver);
            Assert.IsNotNull(world.ProfessionTargets);
            Assert.IsNotNull(world.ReproductionSettings);
            Assert.IsNotNull(world.SoldierControls);
        }

        [Test]
        public void WorldInitializesClockAtZero()
        {
            var world = new World();
            
            Assert.AreEqual(0, world.Clock.CurrentCycle);
            Assert.AreEqual(0f, world.Clock.TotalSimulationSeconds);
        }

        [Test]
        public void WorldInitializeGeneratesNaturalNodes()
        {
            var world = new World();
            world.Initialize(seed: 42);
            
            Assert.AreEqual(400, world.NaturalNodes.Count); // 10x10 regions x 4 types
        }

        [Test]
        public void SimulationStepAdvancesClock()
        {
            var world = new World();
            world.Initialize();
            
            world.SimulationStep(30f);
            
            Assert.AreEqual(0, world.Clock.CurrentCycle);
            Assert.AreEqual(30f, world.Clock.AccumulatedSeconds);
        }

        [Test]
        public void SimulationStepAdvancesFullCycle()
        {
            var world = new World();
            world.Initialize();
            
            world.SimulationStep(SimulationClock.SecondsPerCycle);
            
            Assert.AreEqual(1, world.Clock.CurrentCycle);
            Assert.AreEqual(0f, world.Clock.AccumulatedSeconds);
        }

        [Test]
        public void SimulationStepStopsWhenGameOver()
        {
            var world = new World();
            world.Initialize();
            world.GameOver.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);

            world.SimulationStep(30f);

            // Clock should not advance when game is over
            Assert.AreEqual(0, world.Clock.CurrentCycle);
        }

        [Test]
        public void AddAgentCreatesAgentWithCorrectProperties()
        {
            var world = new World();
            var position = new GridPos(5, 5);
            
            var agent = world.AddAgent(position, Profession.Woodcutter, LifeStage.Adult);
            
            Assert.IsNotNull(agent);
            Assert.AreEqual(position, agent.Position);
            Assert.AreEqual(Profession.Woodcutter, agent.Profession);
            Assert.AreEqual(LifeStage.Adult, agent.LifeStage);
            Assert.IsTrue(world.Agents.Contains(agent));
        }

        [Test]
        public void AddAgentDefaultsToAdult()
        {
            var world = new World();
            var position = new GridPos(5, 5);
            
            var agent = world.AddAgent(position, Profession.Woodcutter);
            
            Assert.AreEqual(LifeStage.Adult, agent.LifeStage);
        }

        [Test]
        public void AddBuildingCreatesBuildingWithCorrectProperties()
        {
            var world = new World();
            var anchor = new GridPos(10, 10);
            
            var building = world.AddBuilding(BuildingKind.House, anchor);
            
            Assert.IsNotNull(building);
            Assert.AreEqual(BuildingKind.House, building.Kind);
            Assert.AreEqual(anchor, building.Anchor);
            Assert.IsTrue(world.Buildings.Contains(building));
            Assert.Greater(building.Id, 0);
        }

        [Test]
        public void AddBuildingUpdatesOccupancy()
        {
            var world = new World();
            var anchor = new GridPos(10, 10);

            var building = world.AddBuilding(BuildingKind.House, anchor);

            var occupiedTiles = new System.Collections.Generic.List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                Assert.IsFalse(world.Occupancy.IsPassable(tile));
            }
        }

        [Test]
        public void RemoveAgentRemovesFromWorld()
        {
            var world = new World();
            var agent = world.AddAgent(new GridPos(5, 5), Profession.Woodcutter);
            
            world.RemoveAgent(agent);
            
            Assert.IsFalse(world.Agents.Contains(agent));
        }

        [Test]
        public void RemoveBuildingRemovesFromWorldAndClearsOccupancy()
        {
            var world = new World();
            var anchor = new GridPos(10, 10);
            var building = world.AddBuilding(BuildingKind.House, anchor);
            var occupiedTiles = new System.Collections.Generic.List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);

            world.RemoveBuilding(building);

            Assert.IsFalse(world.Buildings.Contains(building));
            foreach (var tile in occupiedTiles)
            {
                Assert.IsTrue(world.Occupancy.IsPassable(tile));
            }
        }

        [Test]
        public void GetAgentsByProfessionReturnsCorrectAgents()
        {
            var world = new World();
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter);
            world.AddAgent(new GridPos(2, 2), Profession.Woodcutter);
            world.AddAgent(new GridPos(3, 3), Profession.Miner);
            
            var woodcutters = world.GetAgentsByProfession(Profession.Woodcutter);
            
            Assert.AreEqual(2, woodcutters.Count);
            Assert.IsTrue(woodcutters.All(a => a.Profession == Profession.Woodcutter));
        }

        [Test]
        public void GetAgentsByLifeStageReturnsCorrectAgents()
        {
            var world = new World();
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Adult);
            world.AddAgent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Adult);
            world.AddAgent(new GridPos(3, 3), Profession.Woodcutter, LifeStage.Child);
            
            var adults = world.GetAgentsByLifeStage(LifeStage.Adult);
            
            Assert.AreEqual(2, adults.Count);
            Assert.IsTrue(adults.All(a => a.LifeStage == LifeStage.Adult));
        }

        [Test]
        public void GetCentralBuildingReturnsCentralOrNull()
        {
            var world = new World();
            
            // No central building yet
            Assert.IsNull(world.GetCentralBuilding());
            
            // Add central building
            world.AddBuilding(BuildingKind.Central, new GridPos(50, 50));
            
            Assert.IsNotNull(world.GetCentralBuilding());
            Assert.AreEqual(BuildingKind.Central, world.GetCentralBuilding().Kind);
        }

        [Test]
        public void SimulationStepResetsEatingFlagsOnCycleChange()
        {
            var world = new World();
            world.Initialize();
            var agent = world.AddAgent(new GridPos(5, 5), Profession.Woodcutter);
            agent.HasEatenThisCycle = true;
            
            // Advance one full cycle
            world.SimulationStep(SimulationClock.SecondsPerCycle);
            
            Assert.IsFalse(agent.HasEatenThisCycle);
        }

        [Test]
        public void BuildingIdsIncrementCorrectly()
        {
            var world = new World();
            
            var b1 = world.AddBuilding(BuildingKind.House, new GridPos(10, 10));
            var b2 = world.AddBuilding(BuildingKind.House, new GridPos(20, 20));
            var b3 = world.AddBuilding(BuildingKind.House, new GridPos(30, 30));
            
            Assert.AreEqual(1, b1.Id);
            Assert.AreEqual(2, b2.Id);
            Assert.AreEqual(3, b3.Id);
        }
    }
}
