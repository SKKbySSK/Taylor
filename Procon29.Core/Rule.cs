using System;
using System.Collections.Generic;
using System.Text;

namespace Procon29.Core
{
    public static class Rule
    {
        /// <summary>
        /// 1マスの実寸台の横幅をcmで定義します
        /// </summary>
        public const int RealCellWidth = 50;

        /// <summary>
        /// 1マスの実寸台の縦幅をcmで定義します
        /// </summary>
        public const int RealCellHeight = 50;

        /// <summary>
        /// セルに付与される最大の点数
        /// </summary>
        public const int MaximumCellScore = 16;

        /// <summary>
        /// セルに付与される最小の点数
        /// </summary>
        public const int MinimumCellScore = -16;

        /// <summary>
        /// 1試合の最小ターン数です
        /// </summary>
        public const int MinimumTurns = 60;

        /// <summary>
        /// 1試合の最大ターン数です
        /// </summary>
        public const int MaximumTurns = 120;

        public static bool IsCorrectCell(ScoreCell cell)
        {
            return MaximumCellScore >= cell.Score && MinimumCellScore <= cell.Score;
        }
    }
}
