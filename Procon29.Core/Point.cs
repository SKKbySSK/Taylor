using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon29.Core
{
    /// <summary>
    /// 座標を表す構造体（+, -, *の演算子が使えます）
    /// </summary>
    public struct Point
    {
        //コンストラクタ
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        //コンストラクタのオーバーロード
        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
        public int X { get; set; }

        public int Y { get; set; }

        public Point AddX(int add)
        {
            return new Point(X + add, Y);
        }

        public Point AddY(int add)
        {
            return new Point(X, Y + add);
        }

        //演算子のオーバーロードたち
        public static bool operator ==(Point position1, Point position2)
        {
            return position1.X == position2.X && position1.Y == position2.Y;
        }

        public static bool operator !=(Point position1, Point position2)
        {
            return !(position1 == position2);
        }

        public static Point operator +(Point position1, Point position2)
        {
            return new Point(position1.X + position2.X, position1.Y + position2.Y);
        }

        public static Point operator -(Point position1, Point position2)
        {
            return new Point(position1.X - position2.X, position1.Y - position2.Y);
        }

        public static Point operator *(Point position1, Point position2)
        {
            return new Point(position1.X * position2.X, position1.Y * position2.Y);
        }
        //実数倍の定義
        public static Point operator *(int i,Point point)
        {
            return new Point(point.X * i, point.Y * i);
        }
        //実数倍の定義
        public static Point operator *(Point point,int i)
        {
            return new Point(point.X * i, point.Y * i);
        }
        //割り算の定義
        public static Point operator /(Point point, int i)
        {
            return new Point(point.X / i, point.Y / i);
        }

        /// <summary>
        /// <see cref="X"/>と<see cref="Y"/>の値から三平方の定理を用いて長さを求めます
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// <paramref name="point"/>と現在のの<see cref="X"/>と<see cref="Y"/>の値から三平方の定理を用いて2点間の距離を求めます
        /// </summary>
        /// <returns></returns>
        public double Length(Point point)
        {
            return (this - point).Length();
        }

        /// <summary>
        /// <paramref name="direction"/>方向に<paramref name="steps"/>移動します
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Point Move(Direction direction, int steps = 1)
        {
            Point point = new Point(X, Y);

            foreach (Direction d in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                if (direction.HasFlag(d))
                {
                    switch (d)
                    {
                        case Direction.Left:
                            point = point.AddX(-steps);
                            break;
                        case Direction.Up:
                            point = point.AddY(-steps);
                            break;
                        case Direction.Right:
                            point = point.AddX(steps);
                            break;
                        case Direction.Down:
                            point = point.AddY(steps);
                            break;
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// 現在位置を含まず<paramref name="wayPoints"/>を経由して移動するルートを計算します
        /// </summary>
        /// <param name="wayPoints">経由する位置</param>
        public List<(Point, Direction)> Way(params Point[] wayPoints)
        {
            List<(Point, Direction)> move = new List<(Point, Direction)>();

            Point last = this;
            foreach (var p in wayPoints)
            {
                var delta = p - last;
                int moveX = Math.Abs(delta.X), moveY = Math.Abs(delta.Y);
                int count = Math.Max(moveX, moveY);

                for (int i = 0; count > i; i++)
                {
                    Direction direction = Direction.None;

                    if (moveX-- > 0)
                    {
                        if (delta.X > 0)
                        {
                            direction |= Direction.Right;
                        }
                        else if (delta.X < 0)
                        {
                            direction |= Direction.Left;
                        }
                    }

                    if (moveY-- > 0)
                    {
                        if (delta.Y > 0)
                        {
                            direction |= Direction.Down;
                        }
                        else if (delta.Y < 0)
                        {
                            direction |= Direction.Up;
                        }
                    }

                    last = last.Move(direction);
                    move.Add((last, direction));
                }

            }

            return move;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Point p)
            {
                return X == p.X && Y == p.Y;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
