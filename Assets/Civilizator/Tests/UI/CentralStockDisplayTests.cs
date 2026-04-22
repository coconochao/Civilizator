using Civilizator.Simulation;
using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class CentralStockDisplayTests
    {
        private GameObject _gameObject;
        private CentralStockDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("CentralStockDisplay");
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display = _gameObject.AddComponent<CentralStockDisplay>();
            _display.SetStockText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysAllFourResources()
        {
            string formatted = CentralStockDisplay.CentralStockDisplayFormatter.Format(12, 34, 56, 78);

            Assert.AreEqual("Logs: 12\nOre: 34\nMeat: 56\nPlant food: 78", formatted);
        }

        [Test]
        public void BindRefreshesTextFromStorage()
        {
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Logs, 9);
            storage.Deposit(ResourceKind.Ore, 8);
            storage.Deposit(ResourceKind.Meat, 7);
            storage.Deposit(ResourceKind.PlantFood, 6);

            _display.Bind(storage);

            Assert.AreEqual("Logs: 9\nOre: 8\nMeat: 7\nPlant food: 6", _text.text);
        }

        [Test]
        public void SetStocksUpdatesTextWithoutStorageBinding()
        {
            _display.ClearBinding();
            _display.SetStocks(1, 2, 3, 4);

            Assert.AreEqual("Logs: 1\nOre: 2\nMeat: 3\nPlant food: 4", _text.text);
        }
    }
}
