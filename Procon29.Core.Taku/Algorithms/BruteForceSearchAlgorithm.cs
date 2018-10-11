using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core;
using Procon29.Core.Genetic;
using Procon29.Core.Extensions;
using Procon29.Core.Taku.ClassExtensions;

namespace Procon29.Core.Taku.Algorithms
{
    /// <summary>
    /// 力任せ法を実装したクラス
    /// 
    /// 注意点：
    /// この力任せ法では指定されたチームのみを考慮して探索を行う（相手の動きを考慮しないで探索を行う）
    /// 
    /// チーム単位でインスタンスが保持されることを想定。
    /// (エージェント単位でこのクラスのインスタンスを持つよりも、チーム単位でこのクラスのインスタンスを保持したほうが効率が良いとおもうが、ほとんど変わらないかも？)
    public class BruteForceSearchAlgorithm : SearchAlgorithmBase
    {
        //盤面情報を保持（盤面情報が更新された場合にのみ探索を行うため）
        private ICell[,] Map { get; set; }
        private int Width { get; set; }
        private int Height { get; set; }

        private int TeamScore { get; set; }
        private int EnemyScore { get; set; }

        //現在の盤面での最善手を保持
        public (Intent Agent1, Intent Agent2) BestIntention { get; private set; }

        //何ターンの先読みによって最善手bestIntentionを決めたかを保持
        public int Depth { get; set; } = 4;
        //最善手を選んだときの最高得点差
        public int[] BestScoreDifference { get; private set; }

        //コンストラクタ
        public BruteForceSearchAlgorithm(Game game, Teams team, Agent agent, int depth = 4) : base(game, team, agent)
        {
            //盤面のコピー
            Width = Field.Width;
            Height = Field.Height;
            Map = Field.Map;

            Depth = depth;
        }
        public override void Search()
        {

            //探索内最高点差を保持
            int bestScoreDifference = int.MinValue;

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
            int i, j;
            Agent agent = new Agent(); /*探索用*/
            Point agentNextPos;
            ICell[,] map = Map.Clones();

            //マップを一度複製
            for (i = 0; i < Width; i++)
            {
                for (j = 0; j < Height; j++)
                {
                    map[j, i].State1 = Map[j, i].State1;
                    map[j, i].State2 = Map[j, i].State2;
                }
            }
            //最善手を探す
            while (enumerateRoutes.HasNext)
            {
                //経路の取得
                route = enumerateRoutes.Next();

                //初手　移動　or タイル除去　ができる場合のみを考える
                if (Game.CanMove(Agent,route[0]) || Game.CanRemove(Agent,route[0]))
                {
                    //エージェント情報のコピー
                    agent.Position = Agent.Position;
                    //マップをコピー
                    foreach (var p in points)
                    {
                        map[p.Y, p.X].State1 = Map[p.Y, p.X].State1;
                        map[p.Y, p.X].State2 = Map[p.Y, p.X].State2;
                    }

                    //リセット
                    points.Clear();

                    //取り出した経路にしたがってエージェントを動かす
                    for (i = 0; i < Depth; i++)
                    {
                        agentNextPos = agent.Position.FastMove(route[i]);
                        //移動した先がちゃんとマップ内であれば移動
                        if (!(agentNextPos.X < 0 || agentNextPos.Y < 0 || agentNextPos.X >= Width || agentNextPos.Y >= Height))
                        {
                            points.Add(agentNextPos);
                            AgentMove(map, TeamEnum, agent, route[i]);
                        }
                        else
                        {
                            //マップ外にでてしまった場合
                            goto NextRoutes;  //マップ外に出るような移動は考えない
                        }
                    }

                    //チームの得点を計算
                    TeamScore = EvaluateMap(map, TeamEnum);
                    EnemyScore = EvaluateMap(map, EnemyEnum);

                    //より高い得点を得られる経路であれば
                    if (TeamScore - EnemyScore > bestScoreDifference)
                    {   //方向の更新
                        route.CopyTo(bestDirections, 0);
                        //最高得点の更新
                        bestScoreDifference = TeamScore - EnemyScore;
                        EstimatedTeamScore = TeamScore;
                        EstimatedEnemyScore = EnemyScore;

                    }
                }

                NextRoutes:;
            }
            //最善経路の保持
            Way = new Helper.Way(Agent, bestDirections);

            //必要ターン数保持
            NeedTurn = Depth;
        }

