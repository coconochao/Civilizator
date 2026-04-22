using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Player-facing configuration for producer hysteresis thresholds.
    /// Stores one start/stop pair per producer profession.
    /// </summary>
    [Serializable]
    public sealed class ProducerThresholdSettings
    {
        /// <summary>
        /// Default threshold pair used for all producer professions.
        /// </summary>
        public const float DefaultStartThreshold = 0.2f;
        public const float DefaultStopThreshold = 0.8f;

        public ProducerThresholdPair Woodcutter = new ProducerThresholdPair(DefaultStartThreshold, DefaultStopThreshold);
        public ProducerThresholdPair Miner = new ProducerThresholdPair(DefaultStartThreshold, DefaultStopThreshold);
        public ProducerThresholdPair Hunter = new ProducerThresholdPair(DefaultStartThreshold, DefaultStopThreshold);
        public ProducerThresholdPair Farmer = new ProducerThresholdPair(DefaultStartThreshold, DefaultStopThreshold);

        /// <summary>
        /// Applies these settings to the live producer threshold system.
        /// </summary>
        public void ApplyToSimulation()
        {
            ProducerThresholds.SetThresholds(Profession.Woodcutter, Woodcutter.StartThreshold, Woodcutter.StopThreshold);
            ProducerThresholds.SetThresholds(Profession.Miner, Miner.StartThreshold, Miner.StopThreshold);
            ProducerThresholds.SetThresholds(Profession.Hunter, Hunter.StartThreshold, Hunter.StopThreshold);
            ProducerThresholds.SetThresholds(Profession.Farmer, Farmer.StartThreshold, Farmer.StopThreshold);
        }

        /// <summary>
        /// Creates a configuration snapshot from the current live simulation values.
        /// </summary>
        public static ProducerThresholdSettings FromSimulation()
        {
            var settings = new ProducerThresholdSettings();
            settings.Woodcutter = ReadPair(Profession.Woodcutter);
            settings.Miner = ReadPair(Profession.Miner);
            settings.Hunter = ReadPair(Profession.Hunter);
            settings.Farmer = ReadPair(Profession.Farmer);
            return settings;
        }

        /// <summary>
        /// Sets the thresholds for a single profession.
        /// </summary>
        public void SetThresholds(Profession profession, float startThreshold, float stopThreshold)
        {
            var pair = new ProducerThresholdPair(startThreshold, stopThreshold);
            switch (profession)
            {
                case Profession.Woodcutter:
                    Woodcutter = pair;
                    break;
                case Profession.Miner:
                    Miner = pair;
                    break;
                case Profession.Hunter:
                    Hunter = pair;
                    break;
                case Profession.Farmer:
                    Farmer = pair;
                    break;
                default:
                    throw new ArgumentException($"Profession {profession} does not use producer thresholds.", nameof(profession));
            }
        }

        /// <summary>
        /// Gets the thresholds for a producer profession.
        /// </summary>
        public ProducerThresholdPair GetThresholds(Profession profession)
        {
            return profession switch
            {
                Profession.Woodcutter => Woodcutter,
                Profession.Miner => Miner,
                Profession.Hunter => Hunter,
                Profession.Farmer => Farmer,
                _ => throw new ArgumentException($"Profession {profession} does not use producer thresholds.", nameof(profession))
            };
        }

        private static ProducerThresholdPair ReadPair(Profession profession)
        {
            return new ProducerThresholdPair(
                ProducerThresholds.GetStartThreshold(profession),
                ProducerThresholds.GetStopThreshold(profession));
        }
    }

    /// <summary>
    /// Start/stop pair used for producer hysteresis thresholds.
    /// </summary>
    [Serializable]
    public struct ProducerThresholdPair
    {
        public float StartThreshold;
        public float StopThreshold;

        public ProducerThresholdPair(float startThreshold, float stopThreshold)
        {
            Validate(startThreshold, stopThreshold);
            StartThreshold = startThreshold;
            StopThreshold = stopThreshold;
        }

        public void Set(float startThreshold, float stopThreshold)
        {
            Validate(startThreshold, stopThreshold);
            StartThreshold = startThreshold;
            StopThreshold = stopThreshold;
        }

        private static void Validate(float startThreshold, float stopThreshold)
        {
            ValidateFinite(startThreshold, nameof(startThreshold));
            ValidateFinite(stopThreshold, nameof(stopThreshold));

            if (startThreshold < 0f || startThreshold > 1f || stopThreshold < 0f || stopThreshold > 1f)
                throw new ArgumentOutOfRangeException(nameof(startThreshold), "Thresholds must be between 0 and 1.");

            if (startThreshold >= stopThreshold)
                throw new ArgumentException("Start threshold must be less than stop threshold.");
        }

        private static void ValidateFinite(float value, string paramName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(paramName, "Threshold must be finite.");
        }
    }
}
