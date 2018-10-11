using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Procon29.Core.Extensions;

namespace Procon29.Core
{
    /// <summary>
    /// ジェネレータによって生成されるセルを管理するクラス
    /// </summary>
    public class Field
    {
        //評価中にプロパティ変更を無視するフラグ
        private bool _ignoreUpdate = false;

        public Field(FieldGenerators.IFieldGenerator fieldGenerator)
        {
            Map = fieldGenerator.Generate();
            Width = fieldGenerator.Width;
            Height = fieldGenerator.Height;

            MapForEach((p, cell) => cell.UnsafePropertyChanged += Cell_PropertyChanged);
        }

        public Field(ICell[,] field)
        {
            Map = field;
            Height = field.GetLength(0);
            Width = field.GetLength(1);

            MapForEach((p, cell) => cell.UnsafePropertyChanged += Cell_PropertyChanged);
        }

        /// <summary>
        /// クローン生成用のコンストラクタ
        /// </summary>
        /// <param name="field"></param>
        /// <param name="priority">セルの優先順位をコピーするかどうか。true:する</param>
        /// <param name="state1">team1のセル情報をコピーするかどうか。true:する</param>
        /// <param name="state2">team2のセル情報をコピーするかどうか。true:する</param>
        private Field(Field field, bool priority, bool state1, bool state2)
        {
            Map = new ICell[field.Height, field.Width];

            field.Map.ForEach((p, c) =>
            {
                Map[p.Y, p.X] = c.Clone(priority, state1, state2);
            });

            Width = field.Width;
            Height = field.Height;

            MapForEach((p, cell) => cell.UnsafePropertyChanged += Cell_PropertyChanged);
        }

        private void Cell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (AutoEvaluate)
            {
                EvaluateMap(Teams.Team1);
                EvaluateMap(Teams.Team2);
            }
        }

        /// <summary>
        /// 得点が評価された際に発生します。別スレッドの可能性があるのでUIスレッドでは注意してください
        /// </summary>
        public event EventHandler Evaluated;

        /// <summary>
        /// マップの横幅
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// マップの縦幅
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// マップのデータ
        /// </summary>
        public ICell[,] Map { get; }

        /// <summary>
        /// Team1に対する点数
        /// </summary>
        public int Score1 { get; private set; }

        /// <summary>
        /// Team2に対する点数
        /// </summary>
        public int Score2 { get; private set; }

        /// <summary>
        /// セルのプロパティが変更された際に点数を自動で評価するかを指定します。
        /// </summary>
        public bool AutoEvaluate { get; set; } = true;

        /// <summary>
        /// 評価された際にコンソール上に結果を出力するかを指定します
        /// </summary>
        public bool AutoDump { get; set; } = false;

        //(x,y)にあるセルをそのセルのインターフェースで返す
        public ICell GetCell(int x, int y)
        {
            return Map[y, x];
        }

        //positionにあるセルをそのセルのインターフェースで返す
        public ICell GetCell(Point position)
        {
            return GetCell(position.X, position.Y);
        }

