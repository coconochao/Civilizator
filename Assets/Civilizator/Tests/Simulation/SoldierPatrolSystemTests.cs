using System.Collections.Generic;
using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SoldierPatrolSystemTests
    {
        [Test]
        public void GetPatrolPositions_CentralOnly_ReturnsOuterDiamondPositions()
        {
            var central = new Building(BuildingKind.Central, new GridPos(10, 10));
            var buildings = new List<Building> { central };

            List<GridPos> positions = SoldierPatrolSystem.GetPatrolPositions(central, buildings);

            Assert.AreEqual(4, positions.Count);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    new GridPos(11, 9),
                    new GridPos(13, 11),
                    new GridPos(11, 13),
                    new GridPos(9, 11)
                },
                positions);
        }

        [Test]
        public void GetPatrolPositions_ExcludesOccupiedTiles()
        {
            var central = new Building(BuildingKind.Central, new GridPos(10, 10));
            var buildings = new List<Building>
            {
                central,
                new Building(BuildingKind.House, new GridPos(14, 10))
            };

            List<GridPos> positions = SoldierPatrolSystem.GetPatrolPositions(central, buildings);

            Assert.IsFalse(positions.Contains(new GridPos(14, 10)));
            Assert.IsFalse(positions.Contains(new GridPos(15, 10)));
            Assert.IsTrue(positions.Contains(new GridPos(16, 11)));
        }

        [Test]
        public void AssignPatrolPositions_RoundsRobinAcrossAvailableTiles()
        {
            var central = new Building(BuildingKind.Central, new GridPos(10, 10));
            var buildings = new List<Building> { central };
            var soldiers = new List<Agent>
            {
                new Agent(new GridPos(0, 0), Profession.Soldier, LifeStage.Adult),
                new Agent(new GridPos(0, 1), Profession.Soldier, LifeStage.Adult),
                new Agent(new GridPos(0, 2), Profession.Soldier, LifeStage.Adult),
                new Agent(new GridPos(0, 3), Profession.Soldier, LifeStage.Adult),
                new Agent(new GridPos(0, 4), Profession.Soldier, LifeStage.Adult),
                new Agent(new GridPos(0, 5), Profession.Soldier, LifeStage.Adult)
            };

            Dictionary<int, GridPos> assignments = SoldierPatrolSystem.AssignPatrolPositions(soldiers, central, buildings);
            List<GridPos> patrolPositions = SoldierPatrolSystem.GetPatrolPositions(central, buildings);

            Assert.AreEqual(soldiers.Count, assignments.Count);
            Assert.AreEqual(4, patrolPositions.Count);
            Assert.AreEqual(assignments[soldiers[0].Id], assignments[soldiers[4].Id]);
            Assert.AreEqual(assignments[soldiers[1].Id], assignments[soldiers[5].Id]);

            var uniqueAssignedPositions = new HashSet<GridPos>(assignments.Values);
            Assert.AreEqual(4, uniqueAssignedPositions.Count);
        }
    }
}
