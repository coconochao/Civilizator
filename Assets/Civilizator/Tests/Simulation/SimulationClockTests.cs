using NUnit.Framework;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SimulationClockTests
    {
        [Test]
        public void ClockStartsAtZero()
        {
            var clock = new SimulationClock();
            Assert.AreEqual(0, clock.CurrentCycle);
            Assert.AreEqual(0f, clock.AccumulatedSeconds);
        }

        [Test]
        public void AdvanceWithinCycleAccumulates()
        {
            var clock = new SimulationClock();
            clock.Advance(30f);

            Assert.AreEqual(0, clock.CurrentCycle);
            Assert.AreEqual(30f, clock.AccumulatedSeconds);
        }

        [Test]
        public void AdvanceExactlyOneFullCycleIncrementsCounter()
        {
            var clock = new SimulationClock();
            clock.Advance(SimulationClock.SecondsPerCycle);

            Assert.AreEqual(1, clock.CurrentCycle);
            Assert.AreEqual(0f, clock.AccumulatedSeconds);
        }

        [Test]
        public void AdvanceMultipleCycles()
        {
            var clock = new SimulationClock();
            clock.Advance(SimulationClock.SecondsPerCycle * 3);

            Assert.AreEqual(3, clock.CurrentCycle);
            Assert.AreEqual(0f, clock.AccumulatedSeconds);
        }

        [Test]
        public void AdvanceWithRemainder()
        {
            var clock = new SimulationClock();
            clock.Advance(SimulationClock.SecondsPerCycle + 30f);

            Assert.AreEqual(1, clock.CurrentCycle);
            Assert.AreEqual(30f, clock.AccumulatedSeconds);
        }

        [Test]
        public void AccumulateAndMultipleTicks()
        {
            var clock = new SimulationClock();
            clock.Advance(30f);
            Assert.AreEqual(0, clock.CurrentCycle);

            clock.Advance(40f);
            Assert.AreEqual(1, clock.CurrentCycle);
            Assert.AreEqual(10f, clock.AccumulatedSeconds);
        }

        [Test]
        public void NegativeDeltaTimeIgnored()
        {
            var clock = new SimulationClock();
            clock.Advance(30f);
            float before = clock.AccumulatedSeconds;

            clock.Advance(-10f);
            Assert.AreEqual(before, clock.AccumulatedSeconds);
        }

        [Test]
        public void TotalSimulationSecondsCalculatesCorrectly()
        {
            var clock = new SimulationClock();
            clock.Advance(SimulationClock.SecondsPerCycle + 30f);

            float expected = SimulationClock.SecondsPerCycle + 30f;
            Assert.AreEqual(expected, clock.TotalSimulationSeconds);
        }

        [Test]
        public void ResetClearsClockState()
        {
            var clock = new SimulationClock();
            clock.Advance(SimulationClock.SecondsPerCycle + 30f);
            
            clock.Reset();
            Assert.AreEqual(0, clock.CurrentCycle);
            Assert.AreEqual(0f, clock.AccumulatedSeconds);
        }

        [Test]
        public void GetActionEndTimeReturnsCorrectTime()
        {
            var clock = new SimulationClock();
            clock.Advance(10f);
            
            float endTime = clock.GetActionEndTime(5f);
            Assert.AreEqual(15f, endTime);
        }

        [Test]
        public void IsActionCompleteReturnsTrueWhenTimeExceeds()
        {
            var clock = new SimulationClock();
            clock.Advance(10f);
            float endTime = clock.GetActionEndTime(5f);
            
            // Not yet complete
            Assert.IsFalse(clock.IsActionComplete(endTime));
            
            // Advance to end time
            clock.Advance(5f);
            Assert.IsTrue(clock.IsActionComplete(endTime));
        }

        [Test]
        public void SequencedActionsWorkCorrectly()
        {
            var clock = new SimulationClock();
            
            // First action: 5 seconds
            float action1End = clock.GetActionEndTime(5f);
            Assert.AreEqual(5f, action1End);
            Assert.IsFalse(clock.IsActionComplete(action1End));
            
            // Advance 3 seconds (still in progress)
            clock.Advance(3f);
            Assert.IsFalse(clock.IsActionComplete(action1End));
            
            // Advance to completion
            clock.Advance(2f);
            Assert.IsTrue(clock.IsActionComplete(action1End));
            
            // Second action starts
            float action2End = clock.GetActionEndTime(10f);
            Assert.AreEqual(15f, action2End);
            Assert.IsFalse(clock.IsActionComplete(action2End));
        }
    }
}
