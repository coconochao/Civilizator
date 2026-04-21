using NUnit.Framework;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class CombatSystemTests
    {
        [Test]
        public void ApplyAttackTick_ReducesAgentHitPointsAndClampsAtZero()
        {
            var target = new Agent(new GridPos(0, 0))
            {
                HitPoints = 3
            };

            CombatSystem.ApplyAttackTick(target, 2);

            Assert.AreEqual(1, target.HitPoints);

            CombatSystem.ApplyAttackTick(target, 5);

            Assert.AreEqual(0, target.HitPoints);
            Assert.IsFalse(target.IsAlive);
        }

        [Test]
        public void ApplyAttackTick_ReducesEnemyHitPointsAndClampsAtZero()
        {
            var target = new Enemy(new GridPos(0, 0))
            {
                HitPoints = 4
            };

            CombatSystem.ApplyAttackTick(target, 1);

            Assert.AreEqual(3, target.HitPoints);

            CombatSystem.ApplyAttackTick(target, 99);

            Assert.AreEqual(0, target.HitPoints);
            Assert.IsFalse(target.IsAlive);
        }

        [Test]
        public void ApplyAttackTick_ReducesBuildingHitPointsAndClampsAtZero()
        {
            var target = new Building(BuildingKind.House, new GridPos(0, 0))
            {
                HitPoints = 6
            };

            CombatSystem.ApplyAttackTick(target, 4);

            Assert.AreEqual(2, target.HitPoints);

            CombatSystem.ApplyAttackTick(target, 5);

            Assert.AreEqual(0, target.HitPoints);
        }

        [Test]
        public void ApplyAttackTick_RejectionsKeepDamageNonNegative()
        {
            var target = new Enemy(new GridPos(0, 0));

            Assert.Throws<System.ArgumentOutOfRangeException>(() => CombatSystem.ApplyAttackTick(target, -1));
        }
    }
}
