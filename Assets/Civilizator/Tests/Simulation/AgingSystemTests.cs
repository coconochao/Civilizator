using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class AgingSystemTests
    {
        private AgingSystem _agingSystem;

        [SetUp]
        public void SetUp()
        {
            _agingSystem = new AgingSystem();
        }

        [Test]
        public void RegisterAgent_AddsAgentToSystem()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(agent);

            Assert.AreEqual(1, _agingSystem.GetRegisteredAgentCount());
        }

        [Test]
        public void RegisterAgent_NullAgent_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => _agingSystem.RegisterAgent(null));
        }

        [Test]
        public void RegisterAgent_SetsCorrectInitialCyclesDuration()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            var adult = new Agent(new GridPos(1, 0), Profession.Woodcutter, LifeStage.Adult);
            var elder = new Agent(new GridPos(2, 0), Profession.Woodcutter, LifeStage.Elder);

            _agingSystem.RegisterAgent(child);
            _agingSystem.RegisterAgent(adult);
            _agingSystem.RegisterAgent(elder);

            Assert.AreEqual(LifeStageHelpers.ChildToAdultCycles, _agingSystem.GetRemainingCycles(child));
            Assert.AreEqual(LifeStageHelpers.AdultToElderCycles, _agingSystem.GetRemainingCycles(adult));
            Assert.AreEqual(LifeStageHelpers.ElderToDeathCycles, _agingSystem.GetRemainingCycles(elder));
        }

        [Test]
        public void UnregisterAgent_RemovesAgentFromSystem()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(agent);
            Assert.AreEqual(1, _agingSystem.GetRegisteredAgentCount());

            _agingSystem.UnregisterAgent(agent);
            Assert.AreEqual(0, _agingSystem.GetRegisteredAgentCount());
        }

        [Test]
        public void AdvanceCycle_ChildAgents_DecrementCounter()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(child);
            int initialCycles = _agingSystem.GetRemainingCycles(child);

            _agingSystem.AdvanceCycle();
            int afterCycles = _agingSystem.GetRemainingCycles(child);

            Assert.AreEqual(initialCycles - 1, afterCycles);
            Assert.AreEqual(LifeStageHelpers.ChildToAdultCycles - 1, afterCycles);
        }

        [Test]
        public void AdvanceCycle_ChildToAdult_Transitions()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(child);

            // Advance 10 cycles (child duration)
            for (int i = 0; i < LifeStageHelpers.ChildToAdultCycles; i++)
            {
                _agingSystem.AdvanceCycle();
            }

            Assert.AreEqual(LifeStage.Adult, child.LifeStage);
        }

        [Test]
        public void AdvanceCycle_ReturnsTransitionedAgents()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(child);

            // Advance 9 cycles (no transition)
            for (int i = 0; i < 9; i++)
            {
                var transitioned = _agingSystem.AdvanceCycle();
                Assert.AreEqual(0, transitioned.Count);
            }

            // Advance 1 more cycle (transition happens)
            var transitionedAgents = _agingSystem.AdvanceCycle();
            Assert.AreEqual(1, transitionedAgents.Count);
            Assert.Contains(child, transitionedAgents.ToList());
        }

        [Test]
        public void AdvanceCycle_AdultToElder_Transitions()
        {
            var adult = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            _agingSystem.RegisterAgent(adult);

            // Advance 60 cycles
            for (int i = 0; i < LifeStageHelpers.AdultToElderCycles; i++)
            {
                _agingSystem.AdvanceCycle();
            }

            Assert.AreEqual(LifeStage.Elder, adult.LifeStage);
        }

        [Test]
        public void AdvanceCycle_ElderToDeath_SetHP0()
        {
            var elder = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            _agingSystem.RegisterAgent(elder);
            Assert.IsTrue(elder.IsAlive);

            // Advance 10 cycles
            for (int i = 0; i < LifeStageHelpers.ElderToDeathCycles; i++)
            {
                _agingSystem.AdvanceCycle();
            }

            Assert.AreEqual(0, elder.HitPoints);
            Assert.IsFalse(elder.IsAlive);
        }

        [Test]
        public void AdvanceCycle_ElderToDeath_UnregistersAgent()
        {
            var elder = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Elder);
            _agingSystem.RegisterAgent(elder);
            Assert.AreEqual(1, _agingSystem.GetRegisteredAgentCount());

            // Advance 10 cycles
            for (int i = 0; i < LifeStageHelpers.ElderToDeathCycles; i++)
            {
                _agingSystem.AdvanceCycle();
            }

            Assert.AreEqual(0, _agingSystem.GetRegisteredAgentCount());
        }

        [Test]
        public void AdvanceCycle_MultipleAgents_IndependentCounters()
        {
            var child1 = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            var child2 = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Child);
            _agingSystem.RegisterAgent(child1);
            _agingSystem.RegisterAgent(child2);

            // Advance 5 cycles
            for (int i = 0; i < 5; i++)
            {
                _agingSystem.AdvanceCycle();
            }

            Assert.AreEqual(LifeStageHelpers.ChildToAdultCycles - 5, _agingSystem.GetRemainingCycles(child1));
            Assert.AreEqual(LifeStageHelpers.ChildToAdultCycles - 5, _agingSystem.GetRemainingCycles(child2));
        }

        [Test]
        public void AdvanceCycle_FullProgression_ChildToAdultToElderToDeath()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(agent);

            // Child stage: 10 cycles
            Assert.AreEqual(LifeStage.Child, agent.LifeStage);
            for (int i = 0; i < 10; i++)
                _agingSystem.AdvanceCycle();
            Assert.AreEqual(LifeStage.Adult, agent.LifeStage);

            // Adult stage: 60 cycles
            for (int i = 0; i < 60; i++)
                _agingSystem.AdvanceCycle();
            Assert.AreEqual(LifeStage.Elder, agent.LifeStage);

            // Elder stage: 10 cycles
            for (int i = 0; i < 10; i++)
                _agingSystem.AdvanceCycle();
            Assert.AreEqual(LifeStage.Elder, agent.LifeStage);
            Assert.AreEqual(0, agent.HitPoints);
            Assert.IsFalse(agent.IsAlive);
        }

        [Test]
        public void GetRemainingCycles_UnregisteredAgent_ReturnsZero()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            Assert.AreEqual(0, _agingSystem.GetRemainingCycles(agent));
        }

        [Test]
        public void GetRemainingCycles_NullAgent_ReturnsZero()
        {
            Assert.AreEqual(0, _agingSystem.GetRemainingCycles(null));
        }

        [Test]
        public void Clear_RemovesAllAgents()
        {
            var agent1 = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            var agent2 = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);
            var agent3 = new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Elder);

            _agingSystem.RegisterAgent(agent1);
            _agingSystem.RegisterAgent(agent2);
            _agingSystem.RegisterAgent(agent3);
            Assert.AreEqual(3, _agingSystem.GetRegisteredAgentCount());

            _agingSystem.Clear();
            Assert.AreEqual(0, _agingSystem.GetRegisteredAgentCount());
        }

        [Test]
        public void GetRegisteredAgents_ReturnsAllAgents()
        {
            var agent1 = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            var agent2 = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);

            _agingSystem.RegisterAgent(agent1);
            _agingSystem.RegisterAgent(agent2);

            var registered = _agingSystem.GetRegisteredAgents().ToList();
            Assert.AreEqual(2, registered.Count);
            Assert.Contains(agent1, registered);
            Assert.Contains(agent2, registered);
        }

        [Test]
        public void TransitionCycle_ResetCounterForNewStage()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(child);

            // Advance to transition
            for (int i = 0; i < LifeStageHelpers.ChildToAdultCycles; i++)
                _agingSystem.AdvanceCycle();

            // After transition, counter should be reset to Adult duration
            Assert.AreEqual(LifeStage.Adult, child.LifeStage);
            Assert.AreEqual(LifeStageHelpers.AdultToElderCycles, _agingSystem.GetRemainingCycles(child));
        }

        [Test]
        public void RegisterAgent_MultipleRegistrations_LastOneWins()
        {
            var agent = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            _agingSystem.RegisterAgent(agent);
            _agingSystem.RegisterAgent(agent); // Register again

            Assert.AreEqual(1, _agingSystem.GetRegisteredAgentCount());
        }

        [Test]
        public void AdvanceCycle_MixedAges_TransitionsAtDifferentTimes()
        {
            var newChild = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            var midChild = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Child);
            
            _agingSystem.RegisterAgent(newChild);
            _agingSystem.AdvanceCycle();
            _agingSystem.RegisterAgent(midChild);

            // Manually set midChild to 5 cycles remaining
            // (In real usage, this would be done through the system)
            for (int i = 0; i < 5; i++)
                _agingSystem.AdvanceCycle();

            // midChild is now at 5 cycles remaining
            Assert.AreEqual(5, _agingSystem.GetRemainingCycles(midChild));

            // Advance newChild 4 more cycles (total 10 for transition)
            for (int i = 0; i < 4; i++)
                _agingSystem.AdvanceCycle();

            // newChild should have transitioned
            Assert.AreEqual(LifeStage.Adult, newChild.LifeStage);
            // midChild should transition next cycle
            Assert.AreEqual(LifeStage.Child, midChild.LifeStage);
            Assert.AreEqual(1, _agingSystem.GetRemainingCycles(midChild));
        }
    }
}
