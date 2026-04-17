using System;
using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Tests.Simulation
{
    public class ProducerThresholdsTests
    {
        [SetUp]
        public void SetUp()
        {
            // Ensure defaults before each test
            ProducerThresholds.ResetToDefaults();
        }

        [Test]
        public void DefaultThresholdsExistAndAreSensible()
        {
            foreach (Profession p in new[] { Profession.Woodcutter, Profession.Miner, Profession.Hunter, Profession.Farmer })
            {
                float start = ProducerThresholds.GetStartThreshold(p);
                float stop = ProducerThresholds.GetStopThreshold(p);
                Assert.That(start, Is.GreaterThanOrEqualTo(0f));
                Assert.That(stop, Is.LessThanOrEqualTo(1f));
                Assert.That(start, Is.LessThan(stop), $"Start should be less than stop for {p}");
            }
        }

        [Test]
        public void CanSetCustomThresholds()
        {
            ProducerThresholds.SetThresholds(Profession.Woodcutter, 0.1f, 0.9f);
            Assert.AreEqual(0.1f, ProducerThresholds.GetStartThreshold(Profession.Woodcutter));
            Assert.AreEqual(0.9f, ProducerThresholds.GetStopThreshold(Profession.Woodcutter));
        }

        [Test]
        public void SetThresholds_ThrowsWhenOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ProducerThresholds.SetThresholds(Profession.Miner, -0.1f, 0.5f));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ProducerThresholds.SetThresholds(Profession.Miner, 0.5f, 1.2f));
        }

        [Test]
        public void SetThresholds_ThrowsWhenStartNotLessThanStop()
        {
            Assert.Throws<ArgumentException>(() =>
                ProducerThresholds.SetThresholds(Profession.Hunter, 0.6f, 0.4f));
        }

        [Test]
        public void ResetToDefaultsRestoresOriginalValues()
        {
            ProducerThresholds.SetThresholds(Profession.Farmer, 0.3f, 0.7f);
            ProducerThresholds.ResetToDefaults();

            // Defaults are 0.2 / 0.8 per implementation
            Assert.AreEqual(0.2f, ProducerThresholds.GetStartThreshold(Profession.Farmer));
            Assert.AreEqual(0.8f, ProducerThresholds.GetStopThreshold(Profession.Farmer));
        }
    }
}