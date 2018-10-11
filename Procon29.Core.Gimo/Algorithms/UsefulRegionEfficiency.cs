using System.Collections.Generic;
using Procon29.Core.Extensions;

namespace Procon29.Core.Gimo.Algorithms
{
    class UsefulRegionEfficiency
    {
        public ICell[,] Occupy { get; set; }

        public ICell[,] Region { get; set; }
        
        public UsefulRegionDestination Destination { get; set; }

        public Genetic.ScoringEfficiency Efficiency { get; set; }

        public Helper.Way Way { get; set; }
    }
}