using System;

namespace Procon29.Core.Algorithms
{
    public class DefaultRandom : IRandom
    {
        public DefaultRandom()
        {
            Random = new Random();
        }

        public DefaultRandom(int seed)
        {
            Random = new Random(seed);
        }

        private Random Random { get; }

        public int Next(int min, int max)
        {
            return Random.Next(min, max);
        }
    }
}