        public override Intent NextIntent()
        {
            Search();

            Direction direction;
            Point p = new Point();
            Way.Next(out p, out direction);

            //エージェントが移動した場合の位置を保持
            Point agentNextPos = Agent.Position.FastMove(direction);
            //移動先に移動できれば移動する意志に、敵タイルがあれば除去、敵がいる場合は仕方ないので移動しない
            Intentions intention;

            //エージェントの移動に際して
            //相手のタイルが移動先にあるか
            if (Field.GetCell(agentNextPos).GetState(EnemyEnum) != CellState.None)
            {
                //敵エージェントがそのセルの上にいるか
                if (Enemy.Agent1.Position == agentNextPos || Enemy.Agent2.Position == agentNextPos)
                {
                    intention = Intentions.Stay;
                }
                else
                    intention = Intentions.Remove;
            }
            else
            {
                intention = Intentions.Move;
            }
            Intent = new Intent()
            {
                Intention = intention,
                Direction = direction
            };
            return Intent;
        }
        

        /// <summary>
        ///  力任せ法に基づいて意志を決定する関数、盤面情報が変更されたときにのみ探索を行うようにする
        /// </summary>
        /// <returns ></returns>
        public (Intent intent1, Intent intent2) SearchRoutes()
        {
            //探索内最高点差を保持
            BestScoreDifference = new int[] { int.MinValue, int.MinValue };

            //エージェントの経路を保持
            Direction[] route;
            Direction[] bestDirections = new Direction[] { Direction.None, Direction.None };

            //最善手を探す
            EnumerateRoutes enumerateRoutes = new EnumerateRoutes();
            enumerateRoutes.Turn = Depth;

            //作業用変数
            int i, j;
            Agent agent = new Agent();
            ICell[,] map = Map.Clones();
            Point p;

            while (enumerateRoutes.HasNext)
            {
                //経路の取得
                route = enumerateRoutes.Next();

                //エージェント１について
                agent.Position = Team.Agent1.Position;
                //マップをコピー
                for (i = 0; i < Width; i++)
                {
                    for (j = 0; j < Height; j++)
                    {
                        map[j, i].State1 = Map[j, i].State1;
                        map[j, i].State2 = Map[j, i].State2;
                    }
                }

                for (i = 0; i < Depth; i++)
                {
                    p = agent.Position.FastMove(route[i]);
                    //移動した先がちゃんとマップ内であれば移動
                    if (!(p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height))
                    {
                        AgentMove(map, TeamEnum, agent, route[i]);
                    }
                    else
                        goto Next;  //マップ外に出るような移動は考えない
                }

                //チームの得点を計算
                TeamScore = EvaluateMap(map, TeamEnum);
                EnemyScore = EvaluateMap(map, EnemyEnum);

                //より高い得点を得られる経路であれば
                if (TeamScore - EnemyScore > BestScoreDifference[0])
                {   //方向の更新
                    bestDirections[0] = route[0];
                    //最高得点の更新
                    BestScoreDifference[0] = TeamScore - EnemyScore;
                }

                Next:
                //エージェント2について移動
                agent.Position = Team.Agent2.Position;

                //マップをコピー
                for (i = 0; i < Width; i++)
                {
                    for (j = 0; j < Height; j++)
                    {
                        map[j, i].State1 = Map[j, i].State1;
                        map[j, i].State2 = Map[j, i].State2;
                    }
                }

                for (i = 0; i < Depth; i++)
                {
                    p = agent.Position.FastMove(route[i]);
                    //移動した先がちゃんとマップ内であれば移動
                    if (!(p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height))
                    {
                        AgentMove(map, TeamEnum, agent, route[i]);
                    }
                    else
                        goto End;  //マップ外に出るような移動は考えない
                }

                //チームの得点を計算
                TeamScore = EvaluateMap(map, TeamEnum);
                EnemyScore = EvaluateMap(map, EnemyEnum);

                //より高い得点を得られる経路であるか
                if (TeamScore - EnemyScore > BestScoreDifference[1])
                {   //方向の更新
                    bestDirections[1] = route[0];
                    //最高得点の更新
                    BestScoreDifference[1] = TeamScore - EnemyScore;
                }
                End:;
            }


            //エージェントが移動した場合の位置を保持
            Point agent1NextPos = Team.Agent1.Position.FastMove(bestDirections[0]);
            Point agent2NextPos = Team.Agent2.Position.FastMove(bestDirections[1]);

            //移動先に移動できれば移動する意志に、敵タイルがあれば除去、敵がいる場合は仕方ないので移動しない
            Intentions intention1, intention2;


            //エージェント１の移動に際して
            //相手のタイルが移動先にあるか
            if (Field.GetCell(agent1NextPos).GetState(EnemyEnum) != CellState.None)
            {
                //敵エージェントがそのセルの上にいるか
                if (Enemy.Agent1.Position == agent1NextPos || Enemy.Agent2.Position == agent1NextPos)
                {
                    intention1 = Intentions.Stay;
                }
                else
                    intention1 = Intentions.Remove;
            }
            else
            {
                intention1 = Intentions.Move;
            }
            //エージェント2の移動に際して
            //相手のタイルが移動先にあるか
            if (Field.GetCell(agent2NextPos).GetState(EnemyEnum) != CellState.None)
            {
                //敵エージェントがそのセルの上にいるか
                if (Enemy.Agent1.Position == agent2NextPos || Enemy.Agent2.Position == agent2NextPos)
                {
                    intention2 = Intentions.Stay;
                }
                else
                    intention2 = Intentions.Remove;
            }
            else
            {
                intention2 = Intentions.Move;
            }

            BestIntention = (
            new Intent()
            {
                Intention = intention1,
                Direction = bestDirections[0]
            },
            new Intent()
            {
                Intention = intention2,
                Direction = bestDirections[1]
            });

            //最善手を返す
            return BestIntention;
        }


