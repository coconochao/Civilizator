using NUnit.Framework;
using Civilizator.Simulation;

namespace Civilizator.Tests.Simulation
{
    public class ProducerThresholdsIntegrationTests
    {
        [Test]
        public void ShouldBeProducing_BelowStartThreshold_ReturnsTrue()
        {
            // Woodcutter: start=0.2, stop=0.8
            // Current stock = 10, max stock = 100 → normalized = 0.1 (below start)
            Assert.IsTrue(ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 10, 100));
        }

        [Test]
        public void ShouldBeProducing_AboveStopThreshold_ReturnsFalse()
        {
            // Woodcutter: start=0.2, stop=0.8
            // Current stock = 90, max stock = 100 → normalized = 0.9 (above stop)
            Assert.IsFalse(ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 90, 100));
        }

        [Test]
        public void ShouldBeProducing_BetweenThresholds_ReturnsFalse()
        {
            // Woodcutter: start=0.2, stop=0.8
            // Current stock = 50, max stock = 100 → normalized = 0.5 (between thresholds)
            Assert.IsFalse(ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 50, 100));
        }

        [Test]
        public void ShouldBeProducing_ExactlyAtStartThreshold_ReturnsFalse()
        {
            // Woodcutter: start=0.2, stop=0.8
            // Current stock = 20, max stock = 100 → normalized = 0.2 (exactly at start)
            Assert.IsFalse(ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 20, 100));
        }

        [Test]
        public void ShouldBeProducing_ExactlyAtStopThreshold_ReturnsFalse()
        {
            // Woodcutter: start=0.2, stop=0.8
            // Current stock = 80, max stock = 100 → normalized = 0.8 (exactly at stop)
            Assert.IsFalse(ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 80, 100));
        }

        [Test]
        public void ShouldBeProducing_MaxStockZero_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                ProducerThresholds.ShouldBeProducing(Profession.Woodcutter, 10, 0));
        }

        [Test]
        public void ShouldBeProducing_DifferentProfessions_DifferentThresholds()
        {
            // Miner: start=0.2, stop=0.8
            // Current stock = 10, max stock = 100 → normalized = 0.1 (below start)
            Assert.IsTrue(ProducerThresholds.ShouldBeProducing(Profession.Miner, 10, 100));

            // Farmer: start=0.2, stop=0.8
            // Current stock = 90, max stock = 100 → normalized = 0.9 (above stop)
            Assert.IsFalse(ProducerThresholds.ShouldBeProducing(Profession.Farmer, 90, 100));
        }
    }
}