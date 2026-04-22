using System;
using System.Collections.Generic;
using System.Linq;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for per-profession activity breakdown.
    /// Shows producing versus improving counts for all professions, plus soldier mode split and staffed towers.
    /// </summary>
    public sealed class ActivityBreakdownDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _activityText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasSnapshot;
        private ActivitySnapshot _snapshot;

        private void OnEnable()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            if (_refreshEveryFrame)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Assigns the text component used for rendering activity values.
        /// </summary>
        public void SetActivityText(Text activityText)
        {
            _activityText = activityText;
            Refresh();
        }

        /// <summary>
        /// Binds a precomputed activity snapshot and refreshes immediately.
        /// </summary>
        public void Bind(ActivitySnapshot snapshot)
        {
            _snapshot = snapshot;
            _hasSnapshot = true;
            Refresh();
        }

        /// <summary>
        /// Builds and binds an activity snapshot from the current world state.
        /// </summary>
        public void BindWorld(
            IReadOnlyList<Agent> agents,
            IReadOnlyList<NaturalNode> nodes,
            IReadOnlyList<Building> buildings,
            CentralStorage storage,
            ProfessionTargets targets,
            int maxStockPerResource = 1000)
        {
            _snapshot = ActivitySnapshot.FromWorld(agents, nodes, buildings, storage, targets, maxStockPerResource);
            _hasSnapshot = true;
            Refresh();
        }

        /// <summary>
        /// Clears any bound snapshot and renders zero values.
        /// </summary>
        public void ClearBinding()
        {
            _hasSnapshot = false;
            Refresh();
        }

        /// <summary>
        /// Updates the displayed text from the bound snapshot, or zeroes if nothing is bound.
        /// </summary>
        public void Refresh()
        {
            if (_activityText == null)
            {
                return;
            }

            _activityText.text = _hasSnapshot
                ? ActivityBreakdownDisplayFormatter.Format(_snapshot)
                : ActivityBreakdownDisplayFormatter.Format(ActivitySnapshot.Empty);
        }

        /// <summary>
        /// Helper snapshot for per-profession activity data.
        /// </summary>
        public sealed class ActivitySnapshot
        {
            private const int ProfessionCount = 6;

            public ActivitySnapshot()
            {
                Producing = new int[ProfessionCount];
                Improving = new int[ProfessionCount];
            }

            public int[] Producing { get; }
            public int[] Improving { get; }
            public int SoldierPatrolling { get; set; }
            public int SoldierImproving { get; set; }
            public int StaffedTowers { get; set; }
            public int TotalTowers { get; set; }

            public static ActivitySnapshot Empty => new ActivitySnapshot();

            public static ActivitySnapshot FromWorld(
                IReadOnlyList<Agent> agents,
                IReadOnlyList<NaturalNode> nodes,
                IReadOnlyList<Building> buildings,
                CentralStorage storage,
                ProfessionTargets targets,
                int maxStockPerResource)
            {
                if (targets == null)
                    throw new ArgumentNullException(nameof(targets));

                if (maxStockPerResource <= 0)
                    throw new ArgumentOutOfRangeException(nameof(maxStockPerResource), "Max stock must be greater than zero.");

                var snapshot = new ActivitySnapshot();
                var agentList = agents?.Where(agent => agent != null && agent.IsAlive).ToList() ?? new List<Agent>();
                var buildingList = buildings?.Where(building => building != null).ToList() ?? new List<Building>();
                var nodeList = nodes?.Where(node => node != null).ToList() ?? new List<NaturalNode>();

                foreach (var agent in agentList)
                {
                    switch (agent.Profession)
                    {
                        case Profession.Woodcutter:
                        case Profession.Miner:
                        case Profession.Hunter:
                        case Profession.Farmer:
                            CountProducer(agent, nodeList, storage, maxStockPerResource, snapshot);
                            break;
                        case Profession.Builder:
                            if (BuilderSystem.FindBestImprovementTarget(agent, agentList, buildingList, storage, targets) != null)
                            {
                                snapshot.Improving[(int)Profession.Builder]++;
                            }
                            break;
                        case Profession.Soldier:
                            if (agent.SoldierMode == SoldierMode.Patrolling)
                                snapshot.SoldierPatrolling++;
                            else
                                snapshot.SoldierImproving++;
                            break;
                    }
                }

                var towers = buildingList.Where(building => building.Kind == BuildingKind.Tower).ToList();
                snapshot.TotalTowers = towers.Count;
                snapshot.StaffedTowers = towers.Count(tower => TowerCombatSystem.HasSoldierInside(tower, agentList));

                return snapshot;
            }

            private static void CountProducer(
                Agent agent,
                IReadOnlyList<NaturalNode> nodes,
                CentralStorage storage,
                int maxStockPerResource,
                ActivitySnapshot snapshot)
            {
                ResourceKind resourceKind = ProductionSystem.GetRequiredResourceForProfession(agent.Profession);
                int currentStock = storage != null ? storage.GetStock(resourceKind) : 0;
                NaturalNode nearestNode = ProductionSystem.FindNearestRelevantNode(agent, nodes ?? Array.Empty<NaturalNode>());

                if (ProductionSystem.ShouldSwitchToImprovement(agent, nearestNode, currentStock, maxStockPerResource))
                {
                    snapshot.Improving[(int)agent.Profession]++;
                }
                else
                {
                    snapshot.Producing[(int)agent.Profession]++;
                }
            }
        }

        /// <summary>
        /// Formatting helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class ActivityBreakdownDisplayFormatter
        {
            public static string Format(ActivitySnapshot snapshot)
            {
                if (snapshot == null)
                    return Format(ActivitySnapshot.Empty);

                string[] professionNames =
                {
                    "Woodcutter",
                    "Miner",
                    "Hunter",
                    "Farmer",
                    "Builder",
                    "Soldier"
                };

                var lines = new List<string>();
                for (int i = 0; i < professionNames.Length; i++)
                {
                    if (i == (int)Profession.Builder)
                    {
                        lines.Add($"{professionNames[i]}: improving {snapshot.Improving[i]}");
                        continue;
                    }

                    if (i == (int)Profession.Soldier)
                    {
                        lines.Add($"{professionNames[i]}: patrolling {snapshot.SoldierPatrolling}, improving {snapshot.SoldierImproving}");
                        continue;
                    }

                    lines.Add($"{professionNames[i]}: producing {snapshot.Producing[i]}, improving {snapshot.Improving[i]}");
                }

                lines.Add($"Towers staffed: {snapshot.StaffedTowers}/{snapshot.TotalTowers}");
                return string.Join("\n", lines);
            }
        }
    }
}
