using Civilizator.Simulation;
using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class ProfessionAllocationDisplayTests
    {
        private GameObject _gameObject;
        private ProfessionAllocationDisplay _display;
        private Slider[] _sliders;
        private Text[] _texts;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("ProfessionAllocationDisplay");
            _display = _gameObject.AddComponent<ProfessionAllocationDisplay>();

            _sliders = new Slider[ProfessionTargets.ProfessionCount];
            _texts = new Text[ProfessionTargets.ProfessionCount];

            for (int i = 0; i < ProfessionTargets.ProfessionCount; i++)
            {
                var sliderObject = new GameObject($"Slider_{i}");
                sliderObject.AddComponent<RectTransform>();
                _sliders[i] = sliderObject.AddComponent<Slider>();
                _sliders[i].transform.SetParent(_gameObject.transform, false);

                var textObject = new GameObject($"Text_{i}");
                textObject.AddComponent<RectTransform>();
                _texts[i] = textObject.AddComponent<Text>();
                _texts[i].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _texts[i].transform.SetParent(_gameObject.transform, false);
            }

            _display.SetTargetSliders(_sliders);
            _display.SetActualTexts(_texts);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysPercentage()
        {
            Assert.AreEqual("12.5%", ProfessionAllocationDisplay.ProfessionAllocationDisplayFormatter.FormatPercentage(0.125f));
        }

        [Test]
        public void BindTargetsUpdatesAllSliders()
        {
            var targets = new ProfessionTargets(0.10f, 0.15f, 0.20f, 0.25f, 0.15f, 0.15f);

            _display.BindTargets(targets);

            for (int i = 0; i < ProfessionTargets.ProfessionCount; i++)
            {
                Assert.AreEqual(targets.GetTarget((Profession)i), _sliders[i].value);
            }
        }

        [Test]
        public void BindActualCountsUpdatesReadouts()
        {
            var targets = new ProfessionTargets(0.10f, 0.15f, 0.20f, 0.25f, 0.15f, 0.15f);
            var counts = new[] { 1, 2, 3, 4, 5, 5 };

            _display.Bind(targets, counts, 20);

            Assert.AreEqual("5%", _texts[0].text);
            Assert.AreEqual("10%", _texts[1].text);
            Assert.AreEqual("15%", _texts[2].text);
            Assert.AreEqual("20%", _texts[3].text);
            Assert.AreEqual("25%", _texts[4].text);
            Assert.AreEqual("25%", _texts[5].text);
        }

        [Test]
        public void BindActualCountsWithZeroPopulationShowsZeroPercent()
        {
            _display.BindActualCounts(new[] { 1, 2, 3, 4, 5, 6 }, 0);

            for (int i = 0; i < ProfessionTargets.ProfessionCount; i++)
            {
                Assert.AreEqual("0%", _texts[i].text);
            }
        }
    }
}