        /// <summary>
        /// マップを評価し、指定されたチームの得点を計算します。<see cref="AutoEvaluate"/>がfalseの場合はエージェントが移動した際に手動で呼び出す必要があります。
        /// 計算はほとんど時間がかからない(10ms未満)なので躊躇せず読んであげてね☆
        /// </summary>
        /// <param name="team">チーム</param>
        /// <returns></returns>
        public int EvaluateMap(Teams team)
        {
            if ((int)team > 2) throw new NotSupportedException("複数フラグによる評価は対応していません。片方のチームのみで評価してください");

            if (_ignoreUpdate) return team == Teams.Team1 ? Score1 : Score2;

            _ignoreUpdate = true;
            int x = 0, y = 0;
            
            //マップからチームのタイルポイントを計算します
            var tile = Map.Cast<ICell>().Where((c, i) =>
            {
                y = i / Width;
                x = i - y * Width;

                //ついでに領域フラグ削除
                if (c.GetState(team).HasFlag(CellState.InRegion))
                    c.SetState(team, CellState.None);

                if (c.GetState(team).HasFlag(CellState.Occupied))
                    return true;

                return false;
            }).Sum(c => ((ScoreCell)c).Score);

            //領域点を計算します（塗りつぶしアルゴリズムを応用）

            //評価点を格納する
            int[,] evaluated = new int[Height, Width];

            evaluated.ForEach((point, _) =>
            {
                //未判定（評価点0）の場合
                if (evaluated[point.Y, point.X] == 0)
                {
                    var fillp = new Point(point.X, point.Y);
                    var fills = GetCell(fillp).GetState(team);
                    var filled = Helper.FieldUtils.FillField(this, fillp, team, 
                        (state1, p) => state1 == fills, new Direction[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down });

                    bool containsEdge = false;
                    filled.ForEach((p, c) =>
                    {
                        if (c != null)
                        {
                            if (Map.IsEdge(p.X, p.Y))
                            {
                                //端と当たった場合は領域ではない
                                containsEdge = true;
                            }

                            //評価点を加算してそのセルを評価済みにする
                            evaluated[p.Y, p.X]++;
                        }
                    });

                    if (!containsEdge)
                    {
                        filled.ForEach((p, c) =>
                        {
                            //占有されたセルは領域ではないので除外
                            if (c != null && !c.GetState(team).HasFlag(CellState.Occupied))
                                evaluated[p.Y, p.X]++;
                        });
                    }
                }
            });

            //評価点が2の場合は領域となる
            int region = 0;
            evaluated.ForEach((p, v) =>
            {
                if (v == 2)
                {
                    var cell = Map[p.Y, p.X];
                    cell.SetState(team, CellState.InRegion);
                    region += Math.Abs(cell.Score);
                }
            });

            if (AutoDump)
                DumpMap(team);

            var sum = tile + region;

            switch (team)
            {
                case Teams.Team1:
                    Score1 = sum;
                    break;
                case Teams.Team2:
                    Score2 = sum;
                    break;
            }

            Evaluated?.Invoke(this, new EventArgs());
            _ignoreUpdate = false;

            return sum;
        }

        /// <summary>
        /// チームの評価情報をコンソールへ出力します
        /// </summary>
        public void DumpMap(Teams team)
        {
            Teams enemy = (Teams.Team1 | Teams.Team2) & ~team;

            List<string> warnings = new List<string>();
            Console.WriteLine(team);
            //評価情報を出力
            MapForEach((p, c) =>
            {
                var state = c.GetState(team);

                char console;
                switch (state)
                {
                    case CellState.Occupied:
                        console = '＊';
                        break;
                    case CellState.InRegion:
                        console = '○';
                        break;
                    default:
                        console = '―';
                        break;
                }

                //http://www.procon.gr.jp/wp-content/uploads//2018/05/FAQ_Q1-Q202.pdf の5ページ目より、占有された場所は領域点ではない
                if (state.HasFlag(CellState.Occupied | CellState.InRegion))
                    warnings.Add($"Invalid state : Occupied and InRegion flags was set({p}, {state})");

                //Team1とTeam2が同時に占有することはできない
                if (state.HasFlag(CellState.Occupied) && c.GetState(enemy).HasFlag(CellState.Occupied))
                    warnings.Add($"Invalid state : Both teams set Occupied flag({p}, {state})");

                Console.Write(console);

                if (p.X == Width - 1) Console.WriteLine();
            });

            if (warnings.Count > 0)
            {
                Console.WriteLine("------Warning------");
                Console.WriteLine(Environment.StackTrace);

                foreach (var line in warnings)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("------End------");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// <see cref="Map"/>をループし、各ループごとに指定したデリゲートを実行します
        /// </summary>
        /// <param name="action"></param>
        private void MapForEach(Action<Point, ICell> action, Action<int> lineChanged = null, bool vertical = false)
        {
            Map.ForEach(action, lineChanged, vertical);
        }
        
        /// <param name="priority">セルの優先順位をコピーするかどうか。true:する</param>
        /// <param name="state1">team1のセル情報をコピーするかどうか。true:する</param>
        /// <param name="state2">team2のセル情報をコピーするかどうか。true:する</param>
        public Field Clone(bool priority, bool state1, bool state2)
        {
            return new Field(this, priority, state1, state2);
        }
    }
}
