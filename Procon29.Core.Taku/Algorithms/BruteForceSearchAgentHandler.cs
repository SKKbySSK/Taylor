using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Helper;
using Procon29.Core.Extensions;
using Procon29.Core.Genetic;
using Procon29.Core.Taku.ClassExtensions;


namespace Procon29.Core.Taku.Algorithms
{
    class BruteForceSearchAgentHandler : AgentAlgorithmBase
    {
        //盤面情報を保持（盤面情報が更新された場合にのみ探索を行うため）
        private ICell[,] Map { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }

        //探索の深さ
        public int Depth { get; private set; }

        public double BestDeltaScore { get; private set; }

        private CalculateExistenceProbability CalculateExistenceProbability { get; set; }


        //移動方向を保持
        public Way Way { get; set; }

        //コンストラクタ
        public BruteForceSearchAgentHandler(Game game, Agent agent, int depth = 4) : base(game, agent)
        {
            Depth = depth;
            Map = game.Field.Map;
            Width = game.Field.Width;
            Height = game.Field.Height;

            CalculateExistenceProbability = new CalculateExistenceProbability(depth);

            Way = new Way(agent, Direction.None);
            BestDeltaScore = 0;
        }

        protected override ScoringEfficiency EvaluateGame()
        {
            return new ScoringEfficiency(BestDeltaScore, Depth);
        }


        public override Intent Search(params Point[] prohibitedPoints)
        {
            //探索内最高点差を保持
            double bestScoreDifference = double.MinValue;
            //一時的な差を保持
            double delta;


            //エージェントの経路を保持
            Direction[] route;
            Direction[] bestDirections = new Direction[Depth];

            /*探索した部分をもとに戻すためのリスト*/
            List<Point> points = new List<Point>();

            //nターンで取れる行動を列挙するクラスEnumerateRoutesのインスタンスを作成
            EnumerateRoutes enumerateRoutes = new EnumerateRoutes
            {
                Turn = Depth
            };

            //作業用変数
            int i;
            Agent agent = new Agent(); /*探索用*/
            Point agentNextPos;
            ICell[,] map = Game.Field.Map.Clones();

            int startTeam = EvaluateMap(map, TeamEnum) , startEnemy = EvaluateMap(map, EnemyEnum),beforeTeam,beforeEnemy,nextTeam,nextEnemy;



            //Listで敵エージェントがそこに存在する確率を保持することで、オーバーヘッドを軽減
            List<double[,]> agentsProbabilities = new List<double[,]>();

            i = 1;
            //敵エージェント存在確率保持
            while (Depth >= i)
            {
                agentsProbabilities.Add(
                    CalculateExistenceProbability.MapPossibilities(
                        Enemy.Agent1.Position,
                        Enemy.Agent2.Position,
                        Game.Field.Width,
                        Game.Field.Height,
                        i)
                    );
                i++;
            }


            //最善手を探す
            while (enumerateRoutes.HasNext)
            {
                //経路の取得
                route = enumerateRoutes.Next();

                //初手　移動　or タイル除去　ができる場合のみを考える
                if (Game.CanMove(Agent, route[0]) || Game.CanRemove(Agent, route[0]))
                {
                    //エージェント情報のコピー
                    agent.Position = Agent.Position;
                    //マップをコピー
                    foreach (var p in points)
                    {
                        map[p.Y, p.X].State1 = Map[p.Y, p.X].State1;
                        map[p.Y, p.X].State2 = Map[p.Y, p.X].State2;
                    }

                    //書き換えた経路をリセット
                    points.Clear();

                    delta = 0.0;

                    beforeTeam = startTeam;
                    beforeEnemy = startEnemy;

                    //取り出した経路にしたがってエージェントを動かす
                    for (i = 0; i < Depth; i++)
                    {
                        //移動した場合の場所を検索
                        agentNextPos = agent.Position.FastMove(route[i]);
                        //移動した先がちゃんとマップ内であり、禁止領域でなければ移動
                        if (!(agentNextPos.X < 0 || agentNextPos.Y < 0 || agentNextPos.X >= Width || agentNextPos.Y >= Height) && prohibitedPoints.All(p => p != agentNextPos))
                        {
                            points.Add(agentNextPos);
                            AgentMove(map, TeamEnum, agent, route[i]);

                            nextTeam = EvaluateMap(map, TeamEnum);
                            nextEnemy = EvaluateMap(map, EnemyEnum);
                            delta += ((nextTeam - beforeTeam ) -  (nextEnemy - beforeEnemy)) *(1.0- agentsProbabilities[i][agentNextPos.Y, agentNextPos.X]);
                            beforeTeam = nextTeam;
                            beforeEnemy = nextEnemy;
                        }
                        else
                        {
                            //マップ外にでてしまった場合
                            goto NextRoutes;  //マップ外に出るような移動は考えない
                        }
                    }

                    //より高い得点を得られる経路であれば
                    if (delta > bestScoreDifference)
                    {   //方向の更新
                        route.CopyTo(bestDirections, 0);
                        //最高得点の更新
                        bestScoreDifference = delta;
                    }
                }
                NextRoutes:;
            }
            BestDeltaScore = bestScoreDifference;

            //最善経路の保持
            Way = new Way(Agent, bestDirections);
            Way.Next(out agentNextPos, out Direction direction);


            Intent Intent = new Intent()
            {
                Intention = Intentions.Stay,
                Direction = direction

            };

            //移動先に移動できれば移動する意志に、敵タイルがあれば除去
            if (Game.CanMove(Agent, direction))
            {
                Intent.Intention = Intentions.Move;
            }
            else if (Game.CanRemove(Agent, direction))
            {
                Intent.Intention = Intentions.Remove;
            }


            return Intent;
        }
        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            /*
            Point agentNextPos;
            Direction direction;

            //もう移動する手がない場合、新たに探索する
            if (!Way.Next(out agentNextPos, out direction))
            {
                return Search(prohibitedPoints);
            }
            else //まだ移動する場所が残っている
            {
                Intent Intent = new Intent()
                {
                    Intention = Intentions.Stay,
                    Direction = direction
                };
                //移動先に移動できれば移動する意志に、敵タイルがあれば除去
                if (Game.CanMove(Agent, direction))
                {
                    Intent.Intention = Intentions.Move;
                }
                else if (Game.CanRemove(Agent, direction))
                {
                    Intent.Intention = Intentions.Remove;
                }

                return Intent;
            }*/
            return Search(prohibitedPoints);
        }

