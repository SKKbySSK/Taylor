using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Helper;
using Procon29.Core.Extensions;
using Procon29.Core.Genetic;

using Procon29.Core.Taku.ClassExtensions;
using Procon29.Core.Algorithms;

namespace Procon29.Core.Taku.Algorithms
{
    class EnemyTerritoryDeleteAgentHandler : AgentAlgorithmBase
    {

        private AStarSearch AStarSearch { get; set; } //このインスタンスのBestWayNodesがあるかないかで、経路があるかないかを判断

        //マップの平均得点を保持
        private readonly double averageScore;

        private Field EstimatedField { get; set; }

        //移動方向を保持
        public Way Way { get; set; }

        //消す領域の情報を保持
        (List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, Point center, int territory) DeleteTargetTerritoryInfo { get; set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="game">探索対象ゲーム</param>
        /// <param name="agent">探索対象エージェントS</param>
        public EnemyTerritoryDeleteAgentHandler(Game game, Agent agent) : base(game, agent)
        {
            DeleteTargetTerritoryInfo = (new List<(ICell iCell, Point point)>(), new List<(ICell iCell, Point point)>(), new Point(), 0);
            Way = new Way(agent, Direction.None);

            //アルゴリズム実行部分
            AStarSearch = new AStarSearch();
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

            averageScore /= Game.Field.Width * Game.Field.Height;
        }

        /// <summary>
        /// 敵領域妨害法で探索した場合の能率を計算
        /// 実際に経路に沿って移動して能率を計算する
        /// </summary>
        /// <returns></returns>
        protected override ScoringEfficiency EvaluateGame()
        {
            //移動によるスコアを計算
            if (AStarSearch.BestWayNodes != null)
            {
                int nowIndex = AStarSearch.BestWayNodes.FindIndex((n) => n.Point == Agent.Position);
                int beforeTeam, beforeEnemy, nextTeam, nextEnemy;
                int needTurn = 0;
                double delta = 0.0;
                //移動させて推定得点を出すために、フィールドを複製
                Field field = Game.Field.Clone(true, true, true);
                field.AutoEvaluate = false;

                //敵の存在する確率を求めるクラスを保持
                var calculateExistenceProbability = new CalculateExistenceProbability(1);

                //初期値を設定
                beforeTeam = field.EvaluateMap(TeamEnum);
                beforeEnemy = field.EvaluateMap(EnemyEnum);


                //一つづつ移動してみて、推定能率を計算,最後だけ重複している
                foreach (var node1 in AStarSearch.BestWayNodes.Skip(nowIndex))
                {
                    //ターン追加
                    needTurn++;
                    //最後以外は自チームにタイルを置く
                    if (node1 != AStarSearch.BestWayNodes.Last())
                    {
                        field.Map[node1.Point.Y, node1.Point.X].SetState(TeamEnum, CellState.Occupied);
                        //相手に占有されていたらその分ターンが必要なので
                        if (field.Map[node1.Point.Y, node1.Point.X].GetState(EnemyEnum) == CellState.Occupied)
                        {
                            needTurn++;
                            field.Map[node1.Point.Y, node1.Point.X].SetState(EnemyEnum, CellState.None);
                        }
                    }
                    else
                    {
                        //最後は、相手のタイルを除去するだけ
                        field.Map[node1.Point.Y, node1.Point.X].SetState(EnemyEnum, CellState.None);
                    }


                    //値の変化を見る
                    nextTeam = field.EvaluateMap(TeamEnum);
                    nextEnemy = field.EvaluateMap(EnemyEnum);

                    delta += ((nextTeam - beforeTeam) - (nextEnemy - beforeEnemy))
                        * (1 - calculateExistenceProbability.MapPossibilities(Enemy.Agent1.Position, Enemy.Agent2.Position, field.Width, field.Height, needTurn)[node1.Point.Y, node1.Point.X]);

                    beforeTeam = nextTeam;
                    beforeEnemy = nextEnemy;
                }

                //最後のタイル剥がしのぶんの

                //得点を計算
                return new ScoringEfficiency(delta, needTurn);
            }

            //経路がない場合
            return new ScoringEfficiency(0, 0);
        }


        /// <summary>
        /// 敵領域妨害法に基づいて移動経路を求める
        /// </summary>
        /// <param name="prohibitedPoints">タイル除去・移動禁止エリアを指定</param>
        /// <returns>このIntentの取得によってWayは変化しない。つまり、正常に移動させるにはNextIntentでIntentを取得する必要がある</returns>
        public override Intent Search(params Point[] prohibitedPoints)
        {
            //敵の領域を形成しているセルの情報を取得
            var makingTerritoryCellsInfo = GetMakingTerritoryCellsInfo(EnemyEnum);

            //相手は領域を確保しているかで分岐
            if (makingTerritoryCellsInfo.Any())
            {
                //領域の重心の位置をリストに追加
                List<(List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, Point center, int territory)> territoriesInfo
                    = makingTerritoryCellsInfo.Select((list) =>
                    {
                        Point point = new Point(0, 0);
                        int mass = 0;
                        //重心を計算
                        foreach (var cell in list.enclosingCellsInfo)
                        {
                            point += Math.Abs(cell.iCell.Score) * cell.point;
                            mass += Math.Abs(cell.iCell.Score);
                        }
                        //ゼロ除算はまずいので(基本的に０になることはないはず)
                        if (mass == 0)
                            mass = 1;
                        return (list.enclosingCellsInfo, list.enclosedCellsInfo, point / mass, list.territory);
                    }).ToList();

                //領域総合ベクトルを計算(2.~3.を実施)：原点を自エージェントの位置とする
                // Purpose : 領域が集中している（定義：領域の中で、その領域によって得られる得点が大きいような領域たちが寄り集まっているところ）ところを取得すること

                //まず、一つ一つの領域の重心のエージェントからの相対位置をもとめ、
                //それを移動に必要な最小ステップ数で割って簡易的に単位化した後、
                //その領域の重み（領域によって得られる得点）をかけることで、
                //領域ベクトル（ベクトルの方向:領域の方向、ベクトルのノルム:領域の重要度合い、表すようなベクトル）を計算する。
                List<(Point direction, int territory)> territoriesCenterDirection = territoriesInfo.Select(t =>
                {
                    Point p = t.center - Agent.Position;
                    //重心に移動するのに必要な最小ステップ数を計算、これを単位ベクトル化する
                    int minSteps = Math.Max(Math.Abs(p.X), Math.Abs(p.Y));
                    if (minSteps == 0)//除算の都合で、重心と自分がかぶっている場合は1とする
                        minSteps = 1;
                    return ((p * t.territory) / minSteps, t.territory);
                }).ToList();

                //各領域ベクトルを元に、領域総合ベクトルを求める。
                Point total = new Point();
                foreach (var (_direction, _) in territoriesCenterDirection)
                {
                    total += _direction;
                }

                //領域ベクトルと、領域総合ベクトルの内積を取って、その中から最も大きな領域を、消去対象の領域とする
                int dotMax = int.MinValue;
                int i = 0, index = 0;
                foreach (var t in territoriesCenterDirection)
                {
                    if (total.Dot(t.direction) > dotMax)
                    {
                        dotMax = total.Dot(t.direction);
                        index = i;
                    }
                    i++;
                }

                //消去対象領域の領域に関する情報を保持
                DeleteTargetTerritoryInfo = territoriesInfo[index];

                //新たに探索した、消去領域を消去することを考えた場合の移動経路を取得
                Way = Routes(prohibitedPoints);
            }
            else
            {
                //相手が領域を一つも確保していなかったときの処理
                //いまはとりあえず何もしないものとする
                Way = new Way(Agent, Direction.None);

                //探索した情報を消す
                AStarSearch.ResetResult();
            }

            Point waste = new Point();

            Direction direction;
            Way.Peep(out waste, out direction);

            return Game.GetIntent(Agent, direction);
        }

        /// <summary>
        /// 次にエージェントが取るべき行動を取得
        /// </summary>
        /// <param name="prohibitedPoints">タイル除去・移動禁止エリアを指定</param>
        /// <returns></returns>
        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            Intent Intent = new Intent()
            {
                Intention = Intentions.Stay,
                Direction = Direction.None
            };

            Point agentNextPos = new Point();
            Direction direction = Direction.None;

            //すべての移動を終えたor禁止領域（探索の必要があるか）|| DeleteTargetTerritoryInfo.enclosedCellsInfo.All((taple) => taple.iCell.GetState(EnemyEnum) != CellState.InRegion
            if (!Way.Peep(out agentNextPos, out direction) || prohibitedPoints.Any(p => p == agentNextPos))
            {
                //新たに探索
                Search(prohibitedPoints);
            }

            //正式に経路を取得
            Way.Next(out agentNextPos, out direction);

            return Game.GetIntent(Agent, direction);

            //return Search(prohibitedPoints);
        }

