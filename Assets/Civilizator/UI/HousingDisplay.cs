using System.Collections.Generic;
using System.Globalization;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for housing coverage.
    /// Shows how many living agents are assigned to a house versus unassigned.
    /// </summary>
    public sealed class HousingDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _housingText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasSnapshot;
        private HousingSnapshot _snapshot;

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
        /// Assigns the text component used for rendering housing values.
        /// </summary>
        public void SetHousingText(Text housingText)
        {
            _housingText = housingText;
            Refresh();
        }

        /// <summary>
        /// Binds a housing snapshot and refreshes immediately.
        /// </summary>
        public void Bind(HousingSnapshot snapshot)
        {
            _snapshot = snapshot;
            _hasSnapshot = true;
            Refresh();
        }

        /// <summary>
        /// Binds directly from an agent list by counting assigned and unassigned living agents.
        /// </summary>
        public void BindAgents(IEnumerable<Agent> agents)
        {
            _snapshot = HousingSnapshot.FromAgents(agents);
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
            if (_housingText == null)
            {
                return;
            }

            _housingText.text = _hasSnapshot
                ? HousingDisplayFormatter.Format(_snapshot)
                : HousingDisplayFormatter.Format(0, 0);
        }

        /// <summary>
        /// Sets the assigned and unassigned counts directly.
        /// </summary>
        public void SetCounts(int assigned, int unassigned)
        {
            if (_housingText == null)
            {
                return;
            }

            _housingText.text = HousingDisplayFormatter.Format(assigned, unassigned);
        }

        /// <summary>
        /// Snapshot of housing coverage.
        /// </summary>
        public readonly struct HousingSnapshot
        {
            public HousingSnapshot(int assigned, int unassigned)
            {
                Assigned = assigned;
                Unassigned = unassigned;
            }

            public int Assigned { get; }
            public int Unassigned { get; }

            public int Total => Assigned + Unassigned;

            public static HousingSnapshot FromAgents(IEnumerable<Agent> agents)
            {
                int assigned = 0;
                int unassigned = 0;

                if (agents != null)
                {
                    foreach (var agent in agents)
                    {
                        if (agent == null || !agent.IsAlive)
                        {
                            continue;
                        }

                        if (agent.IsHouseAssigned)
                        {
                            assigned++;
                        }
                        else
                        {
                            unassigned++;
                        }
                    }
                }

                return new HousingSnapshot(assigned, unassigned);
            }
        }

        /// <summary>
        /// Formatting helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class HousingDisplayFormatter
        {
            public static string Format(HousingSnapshot snapshot)
            {
                return Format(snapshot.Assigned, snapshot.Unassigned);
            }

            public static string Format(int assigned, int unassigned)
            {
                int total = assigned + unassigned;
                return
                    $"Assigned: {assigned}\n" +
                    $"Unassigned: {unassigned}\n" +
                    $"Total: {total}";
            }
        }
    }
}
