using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class ProductionRateDisplayTests
    {
        private GameObject _gameObject;
        private ProductionRateDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("ProductionRateDisplay");
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display = _gameObject.AddComponent<ProductionRateDisplay>();
            _display.SetRateText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysOverallAndPerProfessionRates()
        {
            string formatted = ProductionRateDisplay.ProductionRateDisplayFormatter.Format(14.5f, 3f, 4.25f, 2f, 5.25f);

            Assert.AreEqual(
                "Overall: 14.5 / cycle\nWoodcutter: 3 / cycle\nMiner: 4.25 / cycle\nHunter: 2 / cycle\nFarmer: 5.25 / cycle",
                formatted);
        }

        [Test]
        public void BindRefreshesTextFromSnapshot()
        {
            _display.Bind(new ProductionRateDisplay.ProductionRateSnapshot(8.5f, 1.5f, 2.25f, 3f, 1.75f));

            Assert.AreEqual(
                "Overall: 8.5 / cycle\nWoodcutter: 1.5 / cycle\nMiner: 2.25 / cycle\nHunter: 3 / cycle\nFarmer: 1.75 / cycle",
                _text.text);
        }

        [Test]
        public void ClearBindingFallsBackToZeroRates()
        {
            _display.Bind(new ProductionRateDisplay.ProductionRateSnapshot(8.5f, 1.5f, 2.25f, 3f, 1.75f));
            _display.ClearBinding();

            Assert.AreEqual(
                "Overall: 0 / cycle\nWoodcutter: 0 / cycle\nMiner: 0 / cycle\nHunter: 0 / cycle\nFarmer: 0 / cycle",
                _text.text);
        }

        [Test]
        public void SetRatesUpdatesTextDirectly()
        {
            _display.ClearBinding();
            _display.SetRates(9f, 2f, 2f, 2.5f, 2.5f);

            Assert.AreEqual(
                "Overall: 9 / cycle\nWoodcutter: 2 / cycle\nMiner: 2 / cycle\nHunter: 2.5 / cycle\nFarmer: 2.5 / cycle",
                _text.text);
        }
    }
}
