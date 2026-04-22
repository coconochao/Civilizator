using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for population breakdown by life stage.
    /// </summary>
    public sealed class PopulationDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _populationText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasSnapshot;
        private PopulationSnapshot _snapshot;

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
        /// Assigns the text component used for rendering population values.
        /// </summary>
        public void SetPopulationText(Text populationText)
        {
            _populationText = populationText;
            Refresh();
        }

        /// <summary>
        /// Binds a population snapshot and refreshes immediately.
        /// </summary>
        public void Bind(PopulationSnapshot snapshot)
        {
            _snapshot = snapshot;
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
            if (_populationText == null)
            {
                return;
            }

            _populationText.text = _hasSnapshot
                ? PopulationDisplayFormatter.Format(_snapshot)
                : PopulationDisplayFormatter.Format(0, 0, 0);
        }

        /// <summary>
        /// Sets the life-stage counts directly.
        /// </summary>
        public void SetCounts(int children, int adults, int elders)
        {
            if (_populationText == null)
            {
                return;
            }

            _populationText.text = PopulationDisplayFormatter.Format(children, adults, elders);
        }

        /// <summary>
        /// Snapshot of population by life stage.
        /// </summary>
        public readonly struct PopulationSnapshot
        {
            public PopulationSnapshot(int children, int adults, int elders)
            {
                Children = children;
                Adults = adults;
                Elders = elders;
            }

            public int Children { get; }
            public int Adults { get; }
            public int Elders { get; }

            public int Total => Children + Adults + Elders;
        }

        /// <summary>
        /// Formatting helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class PopulationDisplayFormatter
        {
            public static string Format(PopulationSnapshot snapshot)
            {
                return Format(snapshot.Children, snapshot.Adults, snapshot.Elders);
            }

            public static string Format(int children, int adults, int elders)
            {
                int total = children + adults + elders;
                return
                    $"Children: {children}\n" +
                    $"Adults: {adults}\n" +
                    $"Elders: {elders}\n" +
                    $"Total: {total}";
            }

            public static string FormatPercentage(int part, int total)
            {
                if (total <= 0)
                {
                    return "0%";
                }

                float percentage = (float)part / total * 100f;
                return percentage.ToString("0.##", CultureInfo.InvariantCulture) + "%";
            }
        }
    }
}
