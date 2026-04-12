using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Represents a person (citizen) in the simulation.
    /// Agents have position, profession, life stage, health points, and state tracking.
    /// </summary>
    public class Agent
    {
        public GridPos Position { get; set; }
        public Profession Profession { get; set; }
        public LifeStage LifeStage { get; set; }
        public int HitPoints { get; set; }

        /// <summary>
        /// Default hit points for newly spawned agents.
        /// </summary>
        public const int DefaultHitPoints = 10;

        /// <summary>
        /// Creates a new agent at the given position.
        /// Profession defaults to Woodcutter; life stage defaults to Child; HP defaults to 10.
        /// </summary>
        public Agent(GridPos position)
        {
            Position = position;
            Profession = Profession.Woodcutter;
            LifeStage = LifeStage.Child;
            HitPoints = DefaultHitPoints;
        }

        /// <summary>
        /// Creates a new agent with specified profession and life stage.
        /// </summary>
        public Agent(GridPos position, Profession profession, LifeStage lifeStage)
        {
            Position = position;
            Profession = profession;
            LifeStage = lifeStage;
            HitPoints = DefaultHitPoints;
        }

        /// <summary>
        /// Checks if this agent is alive (HP > 0).
        /// </summary>
        public bool IsAlive => HitPoints > 0;

        public override string ToString()
        {
            return $"Agent({Profession} {LifeStage} at {Position}, HP={HitPoints})";
        }
    }

    /// <summary>
    /// Professions available to agents in V1.
    /// </summary>
    public enum Profession
    {
        Woodcutter,
        Miner,
        Hunter,
        Farmer,
        Builder,
        Soldier
    }

    /// <summary>
    /// Life stages for agents.
    /// Progression: Child → Adult → Elder → Death.
    /// </summary>
    public enum LifeStage
    {
        Child,
        Adult,
        Elder
    }

    /// <summary>
    /// Helpers for life stages and aging.
    /// </summary>
    public static class LifeStageHelpers
    {
        /// <summary>
        /// Time (in cycles) for each life stage transition.
        /// </summary>
        public const int ChildToAdultCycles = 10;
        public const int AdultToElderCycles = 60;
        public const int ElderToDeathCycles = 10;

        /// <summary>
        /// Gets the productivity multiplier for a given life stage.
        /// Adult: 1.0 (100%)
        /// Child: 0.5 (50%)
        /// Elder: 0.5 (50%)
        /// </summary>
        public static float GetProductivityMultiplier(LifeStage stage)
        {
            return stage switch
            {
                LifeStage.Adult => 1.0f,
                LifeStage.Child => 0.5f,
                LifeStage.Elder => 0.5f,
                _ => throw new ArgumentException($"Unknown life stage: {stage}")
            };
        }

        /// <summary>
        /// Gets the next life stage.
        /// Child → Adult
        /// Adult → Elder
        /// Elder → (death, returns Elder; use IsAlive check)
        /// </summary>
        public static LifeStage GetNextStage(LifeStage current)
        {
            return current switch
            {
                LifeStage.Child => LifeStage.Adult,
                LifeStage.Adult => LifeStage.Elder,
                LifeStage.Elder => LifeStage.Elder,
                _ => throw new ArgumentException($"Unknown life stage: {current}")
            };
        }

        /// <summary>
        /// Gets the aging duration for a given life stage (cycles until next transition).
        /// </summary>
        public static int GetAgingDuration(LifeStage stage)
        {
            return stage switch
            {
                LifeStage.Child => ChildToAdultCycles,
                LifeStage.Adult => AdultToElderCycles,
                LifeStage.Elder => ElderToDeathCycles,
                _ => throw new ArgumentException($"Unknown life stage: {stage}")
            };
        }
    }
}
