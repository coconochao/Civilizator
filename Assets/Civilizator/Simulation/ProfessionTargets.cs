using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Configuration for profession distribution targets.
    /// Stores the target percentage for each profession (should sum to 1.0 / 100%).
    /// </summary>
    public class ProfessionTargets
    {
        /// <summary>
        /// Default target distribution (equal split among all 6 professions).
        /// </summary>
        public const float DefaultTargetPerProfession = 1f / 6f;

        private float[] _targets;

        public ProfessionTargets()
        {
            _targets = new float[6];
            SetAllToDefault();
        }

        /// <summary>
        /// Gets the target percentage for a profession (0.0 to 1.0).
        /// </summary>
        public float GetTarget(Profession profession)
        {
            return _targets[(int)profession];
        }

        /// <summary>
        /// Sets the target percentage for a profession.
        /// </summary>
        public void SetTarget(Profession profession, float value)
        {
            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(nameof(value), "Target must be between 0 and 1");
            _targets[(int)profession] = value;
        }

        /// <summary>
        /// Sets all professions to equal default distribution (1/6 each).
        /// </summary>
        public void SetAllToDefault()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i] = DefaultTargetPerProfession;
            }
        }

        /// <summary>
        /// Normalizes targets so they sum to 1.0.
        /// Useful after manually setting individual targets.
        /// </summary>
        public void Normalize()
        {
            float sum = 0f;
            for (int i = 0; i < _targets.Length; i++)
            {
                sum += _targets[i];
            }

            if (sum <= 0f || Math.Abs(sum - 1f) < 0.0001f)
            {
                // Already normalized or invalid, reset to default
                SetAllToDefault();
                return;
            }

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i] /= sum;
            }
        }

        /// <summary>
        /// Gets the sum of all target percentages.
        /// </summary>
        public float GetSum()
        {
            float sum = 0f;
            for (int i = 0; i < _targets.Length; i++)
            {
                sum += _targets[i];
            }
            return sum;
        }
    }
}