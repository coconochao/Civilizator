using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Player-facing soldier configuration.
    /// Groups the patrol/improve split and tower emphasis controls into one serializable object.
    /// </summary>
    [Serializable]
    public sealed class SoldierControls
    {
        /// <summary>
        /// Default share of soldiers that should patrol.
        /// </summary>
        public const float DefaultPatrolTargetShare = SoldierModeSwitchSystem.DefaultPatrolTargetShare;

        /// <summary>
        /// Default discrepancy threshold for mode switching.
        /// </summary>
        public const float DefaultSwitchThreshold = SoldierModeSwitchSystem.DefaultSwitchThreshold;

        /// <summary>
        /// Default cooldown in cycles between switches for the same soldier.
        /// </summary>
        public const int DefaultSwitchCooldownCycles = SoldierModeSwitchSystem.DefaultSwitchCooldownCycles;

        /// <summary>
        /// Default bias for tower construction vs upgrades.
        /// </summary>
        public const float DefaultTowerBuildEmphasis = SoldierImprovementControls.DefaultTowerBuildEmphasis;

        private float _patrolTargetShare = DefaultPatrolTargetShare;
        private float _switchThreshold = DefaultSwitchThreshold;
        private int _switchCooldownCycles = DefaultSwitchCooldownCycles;
        private float _towerBuildEmphasis = DefaultTowerBuildEmphasis;

        /// <summary>
        /// Gets or sets the target share of soldiers that should be patrolling.
        /// </summary>
        public float PatrolTargetShare
        {
            get => _patrolTargetShare;
            set => SetPatrolTargetShare(value);
        }

        /// <summary>
        /// Gets or sets the discrepancy threshold that triggers soldier mode switching.
        /// </summary>
        public float SwitchThreshold
        {
            get => _switchThreshold;
            set => SetSwitchThreshold(value);
        }

        /// <summary>
        /// Gets or sets the cooldown period between switches for the same soldier.
        /// </summary>
        public int SwitchCooldownCycles
        {
            get => _switchCooldownCycles;
            set => SetSwitchCooldownCycles(value);
        }

        /// <summary>
        /// Gets or sets the tower build emphasis for soldier improvement logic.
        /// </summary>
        public float TowerBuildEmphasis
        {
            get => _towerBuildEmphasis;
            set => SetTowerBuildEmphasis(value);
        }

        public SoldierControls()
        {
        }

        public SoldierControls(float patrolTargetShare, float switchThreshold, int switchCooldownCycles, float towerBuildEmphasis)
        {
            SetPatrolTargetShare(patrolTargetShare);
            SetSwitchThreshold(switchThreshold);
            SetSwitchCooldownCycles(switchCooldownCycles);
            SetTowerBuildEmphasis(towerBuildEmphasis);
        }

        /// <summary>
        /// Applies these settings to the provided soldier mode switch system and the static
        /// soldier improvement control layer.
        /// </summary>
        public void ApplyToSimulation(SoldierModeSwitchSystem modeSwitchSystem)
        {
            if (modeSwitchSystem == null)
                throw new ArgumentNullException(nameof(modeSwitchSystem));

            modeSwitchSystem.SetPatrolTargetShare(_patrolTargetShare);
            modeSwitchSystem.SetSwitchThreshold(_switchThreshold);
            modeSwitchSystem.SetSwitchCooldownCycles(_switchCooldownCycles);
            SoldierImprovementControls.SetTowerBuildEmphasis(_towerBuildEmphasis);
        }

        /// <summary>
        /// Creates a control snapshot from the current simulation systems.
        /// </summary>
        public static SoldierControls FromSimulation(SoldierModeSwitchSystem modeSwitchSystem)
        {
            if (modeSwitchSystem == null)
                throw new ArgumentNullException(nameof(modeSwitchSystem));

            return new SoldierControls(
                modeSwitchSystem.PatrolTargetShare,
                modeSwitchSystem.SwitchThreshold,
                modeSwitchSystem.SwitchCooldownCycles,
                SoldierImprovementControls.GetTowerBuildEmphasis());
        }

        public void SetPatrolTargetShare(float value)
        {
            ValidateNormalizedFraction(value, nameof(PatrolTargetShare));
            _patrolTargetShare = value;
        }

        public void SetSwitchThreshold(float value)
        {
            ValidateNormalizedFraction(value, nameof(SwitchThreshold));
            _switchThreshold = value;
        }

        public void SetSwitchCooldownCycles(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Cooldown cycles must be non-negative.");

            _switchCooldownCycles = value;
        }

        public void SetTowerBuildEmphasis(float value)
        {
            ValidateNormalizedFraction(value, nameof(TowerBuildEmphasis));
            _towerBuildEmphasis = value;
        }

        private static void ValidateNormalizedFraction(float value, string paramName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(paramName, "Value must be finite.");

            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(paramName, "Value must be between 0 and 1.");
        }
    }
}
