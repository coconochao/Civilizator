using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Civilizator.Simulation.Tests
{
    /// <summary>
    /// Tests for ProfessionSwitchSystem.
    /// </summary>
    [TestFixture]
    public class ProfessionSwitchSystemTests
    {
        private ProfessionTargets _targets;
        private ProfessionSwitchSystem _system;
        private List<Agent> _agents;

        [SetUp]
        public void SetUp()
        {
            _targets = new ProfessionTargets();
            _system = new ProfessionSwitchSystem(_targets);
            _agents = new List<Agent>();
        }

        [Test]
        public void Constructor_WithNullTargets_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfessionSwitchSystem(null));
        }

        [Test]
        public void SetSwitchThreshold_ValidValue_SetsThreshold()
        {
            _system.SetSwitchThreshold(0.2f);
            // We can't directly read the threshold, but we can test behavior
            // This test mainly ensures no exception is thrown
            Assert.DoesNotThrow(() => _system.SetSwitchThreshold(0.2f));
        }

        [Test]
        public void SetSwitchThreshold_InvalidValue_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.SetSwitchThreshold(-0.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.SetSwitchThreshold(1.5f));
        }

        [Test]
        public void SetSwitchCooldownCycles_ValidValue_SetsCooldown()
        {
            _system.SetSwitchCooldownCycles(10);
            Assert.DoesNotThrow(() => _system.SetSwitchCooldownCycles(10));
        }

        [Test]
        public void SetSwitchCooldownCycles_InvalidValue_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _system.SetSwitchCooldownCycles(-1));
        }

        [Test]
        public void AdvanceCycle_EmptyList_ReturnsFalse()
        {
            bool switched = _system.AdvanceCycle(_agents);
            Assert.That(switched, Is.False);
        }

        [Test]
        public void AdvanceCycle_CooldownPreventsSwitch()
        {
            _agents.Add(new Agent(new GridPos(9, 0), Profession.Hunter, LifeStage.Adult));

            // First switch should succeed
            bool switched1 = _system.AdvanceCycle(_agents);
            Assert.That(switched1, Is.True);

            // Second switch should fail due to cooldown (default 5 cycles)
            bool switched2 = _system.AdvanceCycle(_agents);
            Assert.That(switched2, Is.False);
        }

        [Test]
        public void AdvanceCycle_CooldownExpires_AllowsSwitch()
        {
            _agents.Add(new Agent(new GridPos(9, 0), Profession.Hunter, LifeStage.Adult));

            // First switch
            _system.AdvanceCycle(_agents);

            // Advance 4 cycles
            for (int i = 0; i < 4; i++)
            {
                _system.AdvanceCycle(_agents);
            }

            // Now another switch should be possible
            bool switched = _system.AdvanceCycle(_agents);
            Assert.That(switched, Is.True);
        }

        [Test]
        public void AdvanceCycle_ImbalanceBelowThreshold_ReturnsFalse()
        {
            // Create one agent of each profession, plus one extra Woodcutter
            // This creates a small imbalance below default threshold (15%)
            // Woodcutter: 2/7 = 28.6%, Target: 16.7%, Discrepancy: 11.9% (below 15% threshold)
            // All other professions: 1/7 = 14.3%, Target: 16.7%, Discrepancy: -2.4%
            for (int i = 0; i < 6; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), (Profession)i, LifeStage.Adult));
            }
            // Add one extra Woodcutter
            _agents.Add(new Agent(new GridPos(6, 0), Profession.Woodcutter, LifeStage.Adult));

            bool switched = _system.AdvanceCycle(_agents);
            Assert.That(switched, Is.False);
        }

        [Test]
        public void GetCurrentCycle_IncrementsEachAdvance()
        {
            Assert.That(_system.GetCurrentCycle(), Is.EqualTo(0));

            _system.AdvanceCycle(_agents);
            Assert.That(_system.GetCurrentCycle(), Is.EqualTo(1));

            _system.AdvanceCycle(_agents);
            Assert.That(_system.GetCurrentCycle(), Is.EqualTo(2));
        }

        [Test]
        public void Reset_ClearsState()
        {
            // Create some agents and perform a switch
            for (int i = 0; i < 8; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }
            _agents.Add(new Agent(new GridPos(8, 0), Profession.Miner, LifeStage.Adult));
            _agents.Add(new Agent(new GridPos(9, 0), Profession.Hunter, LifeStage.Adult));

            _system.AdvanceCycle(_agents);
            Assert.That(_system.GetSwitchedAgentCount(), Is.GreaterThan(0));

            _system.Reset();
            Assert.That(_system.GetSwitchedAgentCount(), Is.EqualTo(0));
            Assert.That(_system.GetCurrentCycle(), Is.EqualTo(0));
        }

        [Test]
        public void GetLastSwitchCycle_ReturnsCorrectCycle()
        {
            // Create agents and perform a switch
            for (int i = 0; i < 8; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }

            _system.AdvanceCycle(_agents);
            var switchedAgent = _agents.Find(a => a.Profession != Profession.Woodcutter);
            var notSwitchedAgent = _agents.Find(a => a.Profession == Profession.Woodcutter);

            Assert.That(_system.GetLastSwitchCycle(switchedAgent), Is.EqualTo(1)); // Just switched
            Assert.That(_system.GetLastSwitchCycle(notSwitchedAgent), Is.EqualTo(0)); // Never switched
        }

        [Test]
        public void GetSwitchedAgentCount_ReturnsCorrectCount()
        {
            // Create agents and perform multiple switches
            for (int i = 0; i < 12; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }
            _agents.Add(new Agent(new GridPos(12, 0), Profession.Miner, LifeStage.Adult));
            _agents.Add(new Agent(new GridPos(13, 0), Profession.Hunter, LifeStage.Adult));

            // Perform switches (advance enough cycles to allow multiple switches)
            for (int i = 0; i < 10; i++)
            {
                _system.AdvanceCycle(_agents);
            }

            int switchedCount = _system.GetSwitchedAgentCount();
            Assert.That(switchedCount, Is.GreaterThan(0));
            Assert.That(switchedCount, Is.LessThanOrEqualTo(_agents.Count));
        }

        [Test]
        public void CustomThreshold_LowerThanDefault_AllowsMoreSwitches()
        {
            _system.SetSwitchThreshold(0.05f); // 5% threshold

            // Create 6 agents: 4 Woodcutters, 2 Miners
            for (int i = 0; i < 4; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }
            for (int i = 0; i < 2; i++)
            {
                _agents.Add(new Agent(new GridPos(i + 4, 0), Profession.Miner, LifeStage.Adult));
            }

            bool switched = _system.AdvanceCycle(_agents);
            Assert.That(switched, Is.True);
        }

        [Test]
        public void CustomThreshold_HigherThanDefault_PreventsSwitches()
        {
            _system.SetSwitchThreshold(0.5f); // 50% threshold

            // Create 8 Woodcutters, 1 Miner, 1 Hunter (same as other test)
            for (int i = 0; i < 8; i++)
            {
                _agents.Add(new Agent(new GridPos(i, 0), Profession.Woodcutter, LifeStage.Adult));
            }
            _agents.Add(new Agent(new GridPos(8, 0), Profession.Miner, LifeStage.Adult));
            _agents.Add(new Agent(new GridPos(9, 0), Profession.Hunter, LifeStage.Adult));

            bool switched = _system.AdvanceCycle(_agents);
            Assert.That(switched, Is.False);
        }
    }
}