namespace Civilizator.Simulation
{
    /// <summary>
    /// Represents a position on the 100x100 tile grid.
    /// Provides Manhattan distance calculations for nearest/radius operations.
    /// </summary>
    public readonly struct GridPos
    {
        public const int MapWidth = 100;
        public const int MapHeight = 100;

        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            // Clamp to bounds
            X = x < 0 ? 0 : x >= MapWidth ? MapWidth - 1 : x;
            Y = y < 0 ? 0 : y >= MapHeight ? MapHeight - 1 : y;
        }

        /// <summary>
        /// Manhattan distance between two grid positions.
        /// Distance = |x1 - x2| + |y1 - y2|
        /// </summary>
        public static int Manhattan(GridPos a, GridPos b)
        {
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
        }

        /// <summary>
        /// Manhattan distance from this position to another.
        /// </summary>
        public int DistanceTo(GridPos other)
        {
            return Manhattan(this, other);
        }

        /// <summary>
        /// Check if this position is within bounds.
        /// </summary>
        public bool IsInBounds()
        {
            return X >= 0 && X < MapWidth && Y >= 0 && Y < MapHeight;
        }

        public override string ToString()
        {
            return $"GridPos({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridPos other))
                return false;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return (X * 73856093) ^ (Y * 19349663);
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return !(a == b);
        }
    }
}
