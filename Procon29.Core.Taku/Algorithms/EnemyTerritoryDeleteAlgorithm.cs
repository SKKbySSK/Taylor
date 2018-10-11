using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Helper;

using Procon29.Core.Taku.ClassExtensions;
using Procon29.Core.Algorithms;

namespace Procon29.Core.Taku.Algorithms
{
    public class EnemyTerritoryDeleteAlgorithm : SearchAlgorithmBase
    {

        private AStarSearch AStarSearch { get; set; }

        //消す領域の情報を保持
        (List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, Point center, int territory) DeleteTargetTerritoryInfo { get; set; }

        public EnemyTerritoryDeleteAlgorithm(Game game, Teams team, Agent agent) : base(game, team, agent)
        {
            DeleteTargetTerritoryInfo = (new List<(ICell iCell, Point point)>(), new List<(ICell iCell, Point point)>(), new Point() , 0);
        }

        //敵領域点妨害法に従って経路を求める
        public override void Search()
        {
            //敵の領域を形成しているセルの情報から重心を計算する
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
                        point += cell.iCell.Score * cell.point;
                        mass += Math.Abs(cell.iCell.Score);
                    }
                    //ゼロ除算はまずいので
                    if (mass == 0)
                        mass = 1;
                    return (list.enclosingCellsInfo, list.enclosedCellsInfo, point / mass, list.territory);
                }).ToList();

                //領域総合ベクトルを計算(2.~3.を実施)
                List<(Point direction, int territory)> territoriesCenterDirection = territoriesInfo.Select(t =>
                     {
                         Point p = t.center - Agent.Position;
                         int minSteps = Math.Max(Math.Abs(p.X), Math.Abs(p.Y));
                         if (minSteps == 0)
                             minSteps = 1;
                         return (p * t.territory / minSteps, t.territory);
                     }).ToList();

                Point total = new Point();
                foreach (var (direction, _) in territoriesCenterDirection)
                {
                    total += direction;
                }


                //内積が最も大きい領域を保持するタプル
                (List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, Point center, int territory) bestTerritoryInfo
                    = (new List<(ICell iCell, Point point)>(), new List<(ICell iCell, Point point)>(), new Point(), 0);

                int dotMax = int.MinValue;
                //内積の最も大きな領域を選ぶ
                foreach (var t in territoriesInfo)
                {
                    if (total.Dot(t.center - Agent.Position) > dotMax)
                    {
                        dotMax = total.Dot(t.center - Agent.Position);
                        bestTerritoryInfo = t;
                    }
                }

                //消去対象領域の領域に関する情報を保持
                DeleteTargetTerritoryInfo = bestTerritoryInfo;

                //アルゴリズム実行部分
                AStarSearch = new AStarSearch();
                //ノードを作成
                Node[,] nodes = new Node[Field.Height, Field.Width];
                //マップの平均得点を保持
                double averageScore = 0;
                //ノードにデータを入れる＋マップの得点の平均値を求める
                for (int i = 0; i < Field.Width; i++)
                {
                    for (int j = 0; j < Field.Height; j++)
                    {
                        nodes[j, i] = new Node();
                        //得点の総計
                        averageScore += Field.GetCell(i, j).Score;
                    }
                }
                //平均化
                averageScore /= Field.Width * Field.Height;
                //ノードをセット
                AStarSearch.Nodes = nodes;

                //スタートノードをセット
                AStarSearch.StartNode = nodes[Agent.Position.Y, Agent.Position.X];
                //エンドノードたちをセット
                List<Point> bestTerritoryEnclosiongCells = bestTerritoryInfo.enclosingCellsInfo.Select((list) => list.point).ToList();
                AStarSearch.EndNodes = bestTerritoryEnclosiongCells.Select((p) => nodes[p.Y, p.X]).ToArray();

                //探索開始
                AStarSearch.Search((c) =>
                {
                    Point p = c.Point;
                    //Score計算
                    ICell cell = Field.GetCell(p);
                    //エージェントの移動コスト倍率:セルが平均スコア以下であれば遠くに行きたがらなくなる
                    int Cost = (int)averageScore;

                    //敵のタイル情報で場合分け
                    switch (cell.GetState(EnemyEnum))
                    {
                        case CellState.Occupied:
                            return 16 - cell.Score + 2 * Cost;
                        case CellState.InRegion:
                            return 16 - cell.Score + 1 * Cost;
                    }
                    //自チームのタイル状況で場合分け
                    switch (cell.GetState(TeamEnum))
                    {
                        case CellState.None:
                            return 16 - cell.Score + 1 * Cost;
                        case CellState.Occupied:
                            return 16 + 0 + 1 * Cost;
                        case CellState.InRegion:
                            //取るとかえってそんなセルがある
                            if (cell.Score >= 0)
                                return 16 + 0 + 1 * Cost;
                            else
                                return 16 - cell.Score + 1 * Cost;
                        default:
                            //ここに来ることはありえない
                            return 16 - cell.Score + 1 * Cost;
                    }
                }, (c) =>
                 {
                     //HeuristicScore計算
                     Point pos = c.Point - Agent.Position;
                     double minSteps = Math.Max(Math.Abs(pos.X), Math.Abs(pos.Y));
                     return (int)((16 - averageScore + 1) * minSteps);
                 });

                //得られた結果をWayクラスに変換
                Way = new Way(Agent, AStarSearch.BestWayPoints.ToArray());

                //得点を推定
                //移動させたあとの盤面を作る
                Field field = Field.Clone(true, true, true);
                Agent agent = new Agent { Position = Agent.Position };
                //移動にかかるターン数
                int turnCount = 0;

                Way.MoveDirections.Select((d) =>
                {
                    field.AgentMove(agent, d, false);
                    turnCount++;
                    if (field.GetCell(agent.Position).GetState(TeamEnum) != CellState.Occupied)
                    {
                        field.AgentMove(agent, d, false);
                        turnCount++;
                    }
                    return 0;
                });

                //盤面を評価
                field.AgentMove(agent, Direction.None);

                EstimatedTeamScore = (TeamEnum == Teams.Team1) ? field.Score1 : field.Score2;
                EstimatedEnemyScore = (TeamEnum == Teams.Team1) ? field.Score2 : field.Score1;
                NeedTurn = turnCount;
            }
            else
            {
                //相手が領域を一つも確保していなかったときの処理

                //いまはとりあえず何もしないものとする
                Way = new Way(Agent, Direction.None);
            }
        }

        public override Intent NextIntent()
        {

            Point waste = new Point();
            Direction direction = Direction.None;

            //すべての移動を終えたor 領域が消失した->（探索の必要があるか）
            if (Way.Turn >= Way.MovePoints.Count || !DeleteTargetTerritoryInfo.enclosedCellsInfo.All((taple) => taple.iCell.GetState(EnemyEnum) == CellState.InRegion))
            {
                //新たに探索
                Search();
            }

            //経路が存在するか
            if (Way.Turn < Way.MovePoints.Count)
            {
                direction = Way.MoveDirections.ElementAt(Way.Turn);
                //移動したい方向に敵タイルがある
                if (CanRemove(Agent, Way.MoveDirections.ElementAt(Way.Turn)))
                {
                    Intent.Intention = Intentions.Remove;
                }
                else
                {
                    Way.Next(out waste, out direction);

                    if (CanMove(Agent, direction))
                    {
                        Intent.Intention = Intentions.Move;
                    }
                    else
                    {
                        Intent.Intention = Intentions.Stay;
                    }
                }
            }
            //方向指定
            Intent.Direction = direction;
            return Intent;
        }

        /// <summary>
        /// 領域を形成しているセルの情報を取得するメソッド
        /// </summary>
        /// <param name="team">領域形成セル探索対象チーム</param>
        /// <returns>領域を形成しているセルたちの情報（領域を形成しているセル,領域になっているセル,セルの作る得点）のリスト</returns>
        protected List<(List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)> GetMakingTerritoryCellsInfo(Teams team)
        {
            int territory;
            bool[,] evaluated = new bool[Field.Height, Field.Width];
            //まず情報を保持
            List<(List<(ICell iCell, Point point)> enclosingCellsInfo, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)> list
                = new List<(List<(ICell iCell, Point point)>, List<(ICell iCell, Point point)> enclosedCellsInfo, int territory)>();

            //すべての領域を探索
            for (int i = 0; i < Field.Width; i++)
            {
                for (int j = 0; j < Field.Height; j++)
                {
                    //まだ評価していない領域点があれば
                    if (Field.GetCell(i, j).GetState(team) == CellState.InRegion && evaluated[j, i] == false)
                    {
                        territory = 0;
                        List<(ICell, Point)> listEnclosing = new List<(ICell, Point)>();
                        List<(ICell, Point)> listEnclosed = new List<(ICell, Point)>();
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, j, i);
                        //領域の情報を追加
                        list.Add((listEnclosing, listEnclosed, territory));
                    }
                }
            }

            //ある領域を形成するセルを取得＆その領域の得点を計算
            void SearchMakingTerritoryCells(ref List<(ICell, Point)> listEnclosing, ref List<(ICell, Point)> listEnclosed, int y, int x)
            {
                //マップ内かどうか
                if (!(x < 0 || y < 0 || x >= Field.Width || y >= Field.Height))
                {
                    CellState cellState = Field.GetCell(x, y).GetState(team);
                    //まだ未探索&領域セル
                    if (cellState == CellState.InRegion && evaluated[y, x] == false)
                    {
                        //探索済みにする
                        evaluated[y, x] = true;
                        //領域点をついでに計算
                        territory += (Field.GetCell(x, y).Score >= 0) ? Field.GetCell(x, y).Score : -Field.GetCell(x, y).Score;
                        //領域の情報を追加
                        listEnclosed.Add((Field.GetCell(x, y), new Point(x, y)));

                        //周りのセルを探索する
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y - 1, x);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y + 1, x);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y, x - 1);
                        SearchMakingTerritoryCells(ref listEnclosing, ref listEnclosed, y, x + 1);
                    }
                    //領域を形成するセルであるか
                    else if (cellState == CellState.Occupied)
                    {
                        listEnclosing.Add((Field.GetCell(x, y), new Point(x, y)));
                    }
                }
            }
            return list;
        }
    }
}
