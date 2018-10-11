using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Procon29.Core.Algorithms
{
    /// <summary>
    /// セルの優先度を点数に応じて割り振るクラス
    /// </summary>
    public class CellPriorityDetector
    {
        List<ICell> scores = new List<ICell>();
        double sum = 0;
        double av = 0;
        double min = 0;
        double max = 0;
        bool f = true;

        public void SetField(ICell[,] field)
        {
            scores.Clear();

            foreach (var c in field)
            {
                if (f)
                {
                    min = c.Score;
                    max = c.Score;
                    f = false;
                }

                min = Math.Min(min, c.Score);
                max = Math.Max(max, c.Score);
                sum += c.Score - Rule.MinimumCellScore;
                scores.Add(c);
            }

            av = sum / scores.Count + Rule.MinimumCellScore;
        }

        public void Detect(ICell cell)
        {
            int range = Rule.MaximumCellScore - Rule.MinimumCellScore;
            double uscore = cell.Score - Rule.MinimumCellScore;
            int ratio = (int)(uscore / range * 100);

            var pri = Enum.GetValues(typeof(CellPriority)).OfType<CellPriority>().OrderBy(c => (int)c);
            foreach (var p in pri)
            {
                int r = (int)p;
                if (ratio < r)
                {
                    cell.Priority = p;
                    break;
                }
            }
        }
    }
}
