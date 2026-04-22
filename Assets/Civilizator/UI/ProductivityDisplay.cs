using System;
using System.Collections.Generic;
using System.Globalization;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for productivity and starvation diagnostics.
    /// Shows average productivity overall and by life stage, plus starvation counts.
    /// </summary>
    public sealed class ProductivityDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _productivityText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasSnapshot;
        private ProductivitySnapshot _snapshot;

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
        /// Assigns the text component used for rendering productivity values.
        /// </summary>
        public void SetProductivityText(Text productivityText)
        {
            _productivityText = productivityText;
            Refresh();
        }

        /// <summary>
        /// Binds a precomputed productivity snapshot and refreshes immediately.
        /// </summary>
        public void Bind(ProductivitySnapshot snapshot)
        {
            _snapshot = snapshot;
            _hasSnapshot = true;
            Refresh();
        }

        /// <summary>
        /// Builds a productivity snapshot from the current agent list.
        /// </summary>
        public void BindAgents(IEnumerable<Agent> agents)
        {
            _snapshot = ProductivitySnapshot.FromAgents(agents);
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
            if (_productivityText == null)
            {
                return;
            }

            _productivityText.text = _hasSnapshot
                ? ProductivityDisplayFormatter.Format(_snapshot)
                : ProductivityDisplayFormatter.Format(ProductivitySnapshot.Empty);
        }

        /// <summary>
        /// Snapshot of productivity and starvation diagnostics.
        /// </summary>
        public sealed class ProductivitySnapshot
        {
            public ProductivitySnapshot(
                float averageProductivity,
                float childAverageProductivity,
                float adultAverageProductivity,
                float elderAverageProductivity,
                int starvingAgents,
                int deadFromStarvation)
            {
                AverageProductivity = averageProductivity;
                ChildAverageProductivity = childAverageProductivity;
                AdultAverageProductivity = adultAverageProductivity;
                ElderAverageProductivity = elderAverageProductivity;
                StarvingAgents = starvingAgents;
                DeadFromStarvation = deadFromStarvation;
            }

            public float AverageProductivity { get; }
            public float ChildAverageProductivity { get; }
            public float AdultAverageProductivity { get; }
            public float ElderAverageProductivity { get; }
            public int StarvingAgents { get; }
            public int DeadFromStarvation { get; }

            public static ProductivitySnapshot Empty => new ProductivitySnapshot(0f, 0f, 0f, 0f, 0, 0);

            public static ProductivitySnapshot FromAgents(IEnumerable<Agent> agents)
            {
                if (agents == null)
                {
                    return Empty;
                }

                float totalProductivity = 0f;
                int livingAgents = 0;
                int starvingAgents = 0;
                int deadFromStarvation = 0;

                float childTotal = 0f;
                int childCount = 0;
                float adultTotal = 0f;
                int adultCount = 0;
                float elderTotal = 0f;
                int elderCount = 0;

                foreach (var agent in agents)
                {
                    if (agent == null)
                    {
                        continue;
                    }

                    if (!agent.IsAlive)
                    {
                        if (agent.EatingState.IsDeadFromStarvation)
                        {
                            deadFromStarvation++;
                        }

                        continue;
                    }

                    float productivity = agent.GetProductivityMultiplier();
                    totalProductivity += productivity;
                    livingAgents++;

                    if (agent.EatingState.StarvationPenalty > 0f)
                    {
                        starvingAgents++;
                    }

                    switch (agent.LifeStage)
                    {
                        case LifeStage.Child:
                            childTotal += productivity;
                            childCount++;
                            break;
                        case LifeStage.Adult:
                            adultTotal += productivity;
                            adultCount++;
                            break;
                        case LifeStage.Elder:
                            elderTotal += productivity;
                            elderCount++;
                            break;
                    }
                }

                return new ProductivitySnapshot(
                    Average(totalProductivity, livingAgents),
                    Average(childTotal, childCount),
                    Average(adultTotal, adultCount),
                    Average(elderTotal, elderCount),
                    starvingAgents,
                    deadFromStarvation);
            }

            private static float Average(float total, int count)
            {
                return count <= 0 ? 0f : total / count;
            }
        }

        /// <summary>
        /// Formatting helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class ProductivityDisplayFormatter
        {
            public static string Format(ProductivitySnapshot snapshot)
            {
                return Format(
                    snapshot?.AverageProductivity ?? 0f,
                    snapshot?.ChildAverageProductivity ?? 0f,
                    snapshot?.AdultAverageProductivity ?? 0f,
                    snapshot?.ElderAverageProductivity ?? 0f,
                    snapshot?.StarvingAgents ?? 0,
                    snapshot?.DeadFromStarvation ?? 0);
            }

            public static string Format(
                float averageProductivity,
                float childAverageProductivity,
                float adultAverageProductivity,
                float elderAverageProductivity,
                int starvingAgents,
                int deadFromStarvation)
            {
                return
                    $"Average productivity: {FormatPercentage(averageProductivity)}\n" +
                    $"Children: {FormatPercentage(childAverageProductivity)}\n" +
                    $"Adults: {FormatPercentage(adultAverageProductivity)}\n" +
                    $"Elders: {FormatPercentage(elderAverageProductivity)}\n" +
                    $"Starving: {starvingAgents}\n" +
                    $"Dead from starvation: {deadFromStarvation}";
            }

            private static string FormatPercentage(float value)
            {
                return (value * 100f).ToString("0.##", CultureInfo.InvariantCulture) + "%";
            }
        }
    }
}
