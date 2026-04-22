using System;
using System.Collections.Generic;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Hysteresis thresholds for producer professions (Woodcutter, Miner, Hunter, Farmer).
    /// Each profession has a start (produce when stock is below) and stop (halt production when stock is above) threshold.
    /// Values are expressed as a fraction of the maximum stock capacity (0.0 – 1.0).
    /// </summary>
    public static class ProducerThresholds
    {
        // Default sensible thresholds (can be overridden by config UI later)
        private static readonly Dictionary<Profession, (float start, float stop)> _defaults = new()
        {
            { Profession.Woodcutter, (0.2f, 0.8f) }, // Logs
            { Profession.Miner,      (0.2f, 0.8f) }, // Ore
            { Profession.Hunter,     (0.2f, 0.8f) }, // Meat
            { Profession.Farmer,     (0.2f, 0.8f) }  // PlantFood
        };

        // Current thresholds – start with defaults; can be modified at runtime via UI or config loading.
        private static readonly Dictionary<Profession, (float start, float stop)> _thresholds = new(_defaults);

        /// <summary>
        /// Gets the start threshold for the given producer profession.
        /// Production should start when the stock is **below** this value.
        /// </summary>
        public static float GetStartThreshold(Profession profession)
        {
            if (!_thresholds.TryGetValue(profession, out var pair))
                throw new ArgumentException($"No thresholds defined for profession {profession}");
            return pair.start;
        }

        /// <summary>
        /// Gets the stop threshold for the given producer profession.
        /// Production should stop when the stock is **above** this value.
        /// </summary>
        public static float GetStopThreshold(Profession profession)
        {
            if (!_thresholds.TryGetValue(profession, out var pair))
                throw new ArgumentException($"No thresholds defined for profession {profession}");
            return pair.stop;
        }

        /// <summary>
        /// Sets custom thresholds for a profession. Values must satisfy 0 ≤ start < stop ≤ 1.
        /// </summary>
        public static void SetThresholds(Profession profession, float start, float stop)
        {
            ValidateThresholdValue(start, nameof(start));
            ValidateThresholdValue(stop, nameof(stop));
            if (start < 0f || start > 1f || stop < 0f || stop > 1f)
                throw new ArgumentOutOfRangeException("Thresholds must be between 0 and 1.");
            if (start >= stop)
                throw new ArgumentException("Start threshold must be less than stop threshold.");

            _thresholds[profession] = (start, stop);
        }

        /// <summary>
        /// Resets all thresholds to their default values.
        /// </summary>
        public static void ResetToDefaults()
        {
            _thresholds.Clear();
            foreach (var kvp in _defaults)
                _thresholds[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Returns a copy of the current thresholds dictionary (read‑only).
        /// Useful for UI display or serialization.
        /// </summary>
        public static IReadOnlyDictionary<Profession, (float start, float stop)> GetAll()
        {
            return new Dictionary<Profession, (float start, float stop)>(_thresholds);
        }

        /// <summary>
        /// Determines whether a producer should be producing or improving based on current stock levels.
        /// Implements hysteresis: below start threshold → produce; above stop threshold → improve;
        /// between thresholds → maintain current state.
        /// </summary>
        /// <param name="profession">The producer profession (Woodcutter, Miner, Hunter, Farmer)</param>
        /// <param name="currentStock">The current stock level for this profession's resource</param>
        /// <param name="maxStock">The maximum stock capacity (for normalization)</param>
        /// <returns>True if the producer should be producing, false if improving</returns>
        public static bool ShouldBeProducing(Profession profession, int currentStock, int maxStock)
        {
            if (maxStock <= 0)
                throw new ArgumentException("Max stock must be greater than zero.");

            float normalizedStock = (float)currentStock / maxStock;
            var thresholds = _thresholds[profession];

            // Hysteresis logic: below start threshold → produce; above stop threshold → improve
            return Math.Round(normalizedStock, 2) < Math.Round(thresholds.start, 2);
        }

        private static void ValidateThresholdValue(float value, string paramName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(paramName, "Threshold must be finite.");
        }
    }
}
