using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Taku.ClassExtensions
{
    static class PointExtension
    {
        public static Point FastMove(this Point p, Direction direction)
        {
            switch (direction)
            {
                case Direction.Left | Direction.Down:
                    return new Point(p.X-1, p.Y+1);
                case Direction.Down:
                    return new Point(p.X, p.Y+1);
                case Direction.Right | Direction.Down:
                    return new Point(p.X+1, p.Y+1);
                case Direction.Left:
                    return new Point(p.X-1, p.Y);
                case Direction.None:
                    return new Point(p.X, p.Y);
                case Direction.Right:
                    return new Point(p.X+1, p.Y);
                case Direction.Left | Direction.Up:
                    return new Point(p.X-1, p.Y-1);
                case Direction.Up:
                    return new Point(p.X, p.Y-1);
                case Direction.Right | Direction.Up:
                    return new Point(p.X+1, p.Y-1);
                default:
                    return new Point(p.X, p.Y);
            }
        }
        /// <summary>
        /// 内積を計算
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int Dot(this Point p1,Point p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
    }
}
