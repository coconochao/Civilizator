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

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("SimulationTickDriver");
            driver = gameObject.AddComponent<SimulationTickDriver>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void DriverInitializesWorldAndFacade()
        {
            Assert.IsNotNull(driver.World);
            Assert.IsNotNull(driver.Facade);
        }

        [Test]
        public void DriverAdvancesClockThroughSimulationStep()
        {
            // Simulate one Update with 30 seconds delta
            driver.World.SimulationStep(30f);

            Assert.AreEqual(0, driver.CurrentCycle);
            Assert.AreEqual(30f, driver.TotalSimulationSeconds);
        }

        [Test]
        public void DriverAdvancesAcrossMultipleCycles()
        {
            // Advance by more than one cycle
            driver.World.SimulationStep(SimulationClock.SecondsPerCycle * 2 + 30f);

            Assert.AreEqual(2, driver.CurrentCycle);
            Assert.AreEqual(SimulationClock.SecondsPerCycle * 2 + 30f, driver.TotalSimulationSeconds);
        }

        [Test]
        public void VariableTimeScaleDoesNotAffectSimulationCorrectness()
        {
            // Advance the same total time with different delta-time sequences
            var world1 = new World();
            world1.Initialize();
            world1.InitializeGameSetup();
            world1.SimulationStep(30f);
            world1.SimulationStep(30f);

            var world2 = new World();
            world2.Initialize();
            world2.InitializeGameSetup();
            world2.SimulationStep(10f);
            world2.SimulationStep(20f);
            world2.SimulationStep(30f);

            Assert.AreEqual(world1.Clock.CurrentCycle, world2.Clock.CurrentCycle);
            Assert.AreEqual(world1.Clock.AccumulatedSeconds, world2.Clock.AccumulatedSeconds);
            Assert.AreEqual(world1.Clock.TotalSimulationSeconds, world2.Clock.TotalSimulationSeconds);
        }

        [Test]
        public void LargeTimestepYieldsCorrectCycles()
        {
            // Simulate a large time step (e.g., slow machine recovering)
            driver.World.SimulationStep(200f);

            // 200 / 60 = 3 cycles with 20 seconds remainder
            Assert.AreEqual(3, driver.CurrentCycle);
            Assert.AreEqual(20f, driver.World.Clock.AccumulatedSeconds);
        }
    }
}
