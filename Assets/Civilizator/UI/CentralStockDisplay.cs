using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for the four central storage stocks.
    /// </summary>
    public sealed class CentralStockDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text _stockText;

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private CentralStorage _storage;

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
        /// Binds this display to a central storage instance and immediately refreshes the text.
        /// </summary>
        public void Bind(CentralStorage storage)
        {
            _storage = storage;
            Refresh();
        }

        /// <summary>
        /// Assigns the text component used for rendering stock values.
        /// </summary>
        public void SetStockText(Text stockText)
        {
            _stockText = stockText;
            Refresh();
        }

        /// <summary>
        /// Clears the bound storage source.
        /// </summary>
        public void ClearBinding()
        {
            _storage = null;
            Refresh();
        }

        /// <summary>
        /// Updates the text from the bound storage, or shows zeroes if no storage is bound.
        /// </summary>
        public void Refresh()
        {
            if (_stockText == null)
            {
                return;
            }

            _stockText.text = _storage == null
                ? CentralStockDisplayFormatter.Format(0, 0, 0, 0)
                : CentralStockDisplayFormatter.Format(_storage.GetAllStocks());
        }

        /// <summary>
        /// Directly sets the four stock values without requiring a storage binding.
        /// </summary>
        public void SetStocks(int logs, int ore, int meat, int plantFood)
        {
            if (_stockText == null)
            {
                return;
            }

            _stockText.text = CentralStockDisplayFormatter.Format(logs, ore, meat, plantFood);
        }

        /// <summary>
        /// Helper used by tests and any future non-MonoBehaviour presenter.
        /// </summary>
        public static class CentralStockDisplayFormatter
        {
            public static string Format((int logs, int ore, int meat, int plantFood) stocks)
            {
                return Format(stocks.logs, stocks.ore, stocks.meat, stocks.plantFood);
            }

            public static string Format(int logs, int ore, int meat, int plantFood)
            {
                return
                    $"Logs: {logs}\n" +
                    $"Ore: {ore}\n" +
                    $"Meat: {meat}\n" +
                    $"Plant food: {plantFood}";
            }
        }
    }
}
