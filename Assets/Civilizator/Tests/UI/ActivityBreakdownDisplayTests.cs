using System.Collections.Generic;
using Civilizator.Simulation;
using Civilizator.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI.Tests
{
    [TestFixture]
    public class ActivityBreakdownDisplayTests
    {
        private GameObject _gameObject;
        private ActivityBreakdownDisplay _display;
        private Text _text;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("ActivityBreakdownDisplay");
            _display = _gameObject.AddComponent<ActivityBreakdownDisplay>();
            _text = _gameObject.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _display.SetActivityText(_text);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void FormatterDisplaysAllActivityRows()
        {
            var snapshot = new ActivityBreakdownDisplay.ActivitySnapshot
            {
                SoldierPatrolling = 3,
                SoldierImproving = 2,
                StaffedTowers = 1,
                TotalTowers = 2
            };
            snapshot.Producing[(int)Profession.Woodcutter] = 4;
            snapshot.Improving[(int)Profession.Miner] = 5;
            snapshot.Improving[(int)Profession.Builder] = 1;

            string expected =
                "Woodcutter: producing 4, improving 0\n" +
                "Miner: producing 0, improving 5\n" +
                "Hunter: producing 0, improving 0\n" +
                "Farmer: producing 0, improving 0\n" +
                "Builder: improving 1\n" +
                "Soldier: patrolling 3, improving 2\n" +
                "Towers staffed: 1/2";

            Assert.AreEqual(expected, ActivityBreakdownDisplay.ActivityBreakdownDisplayFormatter.Format(snapshot));
        }

        [Test]
        public void BindWorldDerivesActivityCounts()
        {
            var storage = new CentralStorage();
            storage.Deposit(ResourceKind.Logs, 100);
            storage.Deposit(ResourceKind.Ore, 900);
            storage.Deposit(ResourceKind.Meat, 100);
            storage.Deposit(ResourceKind.PlantFood, 100);

            var agents = new List<Agent>();

            var woodcutterProducing = new Agent(new GridPos(0, 0), Profession.Woodcutter, LifeStage.Adult);
            var woodTree = new NaturalNode(NaturalNodeType.Tree, new GridPos(0, 0), 100);
            agents.Add(woodcutterProducing);

            var minerImproving = new Agent(new GridPos(1, 0), Profession.Miner, LifeStage.Adult);
            var oreNode = new NaturalNode(NaturalNodeType.Ore, new GridPos(1, 0), 100);
            agents.Add(minerImproving);

            var hunterImproving = new Agent(new GridPos(2, 0), Profession.Hunter, LifeStage.Adult);
            agents.Add(hunterImproving);

            var farmerProducing = new Agent(new GridPos(3, 0), Profession.Farmer, LifeStage.Adult);
            var plantNode = new NaturalNode(NaturalNodeType.Plant, new GridPos(3, 0), 100);
            agents.Add(farmerProducing);

            var builder = new Agent(new GridPos(10, 10), Profession.Builder, LifeStage.Adult);
            agents.Add(builder);

            var soldierPatrolling = new Agent(new GridPos(20, 20), Profession.Soldier, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Patrolling
            };
            agents.Add(soldierPatrolling);

            var soldierImproving = new Agent(new GridPos(21, 20), Profession.Soldier, LifeStage.Adult)
            {
                SoldierMode = SoldierMode.Improving
            };
            agents.Add(soldierImproving);

            var towerStaffed = new Building(BuildingKind.Tower, new GridPos(20, 20));
            var towerVacant = new Building(BuildingKind.Tower, new GridPos(30, 30));
            var houseTarget = new Building(BuildingKind.House, new GridPos(40, 40))
            {
                IsUnderConstruction = true
            };

            var buildings = new List<Building> { towerStaffed, towerVacant, houseTarget };
            var nodes = new List<NaturalNode> { woodTree, oreNode, plantNode };
            var targets = new ProfessionTargets();

            _display.BindWorld(agents, nodes, buildings, storage, targets, 1000);

            string expected =
                "Woodcutter: producing 1, improving 0\n" +
                "Miner: producing 0, improving 1\n" +
                "Hunter: producing 0, improving 1\n" +
                "Farmer: producing 1, improving 0\n" +
                "Builder: improving 1\n" +
                "Soldier: patrolling 1, improving 1\n" +
                "Towers staffed: 1/2";

            Assert.AreEqual(expected, _text.text);
        }

        [Test]
        public void ClearBindingFallsBackToZeroValues()
        {
            _display.Bind(new ActivityBreakdownDisplay.ActivitySnapshot
            {
                SoldierPatrolling = 1,
                SoldierImproving = 1,
                StaffedTowers = 1,
                TotalTowers = 1
            });
            _display.ClearBinding();

            string expected =
                "Woodcutter: producing 0, improving 0\n" +
                "Miner: producing 0, improving 0\n" +
                "Hunter: producing 0, improving 0\n" +
                "Farmer: producing 0, improving 0\n" +
                "Builder: improving 0\n" +
                "Soldier: patrolling 0, improving 0\n" +
                "Towers staffed: 0/0";

            Assert.AreEqual(expected, _text.text);
        }
    }
}
