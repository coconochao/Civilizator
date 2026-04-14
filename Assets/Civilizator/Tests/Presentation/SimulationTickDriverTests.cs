using Civilizator.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace Civilizator.Presentation.Tests
{
    [TestFixture]
    public class SimulationTickDriverTests
    {
        private GameObject gameObject;
        private SimulationTickDriver driver;
        private SimulationClock clock;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("SimulationTickDriver");
            driver = gameObject.AddComponent<SimulationTickDriver>();
            clock = new SimulationClock();
            driver.Clock = clock;
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(gameObject);
        }

        [Test]
        public void DriverAdvanceClockWithDeltaTime()
        {
            // Simulate one Update with 30 seconds delta
            driver.Clock.Advance(30f);

            Assert.AreEqual(0, driver.CurrentCycle);
            Assert.AreEqual(30f, driver.TotalSimulationSeconds);
        }

        [Test]
        public void DriverAdvancesAcrossMultipleCycles()
        {
            // Advance by more than one cycle
            driver.Clock.Advance(SimulationClock.SecondsPerCycle * 2 + 30f);

            Assert.AreEqual(2, driver.CurrentCycle);
            Assert.AreEqual(SimulationClock.SecondsPerCycle * 2 + 30f, driver.TotalSimulationSeconds);
        }

        [Test]
        public void VariableTimeScaleDoesNotAffectSimulationCorrectness()
        {
            // Advance the same total time with different delta-time sequences
            var clock1 = new SimulationClock();
            clock1.Advance(30f);
            clock1.Advance(30f);

            var clock2 = new SimulationClock();
            clock2.Advance(10f);
            clock2.Advance(20f);
            clock2.Advance(30f);

            Assert.AreEqual(clock1.CurrentCycle, clock2.CurrentCycle);
            Assert.AreEqual(clock1.AccumulatedSeconds, clock2.AccumulatedSeconds);
            Assert.AreEqual(clock1.TotalSimulationSeconds, clock2.TotalSimulationSeconds);
        }

        [Test]
        public void LargeTimestepYieldsCorrectCycles()
        {
            // Simulate a large time step (e.g., slow machine recovering)
            driver.Clock.Advance(200f);

            // 200 / 60 = 3 cycles with 20 seconds remainder
            Assert.AreEqual(3, driver.CurrentCycle);
            Assert.AreEqual(20f, driver.Clock.AccumulatedSeconds);
        }
    }
}