        /// <summary>
        /// DeleteTerritoryに移動する最善経路を求める
        /// </summary>
        /// <param name="prohibitedPoints">タイル除去・移動禁止エリアを指定</param>
        /// <returns></returns>
        private Way Routes(params Point[] prohibitedPoints)
        {
            //探索されている情報を消す
            AStarSearch.ResetResult();

            //スタートノードをセット
            AStarSearch.StartNode = AStarSearch.Nodes[Agent.Position.Y, Agent.Position.X];
            //エンドノードたちをセット
            AStarSearch.EndNodes = DeleteTargetTerritoryInfo.enclosingCellsInfo.Select((list) => AStarSearch.Nodes[list.point.Y, list.point.X]).ToArray();

            //禁止領域に対応するノードを予めクローズすることで移動経路に含めないようにする
            foreach (var p in prohibitedPoints)
            {
                AStarSearch.Nodes[p.Y, p.X].Status = NodeStatus.Closed;
            }

            //敵の存在する確率を求めるクラスを保持
            var calculateExistenceProbability = new Core.Algorithms.CalculateExistenceProbability(1);

            //敵エージェントがそこに存在する確率を計算するたびにListに保持することで、同じ確率を何回も計算しないようにする。＝オーバーヘッドを軽減
            List<double[,]> agentsProbabilities = new List<double[,]>();

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
                //Warning:失敗確率の累積を考えていない
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

            //得られた結果をWayクラスに変換
            return new Way(Agent, AStarSearch.BestWayPoints.ToArray());
        }

