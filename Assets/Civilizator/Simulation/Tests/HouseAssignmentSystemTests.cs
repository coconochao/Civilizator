using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class HouseAssignmentSystemTests
    {
        private HouseAssignmentSystem _system;
        private List<Agent> _agents;
        private List<Building> _buildings;

        [SetUp]
        public void Setup()
        {
            _system = new HouseAssignmentSystem(seed: 42); // Deterministic seed
            _agents = new List<Agent>();
            _buildings = new List<Building>();
        }

        [Test]
        public void FindUnassignedAdultsForHouse_ReturnsUpToTwoAdults()
        {
            // Create 5 adults
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult);
                _agents.Add(agent);
            }

            var result = _system.FindUnassignedAdultsForHouse(_agents, 2);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.LifeStage == LifeStage.Adult));
            Assert.IsTrue(result.All(a => !a.IsHouseAssigned));
        }

        [Test]
        public void FindUnassignedAdultsForHouse_ReturnsFewerthanTwo_WhenFewerUnavailable()
        {
            // Create only 1 unassigned adult
            var agent1 = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            _agents.Add(agent1);

            // Create 2 assigned adults (already in houses)
            var agent2 = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);
            agent2.AssignedHouseId = 999; // Assigned to some house
            _agents.Add(agent2);

            var agent3 = new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Adult);
            agent3.AssignedHouseId = 888;
            _agents.Add(agent3);

            var result = _system.FindUnassignedAdultsForHouse(_agents, 2);

            Assert.AreEqual(1, result.Count);
            Assert.Contains(agent1, result);
        }

        [Test]
        public void FindUnassignedAdultsForHouse_IgnoresChildren()
        {
            // Create 5 children
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Child);
                _agents.Add(agent);
            }

            var result = _system.FindUnassignedAdultsForHouse(_agents, 2);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindUnassignedAdultsForHouse_IgnoresElders()
        {
            // Create 5 elders
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Elder);
                _agents.Add(agent);
            }

            var result = _system.FindUnassignedAdultsForHouse(_agents, 2);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FindUnassignedAdultsForHouse_IgnoresDead()
        {
            // Create 5 adults, some dead
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult);
                if (i % 2 == 0)
                    agent.HitPoints = 0; // Dead
                _agents.Add(agent);
            }

            var result = _system.FindUnassignedAdultsForHouse(_agents, 2);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.IsAlive));
        }

        [Test]
        public void AssignAdultsToCompletedHouses_AssignsTwoAdultsToCompletedHouse()
        {
            // Create 5 unassigned adults
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult);
                _agents.Add(agent);
            }

            // Create a completed house
            var house = new Building(BuildingKind.House, new GridPos(10, 10));
            house.IsUnderConstruction = true;  // Mark as under construction
            int requiredCost = BuildingCostHelper.GetBuildCost(BuildingKind.House);
            house.ConstructionProgress = requiredCost;  // Set progress to required amount
            _buildings.Add(house);

            _system.AssignAdultsToCompletedHouses(_agents, _buildings);

            Assert.AreEqual(2, house.AdultResidentIds.Count);
        }

        [Test]
        public void AssignAdultsToCompletedHouses_IgnoresIncompleteHouses()
        {
            // Create 5 unassigned adults
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult);
                _agents.Add(agent);
            }

            // Create an incomplete house
            var house = new Building(BuildingKind.House, new GridPos(10, 10));
            house.IsUnderConstruction = true;
            house.ConstructionProgress = 0;  // Not completed - progress is 0
            _buildings.Add(house);

            _system.AssignAdultsToCompletedHouses(_agents, _buildings);

            // No assignment should occur since house is not completed
            Assert.AreEqual(0, house.AdultResidentIds.Count);
        }

        [Test]
        public void AssignAdultsToCompletedHouses_IgnoresNonHouseBuildings()
        {
            // Create 5 unassigned adults
            for (int i = 0; i < 5; i++)
            {
                var agent = new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult);
                _agents.Add(agent);
            }

            // Create a completed tower (not a house)
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));
            tower.IsUnderConstruction = true;
            int requiredCost = BuildingCostHelper.GetBuildCost(BuildingKind.Tower);
            tower.ConstructionProgress = requiredCost;
            _buildings.Add(tower);

            _system.AssignAdultsToCompletedHouses(_agents, _buildings);

            // Tower should not have any residents assigned
            Assert.AreEqual(0, tower.AdultResidentIds.Count);
        }

        [Test]
        public void AssignAgentsToHouse_UpdatesAgentAssignment()
        {
            var adult1 = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            var adult2 = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);
            var agentsToAssign = new List<Agent> { adult1, adult2 };

            var house = new Building(BuildingKind.House, new GridPos(10, 10));

            _system.AssignAgentsToHouse(agentsToAssign, house);

            Assert.AreEqual(2, house.AdultResidentIds.Count);
            Assert.IsTrue(adult1.IsHouseAssigned);
            Assert.IsTrue(adult2.IsHouseAssigned);
            Assert.IsNotNull(adult1.AssignedHouseId);
            Assert.IsNotNull(adult2.AssignedHouseId);
        }

        [Test]
        public void DeterministicRNG_SameSeedProducesSameAssignment()
        {
            // First run
            var agents1 = new List<Agent>();
            for (int i = 0; i < 10; i++)
            {
                agents1.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }

            var system1 = new HouseAssignmentSystem(seed: 123);
            var result1 = system1.FindUnassignedAdultsForHouse(agents1, 2);
            var ids1 = new List<int> { agents1.IndexOf(result1[0]), agents1.IndexOf(result1[1]) };
            ids1.Sort();

            // Second run with same seed
            var agents2 = new List<Agent>();
            for (int i = 0; i < 10; i++)
            {
                agents2.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }

            var system2 = new HouseAssignmentSystem(seed: 123);
            var result2 = system2.FindUnassignedAdultsForHouse(agents2, 2);
            var ids2 = new List<int> { agents2.IndexOf(result2[0]), agents2.IndexOf(result2[1]) };
            ids2.Sort();

            Assert.AreEqual(ids1, ids2, "Same seed should produce same random selection");
        }

        [Test]
        public void SetSeed_ChangesRandomBehavior()
        {
            var agents1 = new List<Agent>();
            for (int i = 0; i < 10; i++)
            {
                agents1.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }

            _system.SetSeed(100);
            var result1 = _system.FindUnassignedAdultsForHouse(agents1, 2);
            var id1 = agents1.IndexOf(result1[0]);

            var agents2 = new List<Agent>();
            for (int i = 0; i < 10; i++)
            {
                agents2.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }

            _system.SetSeed(200);
            var result2 = _system.FindUnassignedAdultsForHouse(agents2, 2);
            var id2 = agents2.IndexOf(result2[0]);

            Assert.AreNotEqual(id1, id2, "Different seed should produce different random selection");
        }

        [Test]
        public void AssignAdultsToCompletedHouses_HandlesMixedAgents()
        {
            // Mix of adults, children, elders, assigned and unassigned
            _agents.Add(new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult)); // Unassigned
            _agents.Add(new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult)); // Unassigned
            _agents.Add(new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Adult)); // Unassigned

            var assigned = new Agent(new GridPos(3, 0), Profession.Farmer, LifeStage.Adult);
            assigned.AssignedHouseId = 999;
            _agents.Add(assigned); // Already assigned

            _agents.Add(new Agent(new GridPos(4, 0), Profession.Soldier, LifeStage.Elder)); // Elder
            _agents.Add(new Agent(new GridPos(5, 0), Profession.Woodcutter, LifeStage.Child)); // Child

            var house = new Building(BuildingKind.House, new GridPos(10, 10));
            house.IsUnderConstruction = true;
            int requiredCost = BuildingCostHelper.GetBuildCost(BuildingKind.House);
            house.ConstructionProgress = requiredCost;
            _buildings.Add(house);

            _system.AssignAdultsToCompletedHouses(_agents, _buildings);

            // Should assign exactly 2 unassigned adults
            Assert.AreEqual(2, house.AdultResidentIds.Count);
            Assert.IsFalse(house.AdultResidentIds.Contains(999), "Already assigned adult should not be reassigned");
        }
    }
}
