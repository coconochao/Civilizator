using NUnit.Framework;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SimulationFacadeTests
    {
        private World world;
        private SimulationFacade facade;

        [SetUp]
        public void SetUp()
        {
            world = new World();
            world.Initialize(seed: 42);
            facade = new SimulationFacade(world);
        }

        [Test]
        public void CurrentCycleReturnsWorldClockCycle()
        {
            world.Clock.Advance(SimulationClock.SecondsPerCycle * 2);
            
            Assert.AreEqual(2, facade.CurrentCycle);
        }

        [Test]
        public void TotalSimulationSecondsReturnsWorldClockTotal()
        {
            world.Clock.Advance(30f);
            
            Assert.AreEqual(30f, facade.TotalSimulationSeconds);
        }

        [Test]
        public void CentralStocksReturnsCurrentStorage()
        {
            world.Storage.Deposit(ResourceKind.Logs, 100);
            world.Storage.Deposit(ResourceKind.Ore, 50);
            world.Storage.Deposit(ResourceKind.Meat, 25);
            world.Storage.Deposit(ResourceKind.PlantFood, 75);
            
            var stocks = facade.CentralStocks;
            
            Assert.AreEqual(100, stocks.logs);
            Assert.AreEqual(50, stocks.ore);
            Assert.AreEqual(25, stocks.meat);
            Assert.AreEqual(75, stocks.plantFood);
        }

        [Test]
        public void TotalPopulationReturnsAgentCount()
        {
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter);
            world.AddAgent(new GridPos(2, 2), Profession.Woodcutter);
            world.AddAgent(new GridPos(3, 3), Profession.Woodcutter);
            
            Assert.AreEqual(3, facade.TotalPopulation);
        }

        [Test]
        public void PopulationByStageReturnsCorrectCounts()
        {
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Child);
            world.AddAgent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Adult);
            world.AddAgent(new GridPos(3, 3), Profession.Woodcutter, LifeStage.Adult);
            world.AddAgent(new GridPos(4, 4), Profession.Woodcutter, LifeStage.Elder);
            
            var breakdown = facade.PopulationByStage;
            
            Assert.AreEqual(1, breakdown.children);
            Assert.AreEqual(2, breakdown.adults);
            Assert.AreEqual(1, breakdown.elders);
        }

        [Test]
        public void HousingStatsReturnsCorrectCounts()
        {
            // Add some agents
            var adult1 = world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Adult);
            var adult2 = world.AddAgent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Adult);
            var child1 = world.AddAgent(new GridPos(3, 3), Profession.Woodcutter, LifeStage.Child);
            
            // Add a house and assign adults
            var house = world.AddBuilding(BuildingKind.House, new GridPos(10, 10));
            adult1.AssignedHouseId = house.Id;
            adult2.AssignedHouseId = house.Id;
            
            var stats = facade.HousingStats;
            
            Assert.AreEqual(2, stats.assignedAdults);
            Assert.AreEqual(0, stats.unassignedAdults);
            Assert.AreEqual(1, stats.totalHouses);
        }

        [Test]
        public void ProfessionCountsReturnsCorrectDistribution()
        {
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter);
            world.AddAgent(new GridPos(2, 2), Profession.Woodcutter);
            world.AddAgent(new GridPos(3, 3), Profession.Miner);
            world.AddAgent(new GridPos(4, 4), Profession.Hunter);
            world.AddAgent(new GridPos(5, 5), Profession.Farmer);
            world.AddAgent(new GridPos(6, 6), Profession.Builder);
            world.AddAgent(new GridPos(7, 7), Profession.Soldier);
            
            var counts = facade.ProfessionCounts;
            
            Assert.AreEqual(2, counts.woodcutters);
            Assert.AreEqual(1, counts.miners);
            Assert.AreEqual(1, counts.hunters);
            Assert.AreEqual(1, counts.farmers);
            Assert.AreEqual(1, counts.builders);
            Assert.AreEqual(1, counts.soldiers);
        }

        [Test]
        public void ProfessionTargetsReturnsWorldTargets()
        {
            world.ProfessionTargets.SetTarget(Profession.Woodcutter, 0.3f);
            world.ProfessionTargets.SetTarget(Profession.Miner, 0.2f);
            world.ProfessionTargets.SetTarget(Profession.Hunter, 0.5f);
            world.ProfessionTargets.SetTarget(Profession.Farmer, 0.0f);
            world.ProfessionTargets.SetTarget(Profession.Builder, 0.0f);
            world.ProfessionTargets.SetTarget(Profession.Soldier, 0.0f);
            world.ProfessionTargets.Normalize();
            
            var targets = facade.ProfessionTargets;
            
            Assert.AreEqual(0.3f, targets.woodcutter, 0.001f);
            Assert.AreEqual(0.2f, targets.miner, 0.001f);
        }

        [Test]
        public void AverageProductivityCalculatesCorrectly()
        {
            var agent1 = world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Adult);
            var agent2 = world.AddAgent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Child);
            
            var avg = facade.AverageProductivity;
            
            // Adult has 1.0 base, Child has 0.5 base, average should be 0.75
            Assert.AreEqual(0.75f, avg, 0.001f);
        }

        [Test]
        public void AverageProductivityReturnsZeroWhenNoAgents()
        {
            var avg = facade.AverageProductivity;
            
            Assert.AreEqual(0f, avg);
        }

        [Test]
        public void StarvingAgentCountReturnsZeroWhenAllFed()
        {
            world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Adult);

            var count = facade.StarvingAgentCount;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void StarvingAgentCountReturnsCountWhenStarving()
        {
            var agent1 = world.AddAgent(new GridPos(1, 1), Profession.Woodcutter, LifeStage.Adult);
            var agent2 = world.AddAgent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Adult);

            // Set starvation penalty to 1.0 (dead from starvation)
            agent1.EatingState.StarvationPenalty = 1.0f;
            agent2.EatingState.StarvationPenalty = 1.0f;

            var count = facade.StarvingAgentCount;

            Assert.AreEqual(2, count);
        }

        [Test]
        public void IsGameOverReturnsWorldGameOverState()
        {
            Assert.IsFalse(facade.IsGameOver);

            world.GameOver.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);

            Assert.IsTrue(facade.IsGameOver);
        }

        [Test]
        public void GameOverReasonReturnsWorldReason()
        {
            world.GameOver.MarkGameOver(GameOverState.GameOverReason.CentralDestroyed);

            Assert.AreEqual(GameOverState.GameOverReason.CentralDestroyed, facade.GameOverReason);
        }

        [Test]
        public void ReproductionRateReturnsWorldSetting()
        {
            world.ReproductionSettings.SetReproductionRate(0.5f);
            
            Assert.AreEqual(0.5f, facade.ReproductionRate);
        }

        [Test]
        public void SoldierPatrolPercentageReturnsWorldSetting()
        {
            world.SoldierControls.SetPatrolTargetShare(0.7f);

            Assert.AreEqual(0.7f, facade.SoldierPatrolPercentage);
        }

        [Test]
        public void TowerBuildEmphasisReturnsWorldSetting()
        {
            world.SoldierControls.SetTowerBuildEmphasis(0.8f);

            Assert.AreEqual(0.8f, facade.TowerBuildEmphasis);
        }

        [Test]
        public void StaffedTowerCountReturnsZeroWhenNoTowers()
        {
            var count = facade.StaffedTowerCount;
            
            Assert.AreEqual(0, count);
        }

        [Test]
        public void StaffedTowerCountReturnsZeroWhenTowerUnstaffed()
        {
            world.AddBuilding(BuildingKind.Tower, new GridPos(10, 10));
            
            var count = facade.StaffedTowerCount;
            
            Assert.AreEqual(0, count);
        }

        [Test]
        public void StaffedTowerCountReturnsOneWhenTowerStaffed()
        {
            var tower = world.AddBuilding(BuildingKind.Tower, new GridPos(10, 10));
            var occupiedTiles = new System.Collections.Generic.List<GridPos>();
            tower.GetOccupiedTiles(occupiedTiles);
            var soldier = world.AddAgent(occupiedTiles[0], Profession.Soldier);

            var count = facade.StaffedTowerCount;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void SoldierModeCountsReturnsCorrectSplits()
        {
            var soldier1 = world.AddAgent(new GridPos(1, 1), Profession.Soldier);
            soldier1.SoldierMode = SoldierMode.Patrolling;

            var soldier2 = world.AddAgent(new GridPos(2, 2), Profession.Soldier);
            soldier2.SoldierMode = SoldierMode.Improving;

            var soldier3 = world.AddAgent(new GridPos(3, 3), Profession.Soldier);
            soldier3.SoldierMode = SoldierMode.Patrolling;

            var counts = facade.SoldierModeCounts;

            Assert.AreEqual(2, counts.patrol);
            Assert.AreEqual(1, counts.improve);
        }

        [Test]
        public void SoldierModeCountsReturnsZeroWhenNoSoldiers()
        {
            var counts = facade.SoldierModeCounts;
            
            Assert.AreEqual(0, counts.patrol);
            Assert.AreEqual(0, counts.improve);
        }
    }
}
