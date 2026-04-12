namespace Civilizator.Simulation
{
    /// <summary>
    /// A resource spawned by a facility.
    /// Unlike natural nodes, spawned resources are generated each cycle by facilities
    /// (Plantation, Farm, CattleFarm) and can be collected by agents.
    /// </summary>
    public class SpawnedResource
    {
        /// <summary>
        /// The kind of resource (Logs, Meat, PlantFood).
        /// Ore is never spawned (quarries don't spawn, only enable indefinite gathering).
        /// </summary>
        public ResourceKind Kind { get; }

        /// <summary>
        /// The position where this resource spawned.
        /// </summary>
        public GridPos Position { get; }

        /// <summary>
        /// Whether this resource has been collected.
        /// </summary>
        public bool IsCollected { get; set; }

        public SpawnedResource(ResourceKind kind, GridPos position)
        {
            Kind = kind;
            Position = position;
            IsCollected = false;
        }

        public override string ToString()
        {
            return $"SpawnedResource({Kind} at {Position}, collected={IsCollected})";
        }
    }
}
