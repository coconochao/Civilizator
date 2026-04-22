using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Tests.Simulation
{
    public class ProducerThresholdSettingsTests
    {
        [SetUp]
        public void SetUp()
        {
            ProducerThresholds.ResetToDefaults();
        }

        [Test]
        public void DefaultConstructor_UsesDefaultPairs()
        {
            var settings = new ProducerThresholdSettings();

            Assert.That(settings.Woodcutter.StartThreshold, Is.EqualTo(ProducerThresholdSettings.DefaultStartThreshold));
            Assert.That(settings.Woodcutter.StopThreshold, Is.EqualTo(ProducerThresholdSettings.DefaultStopThreshold));
            Assert.That(settings.Miner.StartThreshold, Is.EqualTo(ProducerThresholdSettings.DefaultStartThreshold));
            Assert.That(settings.Hunter.StopThreshold, Is.EqualTo(ProducerThresholdSettings.DefaultStopThreshold));
        }

        [Test]
        public void SetThresholds_UpdatesSingleProfession()
        {
            var settings = new ProducerThresholdSettings();

            settings.SetThresholds(Profession.Miner, 0.15f, 0.9f);

            Assert.That(settings.Miner.StartThreshold, Is.EqualTo(0.15f));
            Assert.That(settings.Miner.StopThreshold, Is.EqualTo(0.9f));
            Assert.That(settings.Woodcutter.StartThreshold, Is.EqualTo(ProducerThresholdSettings.DefaultStartThreshold));
        }

        [Test]
        public void ApplyToSimulation_BindsValuesToStaticSystem()
        {
            var settings = new ProducerThresholdSettings();
            settings.SetThresholds(Profession.Woodcutter, 0.1f, 0.9f);
            settings.SetThresholds(Profession.Miner, 0.15f, 0.85f);
            settings.SetThresholds(Profession.Hunter, 0.2f, 0.7f);
            settings.SetThresholds(Profession.Farmer, 0.25f, 0.75f);

            settings.ApplyToSimulation();

            Assert.That(ProducerThresholds.GetStartThreshold(Profession.Woodcutter), Is.EqualTo(0.1f));
            Assert.That(ProducerThresholds.GetStopThreshold(Profession.Miner), Is.EqualTo(0.85f));
            Assert.That(ProducerThresholds.GetStartThreshold(Profession.Hunter), Is.EqualTo(0.2f));
            Assert.That(ProducerThresholds.GetStopThreshold(Profession.Farmer), Is.EqualTo(0.75f));
        }

        [Test]
        public void FromSimulation_ReflectsCurrentStaticValues()
        {
            ProducerThresholds.SetThresholds(Profession.Woodcutter, 0.11f, 0.91f);
            ProducerThresholds.SetThresholds(Profession.Miner, 0.12f, 0.92f);
            ProducerThresholds.SetThresholds(Profession.Hunter, 0.13f, 0.93f);
            ProducerThresholds.SetThresholds(Profession.Farmer, 0.14f, 0.94f);

            var settings = ProducerThresholdSettings.FromSimulation();

            Assert.That(settings.Woodcutter.StartThreshold, Is.EqualTo(0.11f));
            Assert.That(settings.Miner.StopThreshold, Is.EqualTo(0.92f));
            Assert.That(settings.Hunter.StartThreshold, Is.EqualTo(0.13f));
            Assert.That(settings.Farmer.StopThreshold, Is.EqualTo(0.94f));
        }

        [Test]
        public void InvalidThresholds_Throw()
        {
            var settings = new ProducerThresholdSettings();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => settings.SetThresholds(Profession.Woodcutter, -0.1f, 0.5f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => settings.SetThresholds(Profession.Woodcutter, 0.5f, 1.1f));
            Assert.Throws<System.ArgumentException>(() => settings.SetThresholds(Profession.Woodcutter, 0.6f, 0.4f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ProducerThresholds.SetThresholds(Profession.Miner, float.NaN, 0.7f));
        }

        [Test]
        public void GetThresholds_RejectsNonProducerProfessions()
        {
            var settings = new ProducerThresholdSettings();

            Assert.Throws<System.ArgumentException>(() => settings.GetThresholds(Profession.Builder));
            Assert.Throws<System.ArgumentException>(() => settings.SetThresholds(Profession.Soldier, 0.1f, 0.2f));
        }
    }
}
