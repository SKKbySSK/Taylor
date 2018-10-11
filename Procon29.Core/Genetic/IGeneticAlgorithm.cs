using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Genetic
{
    /// <summary>
    /// 遺伝的アルゴリズムに使用されるインターフェース
    /// </summary>
    public interface IGeneticAlgorithm
    {
        /// <summary>
        /// 指定された情報に基づいて得点能率を全て計算して返却します
        /// </summary>
        /// <param name="field">フィールド情報</param>
        /// <param name="team">味方チームフラグ</param>
        /// <param name="enemy">敵チームフラグ</param>
        /// <returns>得点能率の計算結果</returns>
        ScoringEfficiency[] CalculateEfficiencies(Field field, Teams team, Teams enemy);
    }
}
