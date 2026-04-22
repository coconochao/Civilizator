using System.Collections.Generic;
using Civilizator.Simulation;
using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class ProductivityDisplayTests
    {
        private GameObject _gameObject;
        private ProductivityDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("ProductivityDisplay");
            _display = _gameObject.AddComponent<ProductivityDisplay>();
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display.SetProductivityText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysAverageProductivityAndStarvationCounts()
        {
            string formatted = ProductivityDisplay.ProductivityDisplayFormatter.Format(0.875f, 0.5f, 1f, 0.75f, 3, 1);

            Assert.AreEqual(
                "Average productivity: 87.5%\nChildren: 50%\nAdults: 100%\nElders: 75%\nStarving: 3\nDead from starvation: 1",
                formatted);
        }

        [Test]
        public void BindAgentsComputesAverageProductivityByStage()
        {
            var child = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Child);
            child.EatingState.ApplyStarvationPenalty();

            var adult = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult)
            {
                AssignedHouseId = 1
            };

            var elder = new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Elder);
            elder.HitPoints = 0;
            elder.EatingState.ApplyStarvationPenalty();
            elder.EatingState.ApplyStarvationPenalty();
            elder.EatingState.ApplyStarvationPenalty();
            elder.EatingState.ApplyStarvationPenalty();

            var agents = new List<Agent> { child, adult, elder };

            _display.BindAgents(agents);

            Assert.AreEqual(
                "Average productivity: 72.5%\nChildren: 25%\nAdults: 120%\nElders: 0%\nStarving: 1\nDead from starvation: 1",
                _text.text);
        }

        [Test]
        public void ClearBindingFallsBackToZeroValues()
        {
            _display.Bind(new ProductivityDisplay.ProductivitySnapshot(0.5f, 0.25f, 0.75f, 0.5f, 2, 1));
            _display.ClearBinding();

            Assert.AreEqual(
                "Average productivity: 0%\nChildren: 0%\nAdults: 0%\nElders: 0%\nStarving: 0\nDead from starvation: 0",
                _text.text);
        }
    }
}