        /// <summary>
        /// 領域を形成しているセルの情報を取得するメソッド
        /// </summary>
        /// <param name="team">領域形成セル探索対象チーム</param>
        /// <returns>領域を形成しているセルたちの情報（領域を形成しているセル,領域になっているセル,セルの作る得点）のリスト</returns>
        protected List<(List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)> GetMakingTerritoryCellsInfo(Teams team)
        {
            int territory;
            bool[,] evaluated = new bool[Game.Field.Height, Game.Field.Width];
            //まず情報を保持
            List<(List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)> list
                = new List<(List<(ICell iCell, Point point)>, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)>();

            //すべての領域を探索
            for (int i = 0; i < Game.Field.Width; i++)
            {
                for (int j = 0; j < Game.Field.Height; j++)
                {
                    //まだ評価していない領域点があれば
                    if (Game.Field.GetCell(i, j).GetState(team) == CellState.InRegion && evaluated[j, i] == false)
                    {
                        territory = 0;
                        List<(ICell, Point)> listEnclosing = new List<(ICell, Point)>();
                        List<(ICell, Point)> listEnclosed = new List<(ICell, Point)>();
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, j, i);

                        //領域を形成しているセルの情報を、エージェントの位置に応じて加工(完全に領域を除去するため)
                        List<Point> listEnclosingPoints = listEnclosing.Select(l => l.Item2).ToList();
                        AdjustEnclosingCells(listEnclosingPoints, Agent.Position);

                        listEnclosing = listEnclosingPoints.Select((p) => (Game.Field.GetCell(p.X, p.Y), p)).ToList();


                        //領域の情報を追加
                        list.Add((listEnclosing, listEnclosed, territory));
                    }
                }
            }

