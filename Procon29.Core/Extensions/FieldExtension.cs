using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon29.Core.Extensions
{
    public static class FieldExtension
    {
        /// <summary>
        /// 指定した位置がフィールド内であるかを判定します
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInField(this Field field, Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= field.Width || point.Y >= field.Height)
                return false;

            return true;
        }

        /// <summary>
        /// 指定したエージェント方向がフィールド内であるかを判定します
        /// </summary>
        /// <param name="field"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsInField(this Field field, Agent agent, Direction direction)
        {
            return IsInField(field, agent.Position.Move(direction));
        }

        /// <summary>
        /// 指定した位置を中心として各方向にある移動可能なセルの位置を返却します
        /// </summary>
        /// <param name="field"></param>
        /// <param name="point"></param>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static IEnumerable<Point> GetPoints(this Field field, Point point, params Direction[] directions)
        {
            return directions.Select(d => point.Move(d)).Where(p => IsInField(field, p));
        }

        /// <summary>
        /// 指定した位置を中心として各方向にある移動可能なセルを返却します
        /// </summary>
        /// <param name="field"></param>
        /// <param name="point"></param>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static IEnumerable<ICell> GetCells(this Field field, Point point, params Direction[] directions)
        {
            return GetPoints(field, point, directions).Select(p => field.GetCell(p));
        }

        /// <summary>
        /// 指定した位置を中心として各方向にある移動可能なセルをフィールドと同じ大きさの2次元配列に投影します
        /// </summary>
        /// <param name="field"></param>
        /// <param name="point"></param>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static ICell[,] GetMappedCells(this Field field, Point point, params Direction[] directions)
        {
            ICell[,] cells = new ICell[field.Height, field.Width];

            foreach (var p in GetPoints(field, point, directions))
                cells[p.Y, p.X] = field.GetCell(p);

            return cells;
        }

        public static int GetScore(this Field field, Teams team)
        {
            if (team == Teams.Team1)
                return field.Score1;

            if (team == Teams.Team2)
                return field.Score2;

            throw new InvalidOperationException("片方のチームを指定してください");
        }
    }
}
