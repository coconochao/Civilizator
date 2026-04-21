using System.Collections.Generic;
using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SoldierModeSwitchSystemTests
    {
        private SoldierModeSwitchSystem _system;
        private List<Agent> _agents;

        [SetUp]
        public void SetUp()
        {
            _system = new SoldierModeSwitchSystem();
            _agents = new List<Agent>();
        }

        [Test]
        public void SetPatrolTargetShare_InvalidValue_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.SetPatrolTargetShare(-0.1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _system.SetPatrolTargetShare(1.1f));
        }

        [Test]
        public void AdvanceCycle_EmptyList_ReturnsFalse()
        {
            bool switched = _system.AdvanceCycle(_agents);

            Assert.IsFalse(switched);
            Assert.AreEqual(1, _system.GetCurrentCycle());
        }

        [Test]
        public void AdvanceCycle_PatrolShareAboveTarget_SwitchesOneSoldierToImproving()
        {
            _system.SetPatrolTargetShare(0.25f);
            _system.SetSwitchThreshold(0.05f);

            for (int i = 0; i < 4; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Soldier, LifeStage.Adult)
                {
                    SoldierMode = SoldierMode.Patrolling
                });
            }

            bool switched = _system.AdvanceCycle(_agents);

            Assert.IsTrue(switched);
            Assert.AreEqual(3, SoldierModeSwitchSystem.GetModeCounts(_agents)[(int)SoldierMode.Patrolling]);
            Assert.AreEqual(1, SoldierModeSwitchSystem.GetModeCounts(_agents)[(int)SoldierMode.Improving]);
        }

        [Test]
        public void AdvanceCycle_LongRunTrendsTowardTargetShare()
        {
            _system.SetPatrolTargetShare(0.3f);
            _system.SetSwitchThreshold(0.15f);
            _system.SetSwitchCooldownCycles(0);

            for (int i = 0; i < 10; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Soldier, LifeStage.Adult)
                {
                    SoldierMode = SoldierMode.Patrolling
                });
            }

            for (int i = 0; i < 10; i++)
            {
                _system.AdvanceCycle(_agents);
            }

            int[] counts = SoldierModeSwitchSystem.GetModeCounts(_agents);

            Assert.That(counts[(int)SoldierMode.Patrolling], Is.EqualTo(3).Within(1));
            Assert.That(SoldierModeSwitchSystem.GetPatrolShare(_agents), Is.EqualTo(0.3f).Within(0.1f));
        }

        [Test]
        public void AdvanceCycle_CooldownPreventsImmediateModeFlip()
        {
            _system.SetPatrolTargetShare(0f);

            var soldier = new Agent(new GridPos(0, 0), Profession.Soldier, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Patrolling
            };
            _agents.Add(soldier);

            bool switchedToImproving = _system.AdvanceCycle(_agents);
            Assert.IsTrue(switchedToImproving);
            Assert.AreEqual(SoldierMode.Improving, soldier.SoldierMode);

            _system.SetPatrolTargetShare(1f);
            bool switchedBackImmediately = _system.AdvanceCycle(_agents);

            Assert.IsFalse(switchedBackImmediately);
            Assert.AreEqual(SoldierMode.Improving, soldier.SoldierMode);
        }

        [Test]
        public void GetModeCounts_IgnoresDeadAndNonSoldierAgents()
        {
            _agents.Add(new Agent(new GridPos(0, 0), Profession.Soldier, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Patrolling
            });
            _agents.Add(new Agent(new GridPos(1, 0), Profession.Soldier, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Improving,
                HitPoints = 0
            });
            _agents.Add(new Agent(new GridPos(2, 0), Profession.Builder, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Improving
            });

            int[] counts = SoldierModeSwitchSystem.GetModeCounts(_agents);

            Assert.AreEqual(1, counts[(int)SoldierMode.Patrolling]);
            Assert.AreEqual(0, counts[(int)SoldierMode.Improving]);
        }
    }
}
