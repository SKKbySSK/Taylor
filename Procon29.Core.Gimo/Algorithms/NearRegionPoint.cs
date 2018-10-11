using System.Collections.Generic;

namespace Procon29.Core.Gimo.Algorithms
{
    class NearRegionPoint
    {
        public int? TeamScore { get; set; } = null;

        public int? EnemyScore { get; set; } = null;

        public Direction MovedDirection { get; set; }

        public Point To { get; set; }

        public Point From { get; set; }

        public Point Max { get; set; }

        public Point Min { get; set; }

        public List<Point> Went { get; set; }

        public NearRegionPoint Parent { get; set; }
        
        public List<NearRegionPoint> Children { get; } = new List<NearRegionPoint>(7);

        private Genetic.ScoringEfficiency efficiency;
        public Genetic.ScoringEfficiency GetScoringEfficiency()
        {
            if (efficiency == null)
                efficiency = new Genetic.ScoringEfficiency(TeamScore.Value - EnemyScore.Value, Went.Count);
            return efficiency;
        }

        //public List<NearRegionPoint> GetLastChildren(NearRegionPoint from = null, List<NearRegionPoint> results = null)
        //{
        //    NearRegionPoint current = from;
        //    if (from == null)
        //        current = this;

        //    if (results == null)
        //        results = new List<NearRegionPoint>();

        //    foreach (var r in current.Children)
        //    {
        //        if (r.Children.Count > 0)
        //        {
        //            GetLastChildren(r, results);
        //        }
        //        else
        //        {
        //            results.Add(r);
        //        }
        //    }

        //    return results;
        //}

        ///// <summary>
        ///// 最も得点が高いルートを計算します
        ///// </summary>
        ///// <returns></returns>
        //public List<NearRegionPoint> EvaluateMaximumScore()
        //{
        //    var list = EvaluateMaximumOccupyScore(null);
        //    list.Add(this);
        //    list.Reverse();

        //    return list;
        //}

        //private List<NearRegionPoint> EvaluateMaximumOccupyScore(List<NearRegionPoint> results = null)
        //{
        //    if (results == null)
        //    {
        //        results = new List<NearRegionPoint>();
        //    }

        //    if (Children.Count > 0)
        //    {
        //        var maxScore = Children.Where(r => r.MaxScore != null).First();
        //        maxScore.EvaluateMaximumOccupyScore(results);
        //        results.Add(maxScore);
        //    }

        //    return results;
        //}
    }
}
