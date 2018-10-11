using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Procon29.Core.Helper
{
    public static class FieldUtils
    {
        /// <summary>
        /// 左上のみのフィールド情報を展開してシンメトリなフィールド変換します
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static ICell[,] ToSymmetry(ICell[,] cells, int width, int height)
        {
            int halfw = (int)Math.Ceiling(width / 2.0);
            int halfh = (int)Math.Ceiling(height / 2.0);

            int baseHeight = cells.GetLength(0);
            int baseWidth = cells.GetLength(1);

            ICell[,] field = new ICell[height, width];

            for (int y = 0; baseHeight > y; y++)
            {
                for (int x = 0; baseWidth > x; x++)
                {
                    field[y, x] = cells[y, x];
                }
            }

            for (int y = 0; height > y; y++)
            {
                for (int x = 0; width > x; x++)
                {
                    if (x >= halfw && y >= halfh)
                    {
                        field[y, x] = field[height - y - 1, width - x - 1].Clone(true, false, false);
                    }
                    else if (x >= halfw)
                    {
                        field[y, x] = field[y, width - x - 1].Clone(true, false, false);
                    }
                    else if (y >= halfh)
                    {
                        field[y, x] = field[height - y - 1, x].Clone(true, false, false);
                    }
                }
            }

            return field;
        }

        public static IEnumerable<ICell> GetEnclosedCells(ICell[,] map, int x, int y)
        {
            ICell[] cells = new ICell[8];
            int index = 0;
            int height = map.GetLength(0);
            int width = map.GetLength(1);

            for (int i = -1; 1 >= i; i++)
            {
                for (int j = -1; 1 >= j; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        int yind = y + i;
                        int xind = x + j;

                        if (yind >= 0 && xind >= 0 && height > yind && width > xind)
                        {
                            cells[index++] = map[yind, xind];
                        }
                    }
                }
            }

            return cells.Where(c => c != null);
        }

        public static Point GetPoint(ICell[,] map, ICell cell)
        {
            int height = map.GetLength(0);
            int width = map.GetLength(1);

            for (int y = 0;height > y; y++)
            {
                for(int x = 0;width > x; x++)
                {
                    if (map[y, x] == cell)
                        return new Point(x, y);
                }
            }

            throw new Exception();
        }

        /// <summary>
        /// 塗りつぶしアルゴリズムを利用して、指定されたチームのセル状態を用いて塗りつぶしを行います
        /// </summary>
        /// <param name="map">探索する2次元配列</param>
        /// <param name="point">探索を開始する配列上の座標</param>
        /// <param name="predicate">探索中の<typeparamref name="T"/>型のデータと座標データから塗りつぶすかどうかを選択する関数</param>
        /// <param name="directions">探索可能な方向を指定します。上下左右及び斜めの8方向に動くことが可能です</param>
        public static ICell[,] FillField(Field field, Point point, Teams team, Func<CellState, Point, bool> predicate, IEnumerable<Direction> directions)
        {
            return FillCells(field.Map, point, team, predicate, directions);
        }

        /// <summary>
        /// 塗りつぶしアルゴリズムを利用して、指定されたチームのセル状態を用いて塗りつぶしを行います
        /// </summary>
        /// <param name="map">探索する2次元配列</param>
        /// <param name="point">探索を開始する配列上の座標</param>
        /// <param name="predicate">探索中の<typeparamref name="T"/>型のデータと座標データから塗りつぶすかどうかを選択する関数</param>
        /// <param name="directions">探索可能な方向を指定します。上下左右及び斜めの8方向に動くことが可能です</param>
        public static ICell[,] FillCells(ICell[,] map, Point point, Teams team, Func<CellState, Point, bool> predicate, IEnumerable<Direction> directions)
        {
            if (team == (Teams.Team1 | Teams.Team2)) throw new NotSupportedException("両チーム同時に判定は行えません");

            Func<ICell, CellState> stateFunc = null;

            switch (team)
            {
                case Teams.Team1:
                    stateFunc = c => c.State1;
                    break;
                case Teams.Team2:
                    stateFunc = c => c.State2;
                    break;
            }

            return Fill(map, point, (cell, p) => predicate(stateFunc(cell), p), directions);
        }

        /// <summary>
        /// 塗りつぶしアルゴリズムを利用して、データを指定された点から探索して返却します
        /// </summary>
        /// <param name="map">探索する2次元配列</param>
        /// <param name="point">探索を開始する配列上の座標</param>
        /// <param name="predicate">探索中の<typeparamref name="T"/>型のデータと座標データから塗りつぶすかどうかを選択する関数</param>
        /// <param name="directions">探索可能な方向を指定します。上下左右及び斜めの8方向に動くことが可能です</param>
        public static T[,] Fill<T>(T[,] map, Point point, Func<T, Point, bool> predicate, IEnumerable<Direction> directions)
        {
            return Fill(map, point, predicate, directions, null, null);
        }

        private static T[,] Fill<T>(T[,] map, Point point, Func<T, Point, bool> predicate, IEnumerable<Direction> directions, T[,] filled = null, bool[,] check = null)
        {
            if (filled == null)
            {
                filled = new T[map.GetLength(0), map.GetLength(1)];
                check = new bool[map.GetLength(0), map.GetLength(1)];
            }

            filled[point.Y, point.X] = map[point.Y, point.X];
            check[point.Y, point.X] = true;

            //左方向
            if (point.X > 0 && !check[point.Y, point.X - 1] && directions.Contains(Direction.Left) && predicate(map[point.Y, point.X - 1], new Point(point.X - 1, point.Y)))
                Fill(map, new Point(point.X - 1, point.Y), predicate, directions, filled, check);

            //右方向
            if (point.X < map.GetLength(1) - 1 && !check[point.Y, point.X + 1] && directions.Contains(Direction.Right) && predicate(map[point.Y, point.X + 1], new Point(point.X + 1, point.Y)))
                Fill(map, new Point(point.X + 1, point.Y), predicate, directions, filled, check);

            //上方向
            if (point.Y > 0 && !check[point.Y - 1, point.X] && directions.Contains(Direction.Up) && predicate(map[point.Y - 1, point.X], new Point(point.X, point.Y - 1)))
                Fill(map, new Point(point.X, point.Y - 1), predicate, directions, filled, check);

            //下方向
            if (point.Y < map.GetLength(0) - 1 && !check[point.Y + 1, point.X] && directions.Contains(Direction.Down) && predicate(map[point.Y + 1, point.X], new Point(point.X, point.Y + 1)))
                Fill(map, new Point(point.X, point.Y + 1), predicate, directions, filled, check);

            //左下方向
            if (point.X > 0 && point.Y < map.GetLength(0) - 1 && !check[point.Y + 1, point.X - 1] && directions.Contains(Direction.Left | Direction.Down) && predicate(map[point.Y + 1, point.X - 1], new Point(point.X - 1, point.Y + 1)))
                Fill(map, new Point(point.X - 1, point.Y + 1), predicate, directions, filled, check);

            //左上方向
            if (point.X > 0 && point.Y > 0 && !check[point.Y - 1, point.X - 1] && directions.Contains(Direction.Left | Direction.Up) && predicate(map[point.Y - 1, point.X - 1], new Point(point.X - 1, point.Y - 1)))
                Fill(map, new Point(point.X - 1, point.Y - 1), predicate, directions, filled, check);

            //右下方向
            if (point.X < map.GetLength(1) - 1 && point.Y < map.GetLength(0) - 1 && !check[point.Y + 1, point.X + 1] && directions.Contains(Direction.Right | Direction.Down) && predicate(map[point.Y + 1, point.X + 1], new Point(point.X + 1, point.Y + 1)))
                Fill(map, new Point(point.X + 1, point.Y + 1), predicate, directions, filled, check);

            //右上方向
            if (point.X < map.GetLength(1) - 1 && point.Y > 0 && !check[point.Y - 1, point.X + 1] && directions.Contains(Direction.Right | Direction.Up) && predicate(map[point.Y - 1, point.X + 1], new Point(point.X + 1, point.Y - 1)))
                Fill(map, new Point(point.X + 1, point.Y - 1), predicate, directions, filled, check);

            return filled;

        }
    }
}
