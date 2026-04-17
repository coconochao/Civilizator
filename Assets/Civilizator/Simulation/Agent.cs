using System;

namespace Civilizator.Simulation
{
    /// <summary>
    /// Represents a person (citizen) in the simulation.
    /// Agents have position, profession, life stage, health points, and state tracking.
    /// Agents can be assigned to houses, which provides a +20% productivity bonus.
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// Unique identifier for this agent.
        /// Assigned at creation and never changes.
        /// </summary>
        public int Id { get; }

        public GridPos Position { get; set; }
        public Profession Profession { get; set; }
        public LifeStage LifeStage { get; set; }
        public int HitPoints { get; set; }
        public int? AssignedHouseId { get; set; }

        /// <summary>
        /// Number of resource units currently being carried by this agent.
        /// </summary>
        public int CarriedResources { get; set; } = 0;

        /// <summary>
        /// Tracks whether this agent has eaten during the current cycle.
        /// Reset at the start of each new cycle.
        /// </summary>
        public bool HasEatenThisCycle { get; set; } = false;

        /// <summary>
        /// Tracks eating state including starvation penalties.
        /// </summary>
        public AgentEatingState EatingState { get; } = new AgentEatingState();

        /// <summary>
        /// Default hit points for newly spawned agents.
        /// </summary>
        public const int DefaultHitPoints = 10;

        /// <summary>
        /// Productivity bonus (additive) when assigned to a house: +20%.
        /// Stored as decimal (0.2f).
        /// </summary>
        public const float HouseAssignmentBonus = 0.2f;

        /// <summary>
        /// Base carry capacity per unit of productivity.
        /// Total capacity = 10 × productivity_multiplier.
        /// </summary>
        public const int BaseCarryCapacity = 10;

        /// <summary>
        /// Creates a new agent at the given position with an automatically assigned ID.
        /// Profession defaults to Woodcutter; life stage defaults to Child; HP defaults to 10.
        /// House assignment defaults to null (unassigned).
        /// </summary>
        public Agent(GridPos position)
        {
            Id = NextId++;
            Position = position;
            Profession = Profession.Woodcutter;
            LifeStage = LifeStage.Child;
            HitPoints = DefaultHitPoints;
            AssignedHouseId = null;
        }

        /// <summary>
        /// Creates a new agent with specified ID at the given position.
        /// Profession defaults to Woodcutter; life stage defaults to Child; HP defaults to 10.
        /// </summary>
        public Agent(GridPos position, int id)
        {
            Id = id;
            Position = position;
            Profession = Profession.Woodcutter;
            LifeStage = LifeStage.Child;
            HitPoints = DefaultHitPoints;
            AssignedHouseId = null;
        }

        /// <summary>
        /// Creates a new agent with specified profession and life stage.
        /// </summary>
        public Agent(GridPos position, Profession profession, LifeStage lifeStage)
        {
            Id = NextId++;
            Position = position;
            Profession = profession;
            LifeStage = lifeStage;
            HitPoints = DefaultHitPoints;
            AssignedHouseId = null;
        }

        /// <summary>
        /// Checks if this agent is alive (HP > 0).
        /// </summary>
        public bool IsAlive => HitPoints > 0;

        /// <summary>
        /// Checks if this agent is assigned to a house.
        /// </summary>
        public bool IsHouseAssigned => AssignedHouseId.HasValue;

        /// <summary>
        /// Gets the total productivity multiplier for this agent.
        /// Base multiplier by life stage + house assignment bonus (if assigned) - starvation penalty.
        /// </summary>
        public float GetProductivityMultiplier()
        {
            float baseMultiplier = LifeStageHelpers.GetProductivityMultiplier(LifeStage);
            if (IsHouseAssigned)
                baseMultiplier += HouseAssignmentBonus;
            
            // Apply starvation penalty (subtractive)
            float withStarvation = baseMultiplier - EatingState.StarvationPenalty;
            
            // Clamp to [0, ∞) range (can't go negative)
            return System.Math.Max(0f, withStarvation);
        }

        /// <summary>
        /// Gets the carry capacity for this agent.
        /// Capacity = 10 × productivity_multiplier.
        /// </summary>
        public int GetCarryCapacity()
        {
            float productivityMult = GetProductivityMultiplier();
            return (int)(BaseCarryCapacity * productivityMult);
        }

        /// <summary>
        /// Marks this agent as having eaten during the current cycle.
        /// Also clears any accumulated starvation penalty from previous failed cycles.
        /// </summary>
        public void MarkAsEaten()
        {
            HasEatenThisCycle = true;
            EatingState.ResetStarvationPenalty();
        }

        /// <summary>
        /// Resets the eating flag for a new cycle.
        /// Called at the start of each simulation cycle.
        /// </summary>
        public void ResetEatingFlag()
        {
            HasEatenThisCycle = false;
        }

        /// <summary>
        /// Counter for generating unique agent IDs.
        /// </summary>
        private static int NextId { get; set; } = 1;

        public override string ToString()
        {
            return $"Agent #{Id}({Profession} {LifeStage} at {Position}, HP={HitPoints})";
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
