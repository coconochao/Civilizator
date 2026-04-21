using System.Collections.Generic;
using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class TowerCombatSystemTests
    {
        [Test]
        public void GetTowerHitArea_ReturnsSixBySixAreaAroundFootprint()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));

            List<GridPos> area = TowerCombatSystem.GetTowerHitArea(tower);

            Assert.AreEqual(36, area.Count);
            CollectionAssert.Contains(area, new GridPos(8, 8));
            CollectionAssert.Contains(area, new GridPos(13, 13));
            CollectionAssert.DoesNotContain(area, new GridPos(7, 8));
            CollectionAssert.DoesNotContain(area, new GridPos(14, 13));
        }

        [Test]
        public void CanTowerFire_RequiresLivingSoldierInsideFootprint()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));
            var agents = new List<Agent>
            {
                new Agent(new GridPos(10, 10), Profession.Soldier, LifeStage.Adult)
            };

            Assert.IsTrue(TowerCombatSystem.CanTowerFire(tower, agents));
            Assert.IsTrue(TowerCombatSystem.HasSoldierInside(tower, agents));
        }

        [Test]
        public void CanTowerFire_ReturnsFalseWithoutSoldierOrWithDeadSoldier()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));
            var noSoldier = new List<Agent>
            {
                new Agent(new GridPos(10, 10), Profession.Builder, LifeStage.Adult)
            };
            var deadSoldier = new Agent(new GridPos(10, 10), Profession.Soldier, LifeStage.Adult)
            {
                HitPoints = 0
            };

            Assert.IsFalse(TowerCombatSystem.CanTowerFire(tower, noSoldier));
            Assert.IsFalse(TowerCombatSystem.CanTowerFire(tower, new List<Agent> { deadSoldier }));
        }

        [Test]
        public void TowerStats_BaseTowerHasExpectedHpDamageAndCadence()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));

            Assert.AreEqual(100, tower.HitPoints);
            Assert.AreEqual(100, TowerCombatSystem.GetTowerMaxHitPoints(tower));
            Assert.AreEqual(1, TowerCombatSystem.GetTowerDamage(tower));
            Assert.AreEqual(1f, TowerCombatSystem.GetTowerAttackIntervalSeconds(tower));
        }

        [Test]
        public void TowerStats_UpgradedTowerDealsDoubleDamage()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));
            tower.UpgradeLevel = 1;

            Assert.AreEqual(2, TowerCombatSystem.GetTowerDamage(tower));
            Assert.AreEqual(100, TowerCombatSystem.GetTowerMaxHitPoints(tower));
        }

        [Test]
        public void TowerStats_BaseDamageKillsTenHpEnemyInTenSeconds()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));

            int damage = TowerCombatSystem.GetTowerDamage(tower);
            float cadence = TowerCombatSystem.GetTowerAttackIntervalSeconds(tower);
            int enemyHitPoints = 10;

            float timeToKill = (enemyHitPoints / (float)damage) * cadence;

            Assert.AreEqual(10f, timeToKill);
        }

        [Test]
        public void IsEnemyInRange_IncludesBoundaryTilesAndExcludesOutsideTiles()
        {
            var tower = new Building(BuildingKind.Tower, new GridPos(10, 10));

            Assert.IsTrue(TowerCombatSystem.IsEnemyInRange(tower, new GridPos(8, 8)));
            Assert.IsTrue(TowerCombatSystem.IsEnemyInRange(tower, new GridPos(13, 13)));
            Assert.IsFalse(TowerCombatSystem.IsEnemyInRange(tower, new GridPos(7, 8)));
            Assert.IsFalse(TowerCombatSystem.IsEnemyInRange(tower, new GridPos(14, 13)));
        }
    }
}
