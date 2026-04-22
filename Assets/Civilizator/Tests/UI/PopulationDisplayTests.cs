using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class PopulationDisplayTests
    {
        private GameObject _gameObject;
        private PopulationDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("PopulationDisplay");
            _display = _gameObject.AddComponent<PopulationDisplay>();
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display.SetPopulationText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysChildAdultAndElderCounts()
        {
            Assert.AreEqual(
                "Children: 3\nAdults: 7\nElders: 2\nTotal: 12",
                PopulationDisplay.PopulationDisplayFormatter.Format(3, 7, 2));
        }

        [Test]
        public void BindRefreshesTextFromSnapshot()
        {
            _display.Bind(new PopulationDisplay.PopulationSnapshot(4, 9, 1));

            Assert.AreEqual("Children: 4\nAdults: 9\nElders: 1\nTotal: 14", _text.text);
        }

        [Test]
        public void ClearBindingFallsBackToZeroValues()
        {
            _display.Bind(new PopulationDisplay.PopulationSnapshot(4, 9, 1));
            _display.ClearBinding();

            Assert.AreEqual("Children: 0\nAdults: 0\nElders: 0\nTotal: 0", _text.text);
        }

        [Test]
        public void SetCountsUpdatesTextDirectly()
        {
            _display.ClearBinding();
            _display.SetCounts(2, 5, 8);

            Assert.AreEqual("Children: 2\nAdults: 5\nElders: 8\nTotal: 15", _text.text);
        }

        [Test]
        public void FormatterPercentageHandlesZeroTotal()
        {
            Assert.AreEqual("0%", PopulationDisplay.PopulationDisplayFormatter.FormatPercentage(1, 0));
        }
    }
}
