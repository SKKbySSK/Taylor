using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon29.Core.Extensions
{
    public static class DirectionExtension
    {
        /// <summary>
        /// 複数の方向で構成された<see cref="Direction"/>を1つ1つの方向に分解しリストへ格納して返却します
        /// </summary>
        /// <param name="directions"></param>
        /// <param name="eightDirections">8方向に分解するかを指定します。trueの場合、斜め方向のフラグも返却されます</param>
        /// <returns></returns>
        public static List<Direction> ParseFlags(this Direction directions, bool eightDirections)
        {
            List<Direction> ds = new List<Direction>();
            var directs = Enum.GetValues(typeof(Direction)).Cast<Direction>();

            foreach (var d in directs)
            {
                if (d != Direction.None && d != Direction.All)
                    AddIf(d);
            }

            if (eightDirections)
            {
                AddIf(Direction.Left | Direction.Up);
                AddIf(Direction.Left | Direction.Down);
                AddIf(Direction.Right | Direction.Up);
                AddIf(Direction.Right | Direction.Down);
            }

            void AddIf(Direction direction)
            {
                if (directions.HasFlag(direction))
                    ds.Add(direction);
            }

            return ds;
        }

        /// <summary>
        /// 与えられた<see cref="Direction"/>を反転します
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Direction Reverse(this Direction direction)
        {
            Direction ret = Direction.None;
            ChangeDirection(Direction.Down, Direction.Up, direction, ref ret);
            ChangeDirection(Direction.Up, Direction.Down, direction, ref ret);
            ChangeDirection(Direction.Right, Direction.Left, direction, ref ret);
            ChangeDirection(Direction.Left, Direction.Right, direction, ref ret);

            void ChangeDirection(Direction from, Direction to, Direction compare, ref Direction operate)
            {
                if (compare.HasFlag(from))
                {
                    operate &= ~from;
                    operate |= to;
                }
            }

            return ret;
        }
    }
}
