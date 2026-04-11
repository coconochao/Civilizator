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
    }
}
