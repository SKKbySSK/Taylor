namespace Procon29.Core.Gimo.Algorithms
{
    class UsefulRegionDestination
    {
        public ICell Cell { get; set; }

        public Point Point { get; set; }

        public Agent Agent { get; set; }

        public double Length { get; set; } = double.MaxValue;
    }
}