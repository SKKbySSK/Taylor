using System.Collections.Generic;
using System.Linq;

namespace Procon29.Core.Gimo.Algorithms
{
    class NearRegionResult
    {
        public NearRegionResult(NearRegionPoint root, List<NearRegionPoint> ends)
        {
            Ends = ends;
            Root = root;
        }

        public List<NearRegionPoint> Ends { get; }

        public NearRegionPoint Root { get; }
    }
}
