using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Player-facing reproduction configuration.
    /// Stores the global reproduction rate as a normalized fraction in the range 0..1.
    /// </summary>
    [Serializable]
    public sealed class ReproductionSettings
    {
        /// <summary>
        /// Default reproduction rate used when no player setting has been provided.
        /// </summary>
        public const float DefaultReproductionRate = 0.5f;

        private float _reproductionRate = DefaultReproductionRate;

        /// <summary>
        /// Gets or sets the reproduction rate as a probability from 0.0 to 1.0.
        /// </summary>
        public float ReproductionRate
        {
            get => _reproductionRate;
            set => SetReproductionRate(value);
        }

        public ReproductionSettings()
        {
        }

        public ReproductionSettings(float reproductionRate)
        {
            SetReproductionRate(reproductionRate);
        }

        /// <summary>
        /// Sets the reproduction rate after validating the value.
        /// </summary>
        public void SetReproductionRate(float reproductionRate)
        {
            ValidateReproductionRate(reproductionRate);
            _reproductionRate = reproductionRate;
        }

        /// <summary>
        /// Applies this settings object to the live simulation systems.
        /// </summary>
        public void ApplyToSimulation()
        {
            ReproductionSystem.SetReproductionRate(_reproductionRate);
        }

        /// <summary>
        /// Creates a settings snapshot from the current simulation state.
        /// </summary>
        public static ReproductionSettings FromSimulation()
        {
            return new ReproductionSettings(ReproductionSystem.ReproductionRate);
        }

        private static void ValidateReproductionRate(float reproductionRate)
        {
            if (float.IsNaN(reproductionRate) || float.IsInfinity(reproductionRate))
                throw new ArgumentOutOfRangeException(nameof(reproductionRate), "Reproduction rate must be a finite number.");

            if (reproductionRate < 0f || reproductionRate > 1f)
                throw new ArgumentOutOfRangeException(nameof(reproductionRate), "Reproduction rate must be between 0 and 1.");
        }
    }
}
