using NUnit.Framework;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class EnemyAISystemTests
    {
        [Test]
        public void FindBestTarget_AttackingPersonTakesPriorityOverTowerAndCivilian()
        {
            var enemy = new Enemy(new GridPos(0, 0));
            var attackingPeople = new List<Agent>
            {
                new Agent(new GridPos(8, 8), Profession.Builder, LifeStage.Adult)
            };
            var attackingTowers = new List<Building>
            {
                new Building(BuildingKind.Tower, new GridPos(1, 1))
            };
            var civilians = new List<Agent>
            {
                new Agent(new GridPos(2, 2), Profession.Woodcutter, LifeStage.Adult)
            };
            var buildings = new List<Building>
            {
                new Building(BuildingKind.House, new GridPos(3, 3))
            };

            var target = EnemyAISystem.FindBestTarget(enemy, attackingPeople, attackingTowers, civilians, buildings);

            Assert.IsNotNull(target);
            Assert.AreEqual(EnemyAISystem.EnemyTargetKind.AttackingPerson, target.Kind);
            Assert.AreEqual(attackingPeople[0], target.Agent);
        }

        [Test]
        public void FindBestTarget_AttackingTowerTakesPriorityOverCivilianAndBuilding()
        {
            var enemy = new Enemy(new GridPos(0, 0));
            var attackingPeople = new List<Agent>();
            var attackingTowers = new List<Building>
            {
                new Building(BuildingKind.Tower, new GridPos(5, 5))
            };
            var civilians = new List<Agent>
            {
                new Agent(new GridPos(1, 1), Profession.Builder, LifeStage.Adult)
            };
            var buildings = new List<Building>
            {
                new Building(BuildingKind.House, new GridPos(2, 2))
            };

            var target = EnemyAISystem.FindBestTarget(enemy, attackingPeople, attackingTowers, civilians, buildings);

            Assert.IsNotNull(target);
            Assert.AreEqual(EnemyAISystem.EnemyTargetKind.AttackingTower, target.Kind);
            Assert.AreEqual(attackingTowers[0], target.Building);
        }

        [Test]
        public void FindBestTarget_WhenNoAttackers_PicksNearestCivilianOrBuilding()
        {
            var enemy = new Enemy(new GridPos(0, 0));
            var civilians = new List<Agent>
            {
                new Agent(new GridPos(6, 6), Profession.Builder, LifeStage.Adult)
            };
            var buildings = new List<Building>
            {
                new Building(BuildingKind.House, new GridPos(2, 2))
            };

            var target = EnemyAISystem.FindBestTarget(
                enemy,
                new List<Agent>(),
                new List<Building>(),
                civilians,
                buildings);

            Assert.IsNotNull(target);
            Assert.AreEqual(EnemyAISystem.EnemyTargetKind.Building, target.Kind);
            Assert.AreEqual(buildings[0], target.Building);
        }

        [Test]
        public void AdvanceEnemy_MovesOneTileUsingFourWayPathing()
        {
            var enemy = new Enemy(new GridPos(0, 0));
            var occupancy = new GridOccupancy(10, 10);
            occupancy.BlockTile(new GridPos(1, 0));

            var civilians = new List<Agent>
            {
                new Agent(new GridPos(2, 0), Profession.Builder, LifeStage.Adult)
            };

            bool moved = EnemyAISystem.AdvanceEnemy(
                enemy,
                occupancy,
                new List<Agent>(),
                new List<Building>(),
                civilians,
                new List<Building>());

            Assert.IsTrue(moved);
            Assert.AreEqual(new GridPos(0, 1), enemy.Position);
        }

        [Test]
        public void AdvanceEnemy_TargetsNearestReachableTileForBuilding()
        {
            var enemy = new Enemy(new GridPos(0, 0));
            var occupancy = new GridOccupancy(10, 10);
            var building = new Building(BuildingKind.House, new GridPos(0, 2));
            var occupiedTiles = new List<GridPos>();
            building.GetOccupiedTiles(occupiedTiles);
            foreach (var tile in occupiedTiles)
            {
                occupancy.BlockTile(tile);
            }

            var buildings = new List<Building> { building };

            bool moved = EnemyAISystem.AdvanceEnemy(
                enemy,
                occupancy,
                new List<Agent>(),
                new List<Building>(),
                new List<Agent>(),
                buildings);

            Assert.IsTrue(moved);
            Assert.AreEqual(new GridPos(0, 1), enemy.Position);
        }
    }
}
