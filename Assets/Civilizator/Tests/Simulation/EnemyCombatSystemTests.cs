using NUnit.Framework;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class EnemyCombatSystemTests
    {
        [Test]
        public void EnemyStats_AreSetToV1Defaults()
        {
            var enemy = new Enemy(new GridPos(0, 0));

            Assert.AreEqual(10, EnemyCombatSystem.GetEnemyMaxHitPoints(enemy));
            Assert.AreEqual(1, EnemyCombatSystem.GetEnemyDamage(enemy));
            Assert.AreEqual(1f, EnemyCombatSystem.GetEnemyAttackIntervalSeconds(enemy));
            Assert.IsFalse(EnemyCombatSystem.DoesEnemyAttackMiss(enemy));
        }

        [Test]
        public void ApplyAttack_ReducesHitPointsByOnePerHit()
        {
            var target = new Agent(new GridPos(0, 0));

            EnemyCombatSystem.ApplyAttack(target);
            Assert.AreEqual(9, target.HitPoints);

            EnemyCombatSystem.ApplyAttack(target);
            Assert.AreEqual(8, target.HitPoints);
        }

        [Test]
        public void ApplyAttack_TenHitsKillTenHpTarget()
        {
            var target = new Agent(new GridPos(0, 0));

            for (int i = 0; i < 10; i++)
            {
                EnemyCombatSystem.ApplyAttack(target);
            }

            Assert.AreEqual(0, target.HitPoints);
            Assert.IsFalse(target.IsAlive);
        }

        [Test]
        public void ApplyAttack_CanDamageBuildingsUsingTheSharedCombatPipeline()
        {
            var target = new Building(BuildingKind.House, new GridPos(0, 0))
            {
                HitPoints = 2
            };

            EnemyCombatSystem.ApplyAttack(target);

            Assert.AreEqual(1, target.HitPoints);
            EnemyCombatSystem.ApplyAttack(target);
            Assert.AreEqual(0, target.HitPoints);
        }
    }
}
