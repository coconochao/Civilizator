using System.Globalization;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for resource production rates.
    /// Displays overall output and optional per-profession breakdown.
    /// </summary>
    public sealed class ProductionRateDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _rateText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasSnapshot;
        private ProductionRateSnapshot _snapshot;

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
        /// Assigns the text component used for rendering production values.
        /// </summary>
        public void SetRateText(Text rateText)
        {
            _rateText = rateText;
            Refresh();
        }

        /// <summary>
        /// Binds a snapshot of current production rates and refreshes immediately.
        /// </summary>
        public void Bind(ProductionRateSnapshot snapshot)
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
            if (_rateText == null)
            {
                return;
            }

            _rateText.text = _hasSnapshot
                ? ProductionRateDisplayFormatter.Format(_snapshot)
                : ProductionRateDisplayFormatter.Format(0f, 0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Sets the overall and per-profession production rates directly.
        /// </summary>
        public void SetRates(
            float overallPerCycle,
            float woodcutterPerCycle,
            float minerPerCycle,
            float hunterPerCycle,
            float farmerPerCycle)
        {
            if (_rateText == null)
            {
                return;
            }

            _rateText.text = ProductionRateDisplayFormatter.Format(
                overallPerCycle,
                woodcutterPerCycle,
                minerPerCycle,
                hunterPerCycle,
                farmerPerCycle);
        }

        /// <summary>
        /// Snapshot of production rates for the four producer professions.
        /// </summary>
        public readonly struct ProductionRateSnapshot
        {
            public ProductionRateSnapshot(
                float overallPerCycle,
                float woodcutterPerCycle,
                float minerPerCycle,
                float hunterPerCycle,
                float farmerPerCycle)
            {
                OverallPerCycle = overallPerCycle;
                WoodcutterPerCycle = woodcutterPerCycle;
                MinerPerCycle = minerPerCycle;
                HunterPerCycle = hunterPerCycle;
                FarmerPerCycle = farmerPerCycle;
            }

            public float OverallPerCycle { get; }
            public float WoodcutterPerCycle { get; }
            public float MinerPerCycle { get; }
            public float HunterPerCycle { get; }
            public float FarmerPerCycle { get; }
        }

        /// <summary>
        /// Formatting helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class ProductionRateDisplayFormatter
        {
            public static string Format(ProductionRateSnapshot snapshot)
            {
                return Format(
                    snapshot.OverallPerCycle,
                    snapshot.WoodcutterPerCycle,
                    snapshot.MinerPerCycle,
                    snapshot.HunterPerCycle,
                    snapshot.FarmerPerCycle);
            }

            public static string Format(
                float overallPerCycle,
                float woodcutterPerCycle,
                float minerPerCycle,
                float hunterPerCycle,
                float farmerPerCycle)
            {
                return
                    $"Overall: {FormatRate(overallPerCycle)} / cycle\n" +
                    $"Woodcutter: {FormatRate(woodcutterPerCycle)} / cycle\n" +
                    $"Miner: {FormatRate(minerPerCycle)} / cycle\n" +
                    $"Hunter: {FormatRate(hunterPerCycle)} / cycle\n" +
                    $"Farmer: {FormatRate(farmerPerCycle)} / cycle";
            }

            private static string FormatRate(float value)
            {
                return value.ToString("0.##", CultureInfo.InvariantCulture);
            }
        }
    }
}
