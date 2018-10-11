using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon29.Core.Extensions;

namespace Procon29.Core.Genetic
{
    /// <summary>
    /// 得点能率を格納するクラス
    /// </summary>
    public class ScoringEfficiency
    {
        /// <summary>
        /// 得点能率を計算してクラスを初期化します
        /// </summary>
        /// <param name="diffScore">自チームと敵チームの点差</param>
        /// <param name="turns">予想されるターン数</param>
        public ScoringEfficiency(double diffScore, int turns)
        {
            Efficiency = diffScore/ turns;
            ExpectedTurns = turns;
        }

        public ScoringEfficiency(Field field, Teams team, int turns) : this(field.GetScore(team) - field.GetScore(team.GetEnemyTeam()), turns)
        {
        }

        /// <summary>
        /// 得点能率
        /// </summary>
        public double Efficiency { get; }

        /// <summary>
        /// 最小限の必要ターン数
        /// </summary>
        public int ExpectedTurns { get; }
    }
}
