using System.Collections.Generic;
using Civilizator.Simulation;
using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class HousingDisplayTests
    {
        private GameObject _gameObject;
        private HousingDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("HousingDisplay");
            _display = _gameObject.AddComponent<HousingDisplay>();
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display.SetHousingText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysAssignedAndUnassignedCounts()
        {
            Assert.AreEqual("Assigned: 6\nUnassigned: 4\nTotal: 10", HousingDisplay.HousingDisplayFormatter.Format(6, 4));
        }

        [Test]
        public void BindRefreshesTextFromSnapshot()
        {
            _display.Bind(new HousingDisplay.HousingSnapshot(7, 3));

            Assert.AreEqual("Assigned: 7\nUnassigned: 3\nTotal: 10", _text.text);
        }

        [Test]
        public void BindAgentsCountsOnlyLivingAgents()
        {
            var assignedAdult = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            assignedAdult.AssignedHouseId = 1;
            var unassignedAdult = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);
            var deadAssigned = new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Adult) { HitPoints = 0 };
            deadAssigned.AssignedHouseId = 2;
            var child = new Agent(new GridPos(3, 0), Profession.Farmer, LifeStage.Child);

            var agents = new List<Agent> { assignedAdult, unassignedAdult, deadAssigned, child };

            _display.BindAgents(agents);

            Assert.AreEqual("Assigned: 1\nUnassigned: 2\nTotal: 3", _text.text);
        }

        [Test]
        public void ClearBindingFallsBackToZeroValues()
        {
            _display.Bind(new HousingDisplay.HousingSnapshot(7, 3));
            _display.ClearBinding();

            Assert.AreEqual("Assigned: 0\nUnassigned: 0\nTotal: 0", _text.text);
        }

        [Test]
        public void SetCountsUpdatesTextDirectly()
        {
            _display.ClearBinding();
            _display.SetCounts(2, 8);

            Assert.AreEqual("Assigned: 2\nUnassigned: 8\nTotal: 10", _text.text);
        }
    }
}
