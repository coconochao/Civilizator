using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Simulation.Tests
{
    [TestFixture]
    public class SoldierControlsTests
    {
        [SetUp]
        public void SetUp()
        {
            SoldierImprovementControls.ResetToDefaults();
        }

        [Test]
        public void DefaultConstructor_UsesExpectedDefaults()
        {
            var controls = new SoldierControls();

            Assert.AreEqual(SoldierModeSwitchSystem.DefaultPatrolTargetShare, controls.PatrolTargetShare);
            Assert.AreEqual(SoldierModeSwitchSystem.DefaultSwitchThreshold, controls.SwitchThreshold);
            Assert.AreEqual(SoldierModeSwitchSystem.DefaultSwitchCooldownCycles, controls.SwitchCooldownCycles);
            Assert.AreEqual(SoldierImprovementControls.DefaultTowerBuildEmphasis, controls.TowerBuildEmphasis);
        }

        [Test]
        public void Constructor_WithValues_StoresValues()
        {
            var controls = new SoldierControls(0.3f, 0.2f, 7, 0.8f);

            Assert.AreEqual(0.3f, controls.PatrolTargetShare);
            Assert.AreEqual(0.2f, controls.SwitchThreshold);
            Assert.AreEqual(7, controls.SwitchCooldownCycles);
            Assert.AreEqual(0.8f, controls.TowerBuildEmphasis);
        }

        [Test]
        public void ApplyToSimulation_BindsValuesToSystems()
        {
            var controls = new SoldierControls(0.25f, 0.1f, 3, 0.9f);
            var modeSwitchSystem = new SoldierModeSwitchSystem();

            controls.ApplyToSimulation(modeSwitchSystem);

            Assert.AreEqual(0.25f, modeSwitchSystem.PatrolTargetShare);
            Assert.AreEqual(0.1f, modeSwitchSystem.SwitchThreshold);
            Assert.AreEqual(3, modeSwitchSystem.SwitchCooldownCycles);
            Assert.AreEqual(0.9f, SoldierImprovementControls.GetTowerBuildEmphasis());
        }

        [Test]
        public void FromSimulation_ReflectsSystemState()
        {
            var modeSwitchSystem = new SoldierModeSwitchSystem();
            modeSwitchSystem.SetPatrolTargetShare(0.35f);
            modeSwitchSystem.SetSwitchThreshold(0.18f);
            modeSwitchSystem.SetSwitchCooldownCycles(4);
            SoldierImprovementControls.SetTowerBuildEmphasis(0.7f);

            var controls = SoldierControls.FromSimulation(modeSwitchSystem);

            Assert.AreEqual(0.35f, controls.PatrolTargetShare);
            Assert.AreEqual(0.18f, controls.SwitchThreshold);
            Assert.AreEqual(4, controls.SwitchCooldownCycles);
            Assert.AreEqual(0.7f, controls.TowerBuildEmphasis);
        }

        [Test]
        public void InvalidValues_Throw()
        {
            var controls = new SoldierControls();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => controls.SetPatrolTargetShare(-0.1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => controls.SetSwitchThreshold(1.1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => controls.SetSwitchCooldownCycles(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => controls.SetTowerBuildEmphasis(float.NaN));
        }

        [Test]
        public void ApplyToSimulation_NullSystem_Throws()
        {
            var controls = new SoldierControls();

            Assert.Throws<System.ArgumentNullException>(() => controls.ApplyToSimulation(null));
        }
    }
}
