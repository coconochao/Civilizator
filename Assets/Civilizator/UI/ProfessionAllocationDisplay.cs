using System;
using System.Globalization;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Minimal HUD panel for profession target allocation versus actual headcount.
    /// Each profession row uses a slider for the target share and a text readout for the actual share.
    /// </summary>
    public sealed class ProfessionAllocationDisplay : MonoBehaviour
    {
        private const int ProfessionCount = ProfessionTargets.ProfessionCount;

        [SerializeField]
        private Slider[] _targetSliders = new Slider[ProfessionCount];

        [SerializeField]
        private Text[] _actualTexts = new Text[ProfessionCount];

        [SerializeField]
        private bool _refreshEveryFrame = true;

        private bool _hasTargetSnapshot;
        private ProfessionTargets _targets;

        private bool _hasActualSnapshot;
        private int[] _actualCounts = new int[ProfessionCount];
        private int _actualPopulation;

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
        /// Assigns the target sliders used for each profession row.
        /// </summary>
        public void SetTargetSliders(params Slider[] sliders)
        {
            _targetSliders = sliders ?? Array.Empty<Slider>();
            Refresh();
        }

        /// <summary>
        /// Assigns the actual-percentage readout texts used for each profession row.
        /// </summary>
        public void SetActualTexts(params Text[] texts)
        {
            _actualTexts = texts ?? Array.Empty<Text>();
            Refresh();
        }

        /// <summary>
        /// Binds a profession target configuration and immediately updates the sliders.
        /// </summary>
        public void BindTargets(ProfessionTargets targets)
        {
            _targets = targets;
            _hasTargetSnapshot = targets != null;
            Refresh();
        }

        /// <summary>
        /// Binds actual profession counts and immediately refreshes the readout text.
        /// </summary>
        public void BindActualCounts(int[] counts, int totalPopulation)
        {
            _actualCounts = counts ?? new int[ProfessionCount];
            _actualPopulation = Math.Max(0, totalPopulation);
            _hasActualSnapshot = counts != null;
            Refresh();
        }

        /// <summary>
        /// Sets both target and actual data in one call.
        /// </summary>
        public void Bind(ProfessionTargets targets, int[] counts, int totalPopulation)
        {
            _targets = targets;
            _hasTargetSnapshot = targets != null;
            _actualCounts = counts ?? new int[ProfessionCount];
            _actualPopulation = Math.Max(0, totalPopulation);
            _hasActualSnapshot = counts != null;
            Refresh();
        }

        /// <summary>
        /// Refreshes the UI from the current bindings.
        /// </summary>
        public void Refresh()
        {
            RefreshTargets();
            RefreshActuals();
        }

        private void RefreshTargets()
        {
            if (_targetSliders == null)
            {
                return;
            }

            for (int i = 0; i < ProfessionCount && i < _targetSliders.Length; i++)
            {
                Slider slider = _targetSliders[i];
                if (slider == null)
                {
                    continue;
                }

                ConfigureSlider(slider);
                slider.value = _hasTargetSnapshot ? _targets.GetTarget((Profession)i) : 0f;
            }
        }

        private void RefreshActuals()
        {
            if (_actualTexts == null)
            {
                return;
            }

            for (int i = 0; i < ProfessionCount && i < _actualTexts.Length; i++)
            {
                Text text = _actualTexts[i];
                if (text == null)
                {
                    continue;
                }

                float actualPercentage = GetActualPercentage(i);
                text.text = ProfessionAllocationDisplayFormatter.FormatPercentage(actualPercentage);
            }
        }

        private float GetActualPercentage(int professionIndex)
        {
            if (!_hasActualSnapshot || _actualPopulation <= 0 || _actualCounts == null || professionIndex < 0 || professionIndex >= _actualCounts.Length)
            {
                return 0f;
            }

            return (float)_actualCounts[professionIndex] / _actualPopulation;
        }

        private static void ConfigureSlider(Slider slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        /// <summary>
        /// Helper used by tests and future non-MonoBehaviour presenters.
        /// </summary>
        public static class ProfessionAllocationDisplayFormatter
        {
            public static string FormatPercentage(float value)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:0.##}%", value * 100f);
            }
        }
    }
}
