using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Configuration for profession distribution targets.
    /// Stores the target percentage for each profession (should sum to 1.0 / 100%).
    /// </summary>
    [Serializable]
    public sealed class ProfessionTargets
    {
        /// <summary>
        /// Default target distribution (equal split among all 6 professions).
        /// </summary>
        public const int ProfessionCount = 6;
        public const float DefaultTargetPerProfession = 1f / ProfessionCount;

        private readonly float[] _targets;

        public ProfessionTargets()
        {
            _targets = new float[ProfessionCount];
            SetAllToDefault();
        }

        /// <summary>
        /// Creates a target set from six values in profession order.
        /// The values may be provided as fractions that already sum to 1.0 or as
        /// rough weights that will be normalized to 1.0.
        /// </summary>
        public ProfessionTargets(params float[] targets) : this()
        {
            SetTargets(targets);
        }

        /// <summary>
        /// Gets the target percentage for a profession (0.0 to 1.0).
        /// </summary>
        public float GetTarget(Profession profession)
        {
            ValidateProfession(profession);
            return _targets[(int)profession];
        }

        /// <summary>
        /// Sets the target percentage for a profession.
        /// </summary>
        public void SetTarget(Profession profession, float value)
        {
            ValidateProfession(profession);
            ValidateTargetValue(value);
            _targets[(int)profession] = value;
        }

        /// <summary>
        /// Replaces all profession targets using six values in profession order.
        /// Values are normalized after validation, so the stored targets always sum to 1.0.
        /// </summary>
        public void SetTargets(params float[] targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            if (targets.Length != ProfessionCount)
                throw new ArgumentException($"Expected {ProfessionCount} target values, got {targets.Length}.", nameof(targets));

            for (int i = 0; i < targets.Length; i++)
            {
                ValidateTargetValue(targets[i]);
                _targets[i] = targets[i];
            }

            Normalize();
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
        /// Returns a copy of the current targets in profession order.
        /// </summary>
        public float[] GetTargetsCopy()
        {
            var copy = new float[ProfessionCount];
            Array.Copy(_targets, copy, ProfessionCount);
            return copy;
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

            if (sum <= 0f)
            {
                // Invalid, reset to default
                SetAllToDefault();
                return;
            }

            if (Math.Abs(sum - 1f) < 0.0001f)
            {
                // Already normalized, return
                return;
            }

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i] /= sum;
            }
        }

        /// <summary>
        /// Returns true when the current target values already sum to approximately 1.0.
        /// </summary>
        public bool IsNormalized(float tolerance = 0.0001f)
        {
            return Math.Abs(GetSum() - 1f) <= tolerance;
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

        private static void ValidateProfession(Profession profession)
        {
            int index = (int)profession;
            if (index < 0 || index >= ProfessionCount)
                throw new ArgumentOutOfRangeException(nameof(profession), profession, "Profession is out of range.");
        }

        private static void ValidateTargetValue(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Target must be a finite number.");

            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(nameof(value), "Target must be between 0 and 1");
        }
    }
}