            //ある領域を形成するセルを取得＆その領域の得点を計算
            void SearchMakingTerritoryCells(ref List<(ICell, Point)> listEnclosing, ref List<(ICell, Point)> listEnclosed, int y, int x)
            {
                //マップ内かどうか
                if (!(x < 0 || y < 0 || x >= Game.Field.Width || y >= Game.Field.Height))
                {
                    CellState cellState = Game.Field.GetCell(x, y).GetState(team);
                    //まだ未探索&領域セル
                    if (cellState == CellState.InRegion && evaluated[y, x] == false)
                    {
                        //探索済みにする
                        evaluated[y, x] = true;
                        //領域点をついでに計算
                        territory += (Game.Field.GetCell(x, y).Score >= 0) ? Game.Field.GetCell(x, y).Score : -Game.Field.GetCell(x, y).Score;
                        //領域の情報を追加
                        listEnclosed.Add((Game.Field.GetCell(x, y), new Point(x, y)));

                        //周りのセルを探索する
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y - 1, x);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y + 1, x);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y, x - 1);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y, x + 1);
                    }
                    //領域を形成するセルであるか
                    else if (cellState == CellState.Occupied)
                    {
                        listEnclosing.Add((Game.Field.GetCell(x, y), new Point(x, y)));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 領域とpointとの位置関係から、領域を形成するセルの部分を外に這わせるか、中に這わせるかを決めて、リストの中身を調整するメソッド
        /// </summary>
        /// <param name="enclosingCellPoints">調整対象の領域を形成するセルのリスト</param>
        /// <param name="point">領域の内外を判定するための基準の位置</param>
        protected void AdjustEnclosingCells(List<Point> enclosingCellPoints, Point point)
        {
            //領域の外と中を区別するための配列
            bool[,] evaluated = new bool[Game.Field.Height, Game.Field.Width];

            //外周を回って、領域を識別
            for (int k = 0; k < Game.Field.Width; k++)
            {
                Fill(0, k);
                Fill(Game.Field.Height - 1, k);
            }
            for (int k = 1; k < Game.Field.Height - 1; k++)
            {
                Fill(k, 0);
                Fill(k, Game.Field.Width - 1);
            }


            //外から回って印をつけた(trueにした)ところに、pointがあれば外（false）、なければ中（true）
            //もし、pointが領域の内側にいる場合、領域を完全に消せるように、領域を形成するセルたちの一番外側の部分を取得する
            if (evaluated[point.Y, point.X] == false)
            {
                evaluated.ForEach((p, b) => !b);//値をひっくり返す
                Point startPoint = enclosingCellPoints.First();//塗りつぶし開始地点を保持

                enclosingCellPoints.Select((p) => evaluated[p.Y, p.X] = false).ToList();

                enclosingCellPoints.Clear();//領域を一度開放

                //塗りつぶしながら、領域を形成するセルの最も外側のセルたちを取得
                Fill2(startPoint.Y, startPoint.X);

                //領域塗りつぶし&再帰処理関数:領域の境界線からスタートして、外側に出てしまったら
                bool Fill2(int j, int i)
                {
                    //盤面外に行ったら終わる
                    if (!(i < 0 || j < 0 || i >= Game.Field.Width || j >= Game.Field.Height))
                    {
                        //(i,j)が盤面内のセルを指す
                        //未探索のエリアで、占有されているセルではないのなら、それが一番外側
                        if (evaluated[j, i] == false && Game.Field.Map[j, i].GetState(EnemyEnum) != CellState.Occupied)
                        {
                            return true; //一番外側に来た
                        }
                        else if (evaluated[j, i] == false)//未知のエリアではある
                        {
                            //塗りつぶす
                            evaluated[j, i] = true;
                            //別のセルへ移動した先が、領域を形成するセルから外れたら,そいつが領域を形成する最も外側のセル
                            if (Fill2(j + 1, i) || Fill2(j - 1, i) || Fill2(j, i + 1) || Fill2(j, i - 1))
                            {
                                enclosingCellPoints.Add(new Point(i, j));
                            }

                        }
                        //あとのやつはそれより内側なので
                        return false;
                    }
                    else
                    {
                        return true; //一番外側に来た
                    }
                }
            }

            //領域塗りつぶし&再帰処理関数:evaluatedでtrueであるところに領域を形成するセルが存在していると考える
            void Fill(int j, int i)
            {
                //盤面外に行ったら終わる
                if (!(i < 0 || j < 0 || i >= Game.Field.Width || j >= Game.Field.Height))
                {
                    //(i,j)が盤面内のセルを指す
                    //占有されていないセルかつ領域を形成しているセルでない
                    if (evaluated[j, i] == false && enclosingCellPoints.All(p => p.X != i || p.Y != j))
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

        /// <summary>
        /// pointがenclosingCellPointsの作る領域の中にいるかどうか
        /// </summary>
        /// <param name="enclosingCellPoints"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        bool IsInTerritory(List<Point> enclosingCellPoints, Point point)
        {

            //領域の外と中を区別するための配列
            bool[,] evaluated = new bool[Game.Field.Height, Game.Field.Width];

            //外周を回って、領域を識別
            for (int k = 0; k < Game.Field.Width; k++)
            {
                Fill(0, k);
                Fill(Game.Field.Height - 1, k);
            }
            for (int k = 1; k < Game.Field.Height - 1; k++)
            {
                Fill(k, 0);
                Fill(k, Game.Field.Width - 1);
            }


            //外から回って印をつけた(trueにした)ところに、pointがあれば外（false）、なければ中（true）
            return evaluated[point.Y, point.X] == false;

            //領域塗りつぶし&再帰処理関数:evaluatedでtrueであるところをタイルが置かれていると考える
            void Fill(int j, int i)
            {
                //盤面外に行ったら終わる
                if (!(i < 0 || j < 0 || i >= Game.Field.Width || j >= Game.Field.Height))
                {
                    //(i,j)が盤面内のセルを指す
                    //占有されていないセルかつ領域を形成しているセルでない
                    if (evaluated[j, i] == false && enclosingCellPoints.All(p => p.X != i || p.Y != j))
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
