using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Algorithms;
using Procon29.Core.Extensions;
using Procon29.Core.Genetic;
using Procon29.Core.Helper;
using Procon29.Core.Taku.ClassExtensions;

namespace Procon29.Core.Taku.Algorithms
{
    /// <summary>
    /// 現在の盤面だけで得点が最も獲得できそうな経路を求めるアルゴリズム。
    /// それは、エージェントのいる場所からマップ上の各セルに移動する場合の最も占有点が大きくなる経路の中で、
    /// 最も能率（＝（占有点の差＋領域点の差）/必要ターン数）が大きくなるものを
    /// 擬似的な最善手とするというアルゴリズムである。
    /// </summary>
    class BetterRoutesSearchAgentHandler : AgentAlgorithmBase
    {
        private Way Way { get; set; }

        private AStarSearch AStarSearch { get; set; }

        private Node[] BetterRoute { get; set; }
        private int NeedTurn { get; set; }
        private double BetterEfficiency { get; set; }


        private readonly double averageScore;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="game"></param>
        /// <param name="agent"></param>
        public BetterRoutesSearchAgentHandler(Game game, Agent agent) : base(game, agent)
        {
            BetterRoute = new Node[1];
            BetterRoute[0] = new Node();

            Way = new Way(Agent, Direction.None);

            //AStarSearchの準備
            AStarSearch = new AStarSearch(Game.Field.Width, Game.Field.Height);

            //ノードを作成
            Node[,] nodes = new Node[Game.Field.Height, Game.Field.Width];

            //ノードにデータを入れる＋マップの得点の平均値を求める
            for (int i = 0; i < Game.Field.Width; i++)
            {
                for (int j = 0; j < Game.Field.Height; j++)
                {
                    //得点の総計
                    averageScore += Game.Field.GetCell(i, j).Score;
                    nodes[j, i] = new Node();
                }
            }
            //ノードをセット
            AStarSearch.Nodes = nodes;
            //平均化
            averageScore /= Game.Field.Width * Game.Field.Height;
        }

        protected override ScoringEfficiency EvaluateGame()
        {
            //移動によるスコアを計算
            if (BetterRoute != null)
            {
                int nowIndex = BetterRoute.ToList().FindIndex((n) => n.Point == Agent.Position);
                int beforeTeam, beforeEnemy, nextTeam, nextEnemy;
                int needTurn = 0;
                double delta = 0.0;
                //移動させて推定得点を出すために、フィールドを複製
                Field field = Game.Field.Clone(true, true, true);
                //敵の存在する確率を求めるクラスを保持
                var calculateExistenceProbability = new CalculateExistenceProbability(1);

                //初期値を設定
                beforeTeam = field.EvaluateMap(TeamEnum);
                beforeEnemy = field.EvaluateMap(EnemyEnum);


                //一つづつ移動してみて、推定能率を計算
                foreach (var node1 in BetterRoute.ToList().Skip(nowIndex))
                {
                    //ターン追加
                    needTurn++;
                    //自チームにタイルを置く
                    field.Map[node1.Point.Y, node1.Point.X].SetState(TeamEnum, CellState.Occupied);
                    //相手に占有されていたらその分ターンが必要なので
                    if (field.Map[node1.Point.Y, node1.Point.X].GetState(EnemyEnum) == CellState.Occupied)
                        needTurn++;

                    field.Map[node1.Point.Y, node1.Point.X].SetState(EnemyEnum, CellState.None);

                    //値の変化を見る
                    nextTeam = field.GetScore(TeamEnum);
                    nextEnemy = field.GetScore(EnemyEnum);
                    delta += ((nextTeam - beforeTeam) - (nextEnemy - beforeEnemy))
                        * (1 - calculateExistenceProbability.MapPossibilities(Enemy.Agent1.Position, Enemy.Agent2.Position, field.Width, field.Height, needTurn)[node1.Point.Y, node1.Point.X]);

                    beforeTeam = nextTeam;
                    beforeEnemy = nextEnemy;
                }

                //得点を計算
                return new ScoringEfficiency(delta, needTurn);
            }

            //経路がない場合
            return new ScoringEfficiency(0, 0);
        }

        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            Intent Intent = new Intent()
            {
                Intention = Intentions.Stay,
                Direction = Direction.None
            };

            Point agentNextPos = new Point();
            Direction direction = Direction.None;

            //すべての移動を終えた
            if (!Way.Peep(out agentNextPos, out direction) || prohibitedPoints.Any(p => p == agentNextPos))
            {
                //新たに探索
                Search(prohibitedPoints);
            }

            //正式に経路を取得
            Way.Next(out agentNextPos, out direction);

            return Game.GetIntent(Agent, direction);
        }

