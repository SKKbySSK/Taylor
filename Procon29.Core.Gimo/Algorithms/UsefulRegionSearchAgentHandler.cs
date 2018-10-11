using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon29.Core.Extensions;
using Procon29.Core.Genetic;

namespace Procon29.Core.Gimo.Algorithms
{
    class UsefulRegionSearchAgentHandler : AgentAlgorithmBase
    {
        private bool[][][] Map { get; set; }

        private int MaximumScore { get; set; }

        private Point FillPoint { get; set; }

        public Direction[] SearchableDirections { get; set; } = Direction.All.ParseFlags(false).ToArray();

        public bool AutoDump { get; set; } = true;

        public UsefulRegionEfficiency CurrentEffic { get; set; }

        //コンストラクタ
        public UsefulRegionSearchAgentHandler(Game game, Agent agent) : base(game, agent)
        {
        }

        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            return Search(prohibitedPoints);
        }

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }

        public override Intent Search(params Point[] prohibitedPoints)
        {
            EvaluateGame();

            if (CurrentEffic == null)
                return Intent.StayIntent;

            if (CurrentEffic.Way.Peep(out var pos, out var dir))
            {
                if (Game.Field.GetCell(pos).GetState(EnemyEnum) == CellState.Occupied)
                {
                    return new Intent() { Intention = Intentions.Remove, Direction = dir };
                }
                else
                {
                    CurrentEffic.Way.Next(out _, out _);
                    return new Intent() { Intention = Intentions.Move, Direction = dir };
                }
            }
            else
            {
                CurrentEffic = null;
                return Search(prohibitedPoints);
            }
        }

        protected override ScoringEfficiency EvaluateGame()
        {
            if (CurrentEffic == null)
            {
                CurrentEffic = CalculateMaxEfficiency();

                if (CurrentEffic == null)
                {
                    Console.WriteLine("有用領域探索:領域を生成できませんでした");
                    return null;
                }
            }

            return CurrentEffic.Efficiency;
        }

        /// <summary>
        /// 最大となる得点能率を計算する関数
        /// </summary>
        /// <returns></returns>
        private UsefulRegionEfficiency CalculateMaxEfficiency()
        {
            MaximumScore = Game.Field.Map.Cast<ICell>().Max(c => Math.Abs(c.Score));
            Console.WriteLine("Maximum Score : " + MaximumScore);
            Map = new bool[MaximumScore][][];

            for (int k = 0; MaximumScore > k; k++)
            {
                //Step1.2
                Map[k] = new bool[Game.Field.Height][];
                for (int j = 0; Game.Field.Height > j; j++)
                {
                    Map[k][j] = new bool[Game.Field.Width];
                    for (int i = 0; Game.Field.Width > i; i++)
                    {
                        var absScore = Math.Abs(Game.Field.GetCell(new Point(i, j)).Score);
                        bool abs = absScore <= k && !Map[k].IsEdge(i, j);
                        Map[k][j][i] = abs;

                        if (abs) FillPoint = new Point(i, j);
                    }
                }
            }

            //得点能率
            UsefulRegionEfficiency efficiency = null;
            int ek = 0;

            //フィールドをクローン
            var cloned = Game.Field.Clone(false, false, false);
            cloned.AutoDump = false;
            cloned.AutoEvaluate = false;

            //Step1.3-1.7
            for (int k = MaximumScore - 1; k > -1; k--)
            {
                var effcs = CalculateScoreEfficiencies(cloned, Map[k]);
                var eff = effcs.OrderBy(e => e.Efficiency).LastOrDefault();

                if (eff != null)
                {
                    if (efficiency == null || eff.Efficiency.Efficiency > efficiency.Efficiency.Efficiency)
                    {
                        efficiency = eff;
                        ek = k;
                    }

                    foreach (var c in cloned.Map)
                    {
                        c.State1 = CellState.None;
                        c.State2 = CellState.None;
                        c.Priority = CellPriority.None;
                    }
                }
            }

            if (AutoDump)
            {
                DumpEfficiency(ek, efficiency);
            }

            return efficiency;
        }

        private void DumpEfficiency(int k, UsefulRegionEfficiency eff)
        {
            Console.WriteLine($"Score <= {k} : {eff.Efficiency.Efficiency}");

            if (eff.Region != null && eff.Occupy != null)
            {
                eff.Region.ForEach((p, c) =>
                {
                    var o = eff.Occupy[p.Y, p.X];

                    if (c != null)
                    {
                        Console.Write("＊");
                    }
                    else if (o != null)
                    {
                        Console.Write("○");
                    }
                    else
                    {
                        Console.Write("―");
                    }
                }, i => Console.WriteLine());
            }

            Console.WriteLine();
        }

        /// <summary>
        /// 得点能率を計算する関数。エージェントからの移動距離は計算されますが、得点能率には加算しません
        /// </summary>
        /// <param name="field"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private List<UsefulRegionEfficiency> CalculateScoreEfficiencies(Field field, bool[][] map)
        {
            List<ICell[,]> occups = new List<ICell[,]>();
            List<ICell[,]> regions = new List<ICell[,]>();
            List<UsefulRegionDestination> destinations = new List<UsefulRegionDestination>();
            
            var directions8 = Direction.All.ParseFlags(true).ToArray();

            var conv = map.Convert();
            map.ForEach((p, b) =>
            {
                ICell[,] regionCells = null;

                if (b)
                {
                    /*領域の作成*/
                    var cell = field.GetCell(p);

                    //このとき領域セルの上下左右は少なくとも領域内となるのでチェックは省略する
                    regionCells = new ICell[field.Height, field.Width];

                    //領域となるセルをコピーする
                    map.ForEach((p2, b2) =>
                    {
                        if (b2)
                        {
                            map[p2.Y][p2.X] = false;
                            regionCells[p2.Y, p2.X] = field.Map[p2.Y, p2.X];
                        }
                    });

                    regions.Add(regionCells);

                    /*占有セルの作成*/

                    //エージェントからの最短地点を格納する
                    UsefulRegionDestination dest = null;

                    //領域セルの外側を塗りつぶす
                    var nonRegion = Helper.FieldUtils.Fill(field.Map, new Point(0, 0), (_, p2) => regionCells[p2.Y, p2.X] == null, directions8);

                    //領域と隣接するセルのみを取り出して占有セルとする
                    ICell[,] occup = new ICell[field.Height, field.Width];

                    //独立した領域となっているセルを格納する
                    List<ICell[,]> smallOccups = new List<ICell[,]>();

                    nonRegion.ForEach((p2, c) =>
                    {
                        if (occup[p2.Y, p2.X] == null && nonRegion[p2.Y, p2.X] != null && OnRegion(field.GetPoints(p2, SearchableDirections)))
                        {
                            var cells = Helper.FieldUtils.Fill(nonRegion, p2, (_, p3) => OnRegion(field.GetPoints(p3, SearchableDirections)), directions8);
                            smallOccups.Add(cells);

                            cells.ForEach((p3, c2) =>
                            {
                                if (c2 != null)
                                {
                                    occup[p3.Y, p3.X] = c2;

                                    //ついでにその位置から各エージェントの距離を調べて短ければ記録
                                    var len = p3.Length(Agent.Position);
                                    if (dest == null || dest.Length < len)
                                    {
                                        if (dest == null)
                                        {
                                            dest = new UsefulRegionDestination();
                                            dest.Agent = Team.Agent1;
                                        }

                                        dest.Length = len;
                                        dest.Point = p3;
                                        dest.Cell = c2;
                                    }
                                }
                            });
                        }
                    });

                    //独立した領域を最短距離で接続
                    for (int i = 0; smallOccups.Count > i; i++)
                    {
                        var oc = smallOccups[i];
                        for (int j = 0; smallOccups.Count > j; j++)
                        {
                            if (i != j)
                            {
                                var points = GetMinimumPoints(oc, smallOccups[j]);
                                var way = points.Item1.Way(points.Item2);

                                foreach (var point in way)
                                    occup[point.Item1.Y, point.Item1.X] = field.GetCell(point.Item1);
                                occup[points.Item1.Y, points.Item1.X] = field.GetCell(points.Item1);

                                smallOccups.RemoveAt(j--);
                            }
                        }
                    }

                    occups.Add(occup);
                    destinations.Add(dest);
                }

                bool OnRegion(IEnumerable<Point> points)
                {
                    var cells = points.Select(p3 => regionCells[p3.Y, p3.X]);
                    var onRegion = cells.Where(c => c != null).Count() > 0;
                    return onRegion;
                }
            });

            //得点能率の大きいものを格納
            List<UsefulRegionEfficiency> effics = new List<UsefulRegionEfficiency>();

            foreach (var pair in regions.Zip(occups, (r, o) => (r, o)).Zip(destinations, (r, d) => (r, d)))
            {
                var e = new UsefulRegionEfficiency();
                var region = pair.r.r.Cast<ICell>().Where(c => c != null).Sum(c => Math.Abs(c.Score));
                var occup = pair.r.o.Cast<ICell>().Where(c => c != null).Sum(c => c.Score);
                
                e.Occupy = pair.r.o;
                e.Region = pair.r.r;
                e.Destination = pair.d;
                e.Way = CreateWay(e.Occupy, e.Destination);

                var f = Game.Field.Clone(false, true, true);
                e.Occupy.ForEach((p, c) => 
                {
                    if (c != null)
                        f.GetCell(p).SetState(TeamEnum, CellState.Occupied);
                });

                e.Efficiency = new ScoringEfficiency(f, TeamEnum, e.Way.MovePoints.Count);

                effics.Add(e);
            }

            return effics;
        }

        private static (Point, Point) GetMinimumPoints(ICell[,] o1, ICell[,] o2)
        {
            double length = double.MaxValue;
            Point from = new Point(), to = new Point();

            o1.ForEach((point1, c1) =>
            {
                if (c1 != null)
                {
                    o2.ForEach((point2, c2) =>
                    {
                        if (c2 != null)
                        {
                            var len = point2.Length(point1);
                            if (len < length)
                            {
                                from = point1;
                                to = point2;
                            }
                        }
                    });
                }
            });

            return (from, to);
        }

        private static void Simplify(Field field, Direction[] directions4, ICell[,] regionCells, ICell[,] occup)
        {
            occup.ForEach((center, oc) =>
            {
                if (oc != null)
                {
                    var enclosed = field.GetPoints(center, directions4);
                    var cells = enclosed.Select(p => occup[p.Y, p.X] ?? regionCells[p.Y, p.X]).ToList();

                    if (!cells.Contains(null))
                    {
                        occup[center.Y, center.X] = null;
                        regionCells[center.Y, center.X] = oc;
                    }
                }
            });
        }
        
        private static Helper.Way CreateWay(ICell[,] occupy, UsefulRegionDestination destination)
        {
            List<Point> points = new List<Point>();

            points.Add(destination.Point);

            Helper.FieldUtils.Fill(occupy, destination.Point, (c2, p2) =>
            {
                if (c2 != null)
                {
                    if (!points.Contains(p2))
                        points.Add(p2);
                    return true;
                }

                return false;
            }, Direction.All.ParseFlags(true));

            return new Helper.Way(destination.Agent, points.ToArray());
        }
    }
}
