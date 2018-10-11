using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Procon29.Core
{
    /// <summary>
    /// 点数を表すセル
    /// </summary>
    public class ScoreCell : CellBase
    {
        //セルの得点を設定
        public ScoreCell(int score)
        {
            Score = score;
            Text = score.ToString();
        }

        //セルのクローンを作って返す
        public override ICell Clone(bool priority, bool state1, bool state2)
        {
            return new ScoreCell(Score)
            {
                State1 = state1 ? State1 : CellState.None,
                State2 = state2 ? State2 : CellState.None,
                Priority = priority ? Priority : CellPriority.None,
                Score = Score
            };
        }
    }
}