        public override Intent Search(params Point[] prohibitedPoints)
        {
            //問題は、二回目以降の探索を行うときに起こる

            //Agent agent = new Agent();
            Field field = Game.Field.Clone(true, true, true);

            int startTeam, startEnemy, beforeTeam, beforeEnemy, nextTeam, nextEnemy;
            int needTurn;
            double delta;

            startTeam = field.EvaluateMap(TeamEnum);
            startEnemy = field.EvaluateMap(EnemyEnum);

            //良いとされるルートを保持
            Node[] betterRoute = new Node[1];

            //敵の存在する確率を求めるクラスを保持
            var calculateExistenceProbability = new CalculateExistenceProbability(1);
            //Listで敵エージェントがそこに存在する確率を保持することで、オーバーヘッドを軽減
            List<double[,]> agentsProbabilities = new List<double[,]>();


            BetterRoute = new Node[1];
            BetterRoute[0] = new Node();

            BetterEfficiency = double.MinValue;

            AStarSearch.Nodes.ForEach((r, n) =>
            {
                //エージェントの位置以外探索
                //おもすぎるので、相対位置が--の場合に変えるかも？

                if (n.Point != Agent.Position)
                {
                    AStarSearch.ResetResult();
                    AStarSearch.StartNode = AStarSearch.Nodes[Agent.Position.Y, Agent.Position.X];
                    AStarSearch.EndNodes = new Node[] { n };

                    //禁止領域は予めクローズすることで対応
                    foreach (var p in prohibitedPoints)
                    {
                        AStarSearch.Nodes[p.Y, p.X].Status = NodeStatus.Closed;
                    }


                    //探索開始
                    AStarSearch.Search((c) =>
                    {
                        Point p = c.Point;
                        //まだ敵エージェントが存在する確率を計算していない
                        if (c.Turn >= agentsProbabilities.Count)
                        {
                            agentsProbabilities.Add(
                            calculateExistenceProbability.MapPossibilities(Enemy.Agent1.Position, Enemy.Agent2.Position, Game.Field.Width, Game.Field.Height, c.Turn)
                            );
                        }

                        //Score計算
                        ICell cell = Game.Field.GetCell(p);

                        //移動成功確率をセルに保持。移動成功確率は前の成功確率とこの移動ができる確率の積
                        c.SuccessProbability = (1.0 - agentsProbabilities[c.Turn - 1][p.Y, p.X]) * c.Parent.SuccessProbability;


                        //移動に伴うコストは、盤面の平均値とする
                        //要改善、失敗確率の累積を考えていない
                        //敵のタイル情報で場合分け
                        switch (cell.GetState(EnemyEnum))
                        {
                            case CellState.Occupied:
                                c.Turn++; //敵のセルがある場合は、ターンが余計にかかるので、その分
                                agentsProbabilities.Add(
                                calculateExistenceProbability.MapPossibilities(Enemy.Agent1.Position, Enemy.Agent2.Position, Game.Field.Width, Game.Field.Height, c.Turn)
                                );
                                //２ターン必要ということで、成功確率も減る
                                c.SuccessProbability *= (1.0 - agentsProbabilities[c.Turn - 1][p.Y, p.X]);

                                return 16 - cell.Score * c.SuccessProbability + 2 * averageScore;
                            case CellState.InRegion:
                                return 16 - cell.Score * c.SuccessProbability + 1 * averageScore;
                        }
                        //自チームのタイル状況で場合分け
                        switch (cell.GetState(TeamEnum))
                        {
                            case CellState.None:
                                return 16 - cell.Score * c.SuccessProbability + 1 * averageScore;
                            case CellState.Occupied:
                                return 16 + 0 * c.SuccessProbability + 1 * averageScore;
                            case CellState.InRegion:
                                //取るとかえって損なセルがある
                                if (cell.Score >= 0)
                                    return 16 + 0 * c.SuccessProbability + 1 * averageScore;
                                else
                                    return 16 - cell.Score * c.SuccessProbability + 1 * averageScore;
                            default:
                                //ここに来ることはありえない
                                return 16 - cell.Score * c.SuccessProbability + 1 * averageScore;
                        }

                    }, (c) =>
                    {
                        //HeuristicScore計算
                        Point pos = c.Point - Agent.Position;
                        double minSteps = Math.Max(Math.Abs(pos.X), Math.Abs(pos.Y));
                        return (int)((16 - averageScore + 1) * minSteps);
                    });

                    //実際に移動させてみて得点がどうなるかを見る
                    field = Game.Field.Clone(true, true, true);

                    field.AutoEvaluate = false;

                    nextTeam = beforeTeam = startTeam;
                    nextEnemy = beforeEnemy = startEnemy;

                    needTurn = 0;
                    delta = 0;

                    //一つづつチェック
                    foreach (var node1 in AStarSearch.BestWayNodes)
                    {
                        if (field.Map[node1.Point.Y, node1.Point.X].GetState(TeamEnum) != CellState.Occupied)
                        {
                            field.Map[node1.Point.Y, node1.Point.X].SetState(TeamEnum, CellState.Occupied);
                            //値の変化を見る
                            nextTeam = field.EvaluateMap(TeamEnum);
                        }
                        //相手に占有されていたらその分ターンが必要なので
                        if (field.Map[node1.Point.Y, node1.Point.X].GetState(EnemyEnum) == CellState.Occupied)
                        {
                            needTurn++;
                            field.Map[node1.Point.Y, node1.Point.X].SetState(EnemyEnum, CellState.None);
                            nextEnemy = field.EvaluateMap(EnemyEnum);
                        }



                        delta += ((nextTeam - beforeTeam) - (nextEnemy - beforeEnemy)) * node1.SuccessProbability;

                        beforeTeam = nextTeam;
                        beforeEnemy = nextEnemy;

                        needTurn++;//必要ターン加算
                    }

                    //よりよい経路が見つかったら
                    if (BetterEfficiency < delta / needTurn)
                    {
                        betterRoute = AStarSearch.BestWayNodes.ToArray();
                        //最後のノードに推定能率を保持
                        BetterEfficiency = delta / needTurn;
                        NeedTurn = needTurn;
                    }
                }
            });
            //結果を保存
            BetterRoute = betterRoute;

            //得られた結果をWayクラスに変換
            Way = new Way(Agent, BetterRoute.Select(node2 => node2.Point).ToArray());
            Point agentNextPos;

            //次を取り出す
            Way.Peep(out agentNextPos, out Direction direction);


            return Game.GetIntent(Agent, direction);
        }

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }
    }
}
