using System.Windows.Media;

namespace Procon29.Core
{
    /// <summary>
    /// UIのセルに描画する色を定義するクラス
    /// </summary>
    public class CellColor
    {
        public CellColor(Point cell, Color fill)
        {
            Cell = cell;
            Fill = fill;
        }

        public Point Cell { get; }

        public Color Fill { get; }
    }
}