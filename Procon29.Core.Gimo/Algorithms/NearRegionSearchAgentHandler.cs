using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon29.Core;
using Procon29.Core.Extensions;
using Procon29.Core.Genetic;

namespace Procon29.Core.Gimo.Algorithms
{
    public class NearRegionSearchAgentHandler : AgentAlgorithmBase
    {
        //コンストラクタ
        public NearRegionSearchAgentHandler(Game game, Agent agent, int around = 4) : base(game, agent)
        {
            Around = around;
        }

        private Direction[] SearchableDirections { get; set; } = Direction.All.ParseFlags(true).ToArray();

        private Helper.Way Way { get; set; }

        private NearRegionResult RegionResult { get; set; }

        //TODO 注意:LoadDumperはロックを行うため処理速度低下を防ぐため本番では無効化するように！
        private Helper.LoadDumper Dumper { get; } //= new Helper.LoadDumper();

        public int Around { get; set; } = 4;

        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            return Search(prohibitedPoints);
        }

        public override Intent Search(params Point[] prohibitedPoints)
        {
            EvaluateGame();

            if (RegionResult == null || RegionResult.Ends.Count == 0)
                return Intent.StayIntent;

            if (Way.Peep(out var pos, out var dir))
            {
                if (Game.Field.GetCell(pos).GetState(EnemyEnum) == CellState.Occupied)
                {
                    return new Intent() { Intention = Intentions.Remove, Direction = dir };
                }
                else
                {
                    Way.Next(out _, out _);
                    return new Intent() { Intention = Intentions.Move, Direction = dir };
                }
            }
            else
            {
                RegionResult = null;
                return Search(prohibitedPoints);
            }
        }

        protected override ScoringEfficiency EvaluateGame()
        {
            if (RegionResult == null)
            {
                RegionResult = Search(Agent.Position, Math.Min(Around, Game.Length - Game.Turn + 1));
                Dumper?.Dump();
                Dumper?.Reset();

                if (RegionResult.Ends.Count > 0)
                {
                    Way = new Helper.Way(Agent, RegionResult.Ends.First().Went.ToArray());
                }
                else
                {
                    Console.WriteLine("近傍領域探索:領域を生成できませんでした");
                    return null;
                }
            }

            return RegionResult.Ends.FirstOrDefault()?.GetScoringEfficiency();
        }
        
        private NearRegionPoint SearchRoot(Point from, int around = 4, int current = 0, NearRegionPoint parentResult = null)
        {
            Dumper?.Record("Check Movable");
            if (parentResult == null)
            {
                parentResult = new NearRegionPoint()
                {
                    To = from,
                    MovedDirection = Direction.None,
                    Went = new List<Point>(new Point[] { from }),
                    Max = from,
                    Min = from
                };
            }

            if (current++ < around)
            {
                var reversed = parentResult.MovedDirection.Reverse();

                var movable = SearchableDirections.Where(d =>
                {
                    if (d != reversed)
                    {
                        var to = parentResult.To.Move(d);
                        return Game.Field.IsInField(parentResult.To.Move(d)) && !parentResult.Went.Contains(to);
                    }
                    return false;

                }).Select((d, i) => (d, i)).ToArray();

                var children = new NearRegionPoint[movable.Length];

                Parallel.ForEach(movable, d =>
                {
                    var to = parentResult.To.Move(d.d);
                    var result = new NearRegionPoint()
                    {
                        From = parentResult.To,
                        To = to,
                        MovedDirection = d.d,
                        Went = parentResult.Went.ToList(),
                        Parent = parentResult,
                        Max = parentResult.Max,
                        Min = parentResult.Min
                    };

                    //Maxを更新
                    if (to.X > parentResult.Max.X)
                        result.Max = new Point(to.X, result.Max.Y);

                    if (to.Y > parentResult.Max.Y)
                        result.Max = new Point(result.Max.X, to.Y);

                    //Minを更新
                    if (to.X < parentResult.Min.X)
                        result.Min = new Point(to.X, result.Min.Y);

                    if (to.Y < parentResult.Min.Y)
                        result.Min = new Point(result.Min.X, to.Y);

                    result.Went.Add(to);
                    children[d.i] = result;
                    
                    SearchRoot(from, around, current, result);
                });

                foreach (var c in children) parentResult.Children.Add(c);
            }

            return parentResult;
        }

        private NearRegionResult Search(Point from, int around = 4)
        {
            var root = SearchRoot(from, around, 0, null);

            List<NearRegionPoint> ends = new List<NearRegionPoint>();

            GetEnds(root);
            void GetEnds(NearRegionPoint parent)
            {
                if (parent.Children.Count == 0 && parent.Went.Count > Around)
                {
                    ends.Add(parent);
                }
                else
                {
                    foreach (var p in parent.Children)
                        GetEnds(p);
                }
            }

            var backup = Game.Field.Clone(false, true, true);
            var cloned = Game.Field.Clone(false, true, true);
            cloned.AutoDump = false;
            cloned.AutoEvaluate = false;

            var enemyScore = cloned.EvaluateMap(EnemyEnum);

            foreach (var e in ends)
            {
                Dumper?.Record("Set Cell State");
                cloned.Map.ForEach((p, c) =>
                {
                    c.State1 = backup.GetCell(p).State1;
                    c.State2 = backup.GetCell(p).State2;
                });

                foreach (var w in e.Went)
                    cloned.GetCell(w).SetState(TeamEnum, CellState.Occupied);

                Dumper?.Record("Evaluate Score");

                e.TeamScore = cloned.EvaluateMap(TeamEnum);
                e.EnemyScore = enemyScore;

                Dumper?.Stop();
            }

            return new NearRegionResult(root, ends.OrderByDescending(p => p.GetScoringEfficiency().Efficiency).ToList());
        }

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }
    }
}
