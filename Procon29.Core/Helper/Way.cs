using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Helper
{
    /// <summary>
    /// ある点からある点までのルートの移動を管理するクラス
    /// </summary>
    public class Way
    {
        /// <summary>
        /// <paramref name="from"/>から<paramref name="wayPoints"/>を経由して移動するクラスを生成します。経由点は自動で補完されます
        /// </summary>
        /// <param name="from">移動を開始する位置</param>
        /// <param name="wayPoints">経由する位置</param>
        public Way(Point from, params Point[] wayPoints)
        {
            From = from;
            WayPoints = wayPoints.ToList();

            List<Point> points = new List<Point>();
            List<Direction> directions = new List<Direction>();
            var move = from.Way(wayPoints);

            foreach (var m in move)
            {
                points.Add(m.Item1);
                directions.Add(m.Item2);
            }

            actualPoints = points;
            actualDirections = directions;
        }

        /// <summary>
        /// <paramref name="from"/>から<paramref name ="directions"/>に従って移動するクラスを生成します。経由点は自動で補完されます
        /// </summary>
        /// <param name="from">移動を開始する位置</param>
        /// <param name="directions">移動する方向</param>
        public Way(Point from, params Direction[] directions)
        {
            Point p = new Point(from);
            From = from;
            WayPoints = directions.Select((d) =>
            {
                p = p.Move(d);
                return p;
            }).ToList();

            actualPoints = WayPoints.ToList();
            actualDirections = directions.ToList();
        }

        /// <summary>
        /// <paramref name="agent"/>の位置から<paramref name="wayPoints"/>を経由して移動するクラスを生成します。経由点は自動で補完されます
        /// </summary>
        /// <param name="agent">移動を開始するエージェント</param>
        /// <param name="wayPoints">経由する位置</param>
        public Way(Agent agent, params Point[] wayPoints) : this(agent.Position, wayPoints)
        {
        }

        /// <summary>
        /// <paramref name="agent"/>から<paramref name ="directions"/>に従って移動するクラスを生成します。経由点は自動で補完されます
        /// </summary>
        /// <param name="agent">移動を開始するエージェント</param>
        /// <param name="directions">移動する方向</param>
        public Way(Agent agent, params Direction[] directions) : this(agent.Position, directions)
        {
        }

        /// <summary>
        /// 0から始まる。現在が移動の何ターン目かを表す
        /// </summary>
        public int Turn { get; private set; }

        /// <summary>
        /// 移動を開始する位置
        /// </summary>
        public Point From { get; }

        /// <summary>
        /// 経由する位置
        /// </summary>
        public List<Point> WayPoints { get; }

        /// <summary>
        /// 現在地
        /// </summary>
        public Point Current => WayPoints[Turn];

        private List<Point> actualPoints { get; set; }

        private List<Direction> actualDirections { get; set; }

        /// <summary>
        /// 終点まで実際に移動する位置（始点は含みません）
        /// </summary>
        public IReadOnlyList<Point> MovePoints => actualPoints;

        /// <summary>
        /// 終点まで実際に移動する方向（始点は含みません）
        /// </summary>
        public IReadOnlyList<Direction> MoveDirections => actualDirections;

        /// <summary>
        /// 途中で別ルートを経由したい場合に呼び出してください。指定されたデリゲートからルートを計算します
        /// </summary>
        public void Detour(Func<Point[]> detour)
        {
            var points = detour?.Invoke();

            if (points != null && points.Length > 1)
            {
                var from = points[0];
                var way = from.Way(points);

                actualPoints.RemoveRange(Turn, actualPoints.Count - Turn - 1);
                actualDirections.RemoveRange(Turn, actualDirections.Count - Turn - 1);

                actualPoints.AddRange(way.Select(p => p.Item1));
                actualDirections.AddRange(way.Select(p => p.Item2));
            }
        }

        /// <summary>
        /// 次のターンへ移行し、移動先を取得します
        /// </summary>
        /// <returns></returns>
        public bool Next(out Point nextPosition, out Direction direction)
        {
            var res = Peep(out nextPosition, out direction);
            if (res) Turn++;
            return res;
        }

        /// <summary>
        /// 次のターンで行く場所を取得します
        /// </summary>
        /// <param name="nextPosition"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Peep(out Point nextPosition, out Direction direction)
        {
            if (Turn < MovePoints.Count)
            {
                nextPosition = MovePoints[Turn];
                direction = MoveDirections[Turn];
                return true;
            }
            else
            {
                nextPosition = new Point();
                direction = Direction.None;
                return false;
            }
        }
    }

    //複数のWayクラスを同時に操作する管理クラス
    public class WayManager
    {
        public WayManager(List<Way> ways)
        {
            Ways = ways;
        }

        public WayManager(params Way[] ways) : this(ways.ToList()) { }

        public event EventHandler Finished;

        public bool IsFinished => Ways.Count == 0;

        private List<Way> Ways { get; }

        public void Next(Action<Way, Point, Direction> callback)
        {
            for (int i = 0; Ways.Count > i; i++)
            {
                var w = Ways[i];
                if (w.Next(out var p, out var d))
                {
                    callback(w, p, d);
                }
                else
                {
                    Ways.Remove(w);
                    i--;
                }
            }

            if (Ways.Count == 0) Finished?.Invoke(this, new EventArgs());
        }
    }
}