        /// <summary>
        ///　エージェントを移動し、その移動に際してマップ情報を書き換える
        /// </summary>
        /// <param name="map">書き換えるマップ</param>
        /// <param name="team">移動させるエージェントの所属チーム</param>
        /// <param name="agent">移動させるエージェント情報</param>
        /// <param name="direction">エージェントの移動方向</param>
        public void AgentMove(ICell[,] map, Teams team, Agent agent, Direction direction)
        {
            //エージェントが移動したときの場所
            Point agentNextPos = agent.Position.FastMove(direction);
            //エージェント移動先セルに、相手タイルが置かれているかどうか
            if (map[agentNextPos.Y, agentNextPos.X].GetState(EnemyEnum) == CellState.Occupied)
            {
                //置かれていたら、相手タイルを取り除く
                map[agentNextPos.Y, agentNextPos.X].SetState(EnemyEnum, CellState.None);
            }
            else
            {
                //自チームタイルがまだ置かれていないか
                if (map[agentNextPos.Y, agentNextPos.X].GetState(TeamEnum) != CellState.Occupied)
                {
                    //自チームタイルをおく
                    map[agentNextPos.Y, agentNextPos.X].SetState(TeamEnum, CellState.Occupied);
                }
                //エージェントを移動
                agent.Position = agentNextPos;
            }

        }

        /// <summary>
        /// マップを評価し、指定されたチームの得点を計算します。
        /// 
        /// ｃ言語に近づけて速度向上を図ったby大久保
        /// </summary>
        /// <param name="Map">評価するマップ</param>
        /// <param name="team">評価するチーム</param>
        /// <returns>teamの得点</returns>  
        public int EvaluateMap(ICell[,] Map, Teams team)
        {
            //作業用変数
            int x, y;

            //占有点,領域点保持
            int tile = 0, territory = 0;
            //領域点判定用領域
            bool[,] evaluated = new bool[Height, Width];
            //占有点計算
            for (x = 0; x < Width; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    //占有していたら
                    if (Map[y, x].GetState(team) == CellState.Occupied)
                    {
                        //得点を追加
                        tile += Map[y, x].Score;
                        evaluated[y, x] = true;
                    }
                    else
                    {
                        //領域点計算用の準備
                        Map[y, x].SetState(team, CellState.None);
                        evaluated[y, x] = false;
                    }
                }
            }

            //外周を回って、領域を識別
            for (int k = 0; k < Width; k++)
            {
                Fill(0, k);
                Fill(Height - 1, k);
            }
            for (int k = 1; k < Height - 1; k++)
            {
                Fill(k, 0);
                Fill(k, Width - 1);
            }

            //識別した領域部分の得点を加算
            for (x = 0; x < Width; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    //領域であれば加算
                    if (evaluated[y, x] == false && Map[y, x].GetState(team) != CellState.Occupied)
                    {
                        //得点を追加(絶対値を取る)
                        territory += (Map[y, x].Score > 0) ? Map[y, x].Score : -Map[y, x].Score;
                        //領域の部分塗りつぶす
                        Map[y, x].SetState(team, CellState.InRegion);
                    }
                }
            }

            return tile + territory;


            //領域塗りつぶし&再帰処理関数:evaluatedでtrueであるところをタイルが置かれていると考える
            void Fill(int j, int i)
            {
                //盤面外に行ったら終わる
                if (!(i < 0 || j < 0 || i >= Width || j >= Height))
                {
                    //(i,j)が盤面内のセルを指す
                    //占有されていないセルかつまだ探索していないセル
                    if (evaluated[j, i] == false && Map[j, i].GetState(team) != CellState.Occupied)
                    {
                        //塗りつぶす
                        evaluated[j, i] = true;
                        //別のセルへ移動
                        Fill(j + 1, i);
                        Fill(j - 1, i);
                        Fill(j, i + 1);
                        Fill(j, i - 1);
                    }
                }
            }
        }

        public override TeamHandlerUI ProvideUI()
        {
            throw new NotImplementedException();
        }
    }
}
