using System;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Extensions
{
    public static class ArrayExtension
    {
        public static T[,] Convert<T>(this T[][] array)
        {
            var h = array.Length;
            var w = array[0].Length;

            T[,] map = new T[h, w];
            for (int y = 0; h > y; y++)
            {
                for (int x = 0; w > x; x++)
                {
                    map[y, x] = array[y][x];
                }
            }

            return map;
        }

        public static T[][] Convert<T>(this T[,] array)
        {
            var h = array.GetLength(0);
            var w = array.GetLength(1);

            T[][] map = new T[h][];
            for (int y = 0; h > y; y++)
            {
                map[y] = new T[w];
                for (int x = 0; w > x; x++)
                {
                    map[y][x] = array[y, x];
                }
            }

            return map;
        }
        
        public static T[,,] Convert<T>(this T[][][] array)
        {
            var h = array.Length;
            var w = array[0].Length;
            var d = array[0][0].Length;

            T[,,] map = new T[h, w, d];
            for (int y = 0; h > y; y++)
            {
                for (int x = 0; w > x; x++)
                {
                    for(int z = 0; d > z; z++)
                    {
                        map[y, x, z] = array[y][x][z];
                    }
                }
            }

            return map;
        }

        public static T[][][] Convert<T>(this T[,,] array)
        {
            var h = array.GetLength(0);
            var w = array.GetLength(1);
            var d = array.GetLength(2);

            T[][][] map = new T[h][][];
            for (int y = 0; h > y; y++)
            {
                map[y] = new T[w][];
                for (int x = 0; w > x; x++)
                {
                    for (int z = 0; d > z; z++)
                    {
                        map[y][x][z] = array[y, x, z];
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// 指定された配列をループし、各ループごとに指定したデリゲートを実行します
        /// </summary>
        public static void ForEach<T>(this T[,] map, Action<Point, T> action, Action<int> lineChanged = null, bool vertical = false)
        {
            ForEach(map, (p, t) =>
            {
                action(p, t);
                return true;
            }, lineChanged, vertical);
        }

        /// <summary>
        /// 指定された配列をループし、各ループごとに指定したデリゲートを実行します
        /// </summary>
        public static void ForEach<T>(this T[][] map, Action<Point, T> action, Action<int> lineChanged = null, bool vertical = false)
        {
            ForEach(map, (p, t) =>
            {
                action(p, t);
                return true;
            }, lineChanged, vertical);
        }

        /// <summary>
        /// 指定された配列をループし、各ループごとに指定したデリゲートを実行します
        /// </summary>
        /// <param name="func">ループ時の処理。falseを返すことでループを中断して次の行または列へ処理をスキップします</param>
        public static void ForEach<T>(this T[,] map, Func<Point, T, bool> func, Action<int> lineChanged = null, bool vertical = false)
        {
            var height = map.GetLength(0);
            var width = map.GetLength(1);
            int iMax = vertical ? width : height;
            int jMax = vertical ? height : width;

            for (int i = 0; iMax > i; i++)
            {
                for (int j = 0; jMax > j; j++)
                {
                    bool go;
                    if (vertical)
                        go = func(new Point(i, j), map[j, i]);
                    else
                        go = func(new Point(j, i), map[i, j]);
                    if (!go) break;
                }
                lineChanged?.Invoke(i);
            }
        }

        /// <summary>
        /// 指定された配列をループし、各ループごとに指定したデリゲートを実行します
        /// </summary>
        /// <param name="func">ループ時の処理。falseを返すことでループを中断して次の行または列へ処理をスキップします</param>
        public static void ForEach<T>(this T[][] map, Func<Point, T, bool> func, Action<int> lineChanged = null, bool vertical = false)
        {
            var height = map.Length;
            var width = map[0].Length;
            int iMax = vertical ? width : height;
            int jMax = vertical ? height : width;

            for (int i = 0; iMax > i; i++)
            {
                for (int j = 0; jMax > j; j++)
                {
                    bool go;
                    if (vertical)
                        go = func(new Point(i, j), map[j][i]);
                    else
                        go = func(new Point(j, i), map[i][j]);

                    if (!go) break;
                }
                lineChanged?.Invoke(i);
            }
        }

        /// <summary>
        /// 指定された座標が端にあるかを判定します
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsEdge(this Array map, int x, int y)
        {
            if (x == 0 || y == 0) return true;
            if (x == map.GetLength(1) - 1 || y == map.GetLength(0) - 1) return true;

            return false;
        }

        /// <summary>
        /// 指定された座標が端にあるかを判定します
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsEdge<T>(this T[][] map, int x, int y)
        {
            if (x == 0 || y == 0) return true;
            if (x == map[0].Length - 1 || y == map.GetLength(0) - 1) return true;

            return false;
        }
    }
}