        //エージェントの移動に際して、マップ情報を更新する
        private void UpdateMapInfo(ICell[,] Map, Agent agent, Direction direction)
        {
            //エージェントが移動したときの場所
            Point agentNextPos = agent.Position.FastMove(direction);

            //エージェント移動先セルに、相手タイルが置かれているかどうか
            if (Map[agentNextPos.Y, agentNextPos.X].GetState(EnemyEnum) == CellState.Occupied)
            {
                //置かれていたら、相手タイルを取り除く
                Map[agentNextPos.Y, agentNextPos.X].SetState(EnemyEnum, CellState.None);
                //相手のタイルを取り除いたことによって相手の得点が変化するので再評価
                EnemyScore = EvaluateMap(Map, EnemyEnum);
            }
            else
            {
                //自チームタイルがまだ置かれていないか
                if (Map[agentNextPos.Y, agentNextPos.X].GetState(TeamEnum) != CellState.Occupied)
                {
                    //自チームタイルをおく
                    Map[agentNextPos.Y, agentNextPos.X].SetState(TeamEnum, CellState.Occupied);

                    //新たにタイルを置いたことによって自分の得点が変化するので再評価
                    TeamScore = EvaluateMap(Map, TeamEnum);
                }
                //エージェントを移動
                agent.Position = agentNextPos;
            }
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

            //エージェントが指定された場所に移動できるか

            //移動できる

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
    }
}
