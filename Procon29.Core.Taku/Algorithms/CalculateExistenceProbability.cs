using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Extensions;


namespace Procon29.Core.Algorithms
{
    /// <summary>
    /// エージェンが、９通り（＝移動しない＋八方位に移動する)　の行動をどれも等しく行うと考えた場合に、
    /// そのエージェントが指定したターン後に指定したセルにいる確率を計算するクラス
    /// 
    /// アルゴリズムのスコアの推定に用いるために用意
    /// 
    /// 注意点として、turn >=21の場合、誤差が出ます。
    /// 何故かと言うと、totalPatternがulongよりも大きな値となってしまい、オーバーフローしてしまうので、変数をdoubleにして計算するからです。
    /// 
    /// </summary>
    public class CalculateExistenceProbability
    {
        public int Width { get; private set; }
        private int _Turn;
        public int Turn
        {
            get => _Turn;
            private set
            {
                if (value >= 160)//大きすぎると、doubleでも表現しきれなくなってしまうので例外を発生
                    throw new Exception();
                else
                    _Turn = value;
            }
        }

        public ulong[,] Result { get; private set; }
        public double[,] ResultDouble { get; private set; } //ulongでオーバーフローが発生する場合

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="turn">何ターン後のエージェントの存在パターンを考えるか</param>
        public CalculateExistenceProbability(int turn)
        {
            Turn = turn;
            Width = 2 * turn + 1;
            Result = new ulong[Width, Width];
            ResultDouble = new double[Width, Width];
        }

        /// <summary>
        /// エージェントのいる位置を配列の中心(== Result[Turn,Turn])としたときの存在パターンを求めるメソッド。
        /// </summary>
        /// <returns>エージェントのいる位置を配列の中心(== Result[Turn,Turn])としたときの存在パターン数を保持した２次配列</returns>
        private ulong[,] Calculate()
        {
            checked
            {
                int a, b;
                ulong PatternA, PatternB;

                //各場所の存在確率を計算
                for (int i = 0; i <= Turn; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        //相対位置
                        a = i - Turn;
                        b = j - Turn;

                        PatternA = PatternB = 0;

                        if (a >= 0)
                        {
                            if ((Turn - a) % 2 == 0)
                            {
                                for (int k = 0; k <= Turn - a; k += 2)
                                    PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                            }
                            else
                            {
                                for (int k = 1; k <= Turn - a; k += 2)
                                    PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                            }
                        }
                        else
                        {
                            if ((Turn + a) % 2 == 0)
                            {

                                for (int k = 0; k <= Turn + a; k += 2)
                                    PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                            }
                            else
                            {
                                for (int k = 1; k <= Turn + a; k += 2)
                                    PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                            }
                        }

                        if (b >= 0)
                        {
                            if ((Turn - b) % 2 == 0)
                            {
                                for (int k = 0; k <= Turn - b; k += 2)
                                    PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                            }
                            else
                            {
                                for (int k = 1; k <= Turn - b; k += 2)
                                    PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                            }
                        }
                        else
                        {
                            if ((Turn + b) % 2 == 0)
                            {
                                for (int k = 0; k <= Turn + b; k += 2)
                                    PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                            }
                            else
                            {
                                for (int k = 1; k <= Turn + b; k += 2)
                                    PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                            }
                        }


                        Result[j, Turn * 2 - i] = Result[Turn * 2 - j, i] = Result[j, i] = Result[Turn * 2 - j, Turn * 2 - i] =
                            Result[i, Turn * 2 - j] = Result[Turn * 2 - i, j] = Result[i, j] = Result[Turn * 2 - i, Turn * 2 - j]
                            = PatternA * PatternB;
                    }
                }
                return Result;
            }
        }

        /// <summary>
        /// エージェントのいる位置を配列の中心(== Result[Turn,Turn])としたときの存在パターンを求めるメソッド。
        /// ulongだとオーバーフローが発生してしまう場合にdoubleで代用
        /// </summary>
        /// <returns>エージェントのいる位置を配列の中心(== Result[Turn,Turn])としたときの存在パターン数を保持した２次配列</returns>
        private double[,] CalculateDouble()
        {
            double a, b;
            double PatternA, PatternB;

            //各場所の存在確率を計算
            for (int i = 0; i <= Turn; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    //相対位置
                    a = i - Turn;
                    b = j - Turn;

                    PatternA = PatternB = 0;

                    if (a >= 0)
                    {
                        if ((Turn - a) % 2 == 0)
                        {
                            for (int k = 0; k <= Turn - a; k += 2)
                                PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                        }
                        else
                        {
                            for (int k = 1; k <= Turn - a; k += 2)
                                PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                        }
                    }
                    else
                    {
                        if ((Turn + a) % 2 == 0)
                        {

                            for (int k = 0; k <= Turn + a; k += 2)
                                PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                        }
                        else
                        {
                            for (int k = 1; k <= Turn + a; k += 2)
                                PatternA += HomogeneousProduct(k, (Turn - a - k) / 2, (Turn + a - k) / 2);
                        }
                    }

                    if (b >= 0)
                    {
                        if ((Turn - b) % 2 == 0)
                        {
                            for (int k = 0; k <= Turn - b; k += 2)
                                PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                        }
                        else
                        {
                            for (int k = 1; k <= Turn - b; k += 2)
                                PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                        }
                    }
                    else
                    {
                        if ((Turn + b) % 2 == 0)
                        {
                            for (int k = 0; k <= Turn + b; k += 2)
                                PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                        }
                        else
                        {
                            for (int k = 1; k <= Turn + b; k += 2)
                                PatternB += HomogeneousProduct(k, (Turn - b - k) / 2, (Turn + b - k) / 2);
                        }
                    }


                    ResultDouble[j, Turn * 2 - i] = ResultDouble[Turn * 2 - j, i] = ResultDouble[j, i] = ResultDouble[Turn * 2 - j, Turn * 2 - i] =
                        ResultDouble[i, Turn * 2 - j] = ResultDouble[Turn * 2 - i, j] = ResultDouble[i, j] = ResultDouble[Turn * 2 - i, Turn * 2 - j]
                        = PatternA * PatternB;
                }
            }
            return ResultDouble;
        }

        /// <summary>
        /// エージェントの位置と、盤面の大きさを指定し、盤面のあるマスにそのエージェントが存在するパターンを計算する
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <returns></returns>
        private ulong[,] MapPatterns(Point pos, int width, int height)
        {
            ulong[,] Map = new ulong[height, width];

            Point q;
            Point complement = new Point(pos.X - Turn, pos.Y - Turn);

            Calculate();

            Map.ForEach((p, u) =>
            {
                q = p - complement;
                //qが配列Resultの存在内か
                if ((q.X >= 0 && q.Y >= 0 && q.X < Width && q.Y < Width))
                {
                    Map[p.Y, p.X] = Result[q.Y, q.X];
                }
                else
                {
                    Map[p.Y, p.X] = 0; //指定したターン数では絶対にエージェントがpointにいることはない
                }
            });

            return Map;
        }
        /// <summary>
        /// エージェントの位置と、盤面の大きさ、ターン数を指定し、盤面のあるマスにそのエージェントが存在するパターンを計算する
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <param name="turn">何ターン後のエージェントの存在パターンを考えるか</param>
        /// <returns></returns>
        private ulong[,] MapPatterns(Point pos, int width, int height, int turn)
        {
            Turn = turn;
            Width = 2 * turn + 1;
            Result = new ulong[Width, Width];

            return MapPatterns(pos, width, height);
        }

        /// <summary>
        /// エージェントの位置と、盤面の大きさを指定し、盤面のあるマスにそのエージェントが存在するパターンを計算する
        /// ulongだとオーバーフローが発生してしまうのでdoubleで代用
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <returns></returns>
        private double[,] MapPatternsDouble(Point pos, int width, int height)
        {
            double[,] Map = new double[height, width];

            Point q;
            Point complement = new Point(pos.X - Turn, pos.Y - Turn);

            CalculateDouble();

            Map.ForEach((p, u) =>
            {
                q = p - complement;
                //qが配列Resultの存在内か
                if ((q.X >= 0 && q.Y >= 0 && q.X < Width && q.Y < Width))
                {
                    Map[p.Y, p.X] = ResultDouble[q.Y, q.X];
                }
                else
                {
                    Map[p.Y, p.X] = 0.0; //指定したターン数では絶対にエージェントがpointにいることはない
                }
            });

            return Map;
        }

        /// <summary>
        /// エージェントの位置と、盤面の大きさ、ターン数を指定し、盤面のあるマスにそのエージェントが存在するパターンを計算する
        ///  ulongだとオーバーフローが発生してしまう場合にdoubleで代用するためのメソッド
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <param name="turn">何ターン後のエージェントの存在パターンを考えるか</param>
        /// <returns></returns>
        private double[,] MapPatternsDouble(Point pos, int width, int height, int turn)
        {
            Turn = turn;
            Width = 2 * turn + 1;
            ResultDouble = new double[Width, Width];

            return MapPatternsDouble(pos, width, height);
        }

        /// <summary>
        /// エージェントの位置と、盤面の大きさを指定し、盤面のあるマスにそのエージェントが存在する確率を計算する
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <returns></returns>
        public double[,] MapPossibilities(Point pos, int width, int height)
        {
            //Turn数が大きいとき(Turn >= 21)は、ulongだとオーバーフローするので、doubleで代用
            if (Turn <= 20)
            {
                //ulongでオーバーフローしないケース
                ulong totalPattern = 0;
                ulong[,] Mapping = MapPatterns(pos, width, height);
                double[,] MappingPossibilities = new double[height, width];


                //全パターン数を計算
                Mapping.ForEach((p, ul) => totalPattern += ul);
                //確率をセットしていく
                Mapping.ForEach((p, ul) => MappingPossibilities[p.Y, p.X] = (double)ul / totalPattern);

                return MappingPossibilities;
            }
            else
            {
                //ulongだとオーバーフローしてしまうケース
                double totalPattern = 0.0, error = 0.0, before;
                double[,] Mapping = MapPatternsDouble(pos, width, height);
                double[,] MappingPossibilities = new double[height, width];


                //全パターン数を計算、蓄積誤差をできるだけ抑えるためにカハンのアルゴリズムを使用
                Mapping.ForEach((p, ul) =>
                {
                    before = totalPattern;                   //一つ前を保持 
                    totalPattern += ul + error;             //蓄積誤差を含めて加算
                    error = (ul + error) - (totalPattern - before);//次の蓄積誤差を求める
                });
                //確率をセットしていく
                Mapping.ForEach((p, ul) => MappingPossibilities[p.Y, p.X] = ul / totalPattern);

                return MappingPossibilities;
            }

        }

        /// <summary>
        /// エージェントの位置と、盤面の大きさ、ターン数を指定し、盤面のあるマスにそのエージェントが存在する確率を計算する
        /// </summary>
        /// <param name="pos">エージェントの位置</param>
        /// <param name="width">マップの幅</param>
        /// <param name="height">マップの高さ</param>
        /// <param name="turn">何ターン後のエージェントの存在パターンを考えるか</param>
        /// <returns></returns>
        public double[,] MapPossibilities(Point pos, int width, int height, int turn)
        {
            Turn = turn;
            Width = 2 * turn + 1;
            if (Turn >= 21)
                ResultDouble = new double[Width, Width];
            else
                Result = new ulong[Width, Width];

            return MapPossibilities(pos, width, height);
        }

        ///二人のエージェントの位置から盤面のあるマスにそのエージェントたちが存在する確率を求める
        public double[,] MapPossibilities(Point pos1,Point pos2, int width, int height, int turn)
        {
            Turn = turn;
            Width = 2 * turn + 1;
            Result = new ulong[Width, Width];
            ResultDouble = new double[Width, Width];

            /*
            double[,] Result1 = MapPatternsDouble(pos1, width, height);
            double[,] Result2 = MapPatternsDouble(pos2, width, height);

            //２つの存在パターン
            Result1.ForEach((p, d) => Result2[p.Y, p.X] += d);

            double totalPattern = 0.0, error = 0.0, before;
            double[,] MappingPossibilities = new double[height, width];


            //全パターン数を計算、蓄積誤差をできるだけ抑えるためにカハンのアルゴリズムを使用
            Result2.ForEach((p, ul) =>
            {
                before = totalPattern;                   //一つ前を保持 
                totalPattern += ul + error;             //蓄積誤差を含めて加算
                error = (ul + error) - (totalPattern - before);//次の蓄積誤差を求める
            });
            //確率をセットしていく
            Result2.ForEach((p, ul) => MappingPossibilities[p.Y, p.X] = ul / totalPattern);

            */
            double[,] Result1 = MapPossibilities(pos1, width, height);
            double[,] Result2 = MapPossibilities(pos2, width, height);

            //２つの存在パターンを合成
            Result1.ForEach((p, d) => Result2[p.Y, p.X] += d);

            return Result2;
        }

        /// <summary>
        /// 配列を表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public void Dump<T>(T[,] array)
        {
            array.ForEach((p, i) => Console.Write("{0,8}", i), (i) => Console.WriteLine());
            Console.WriteLine();
        }

        /// <summary>
        /// double型配列を表示
        /// </summary>
        /// <param name="array"></param>
        public void Dump(double[,] array)
        {
            double totalPossibility = 0.0;
            array.ForEach((p, i) =>
            {
                Console.Write("{0,8:F5}", i);
                totalPossibility += i;
            }, (i) => Console.WriteLine());
            Console.WriteLine("totalPossibility:{0}\n", totalPossibility);
        }

        //同じものを含む順列を計算(p,q,r =<20まで、それ以上は値がおおきくなりすぎてオーバーフロー可能性が非常に高い)
        public ulong HomogeneousProduct(int p, int q, int r)
        {
            checked
            {
                return Combination(p + q + r, p) * Combination(q + r, r);
            }
        }


        //組み合わせを計算
        public ulong Combination(int n, int r)
        {
            checked
            {
                ulong upper = 1, down = 1;
                while (r > 0)
                {
                    upper *= (ulong)(n - r + 1);
                    down *= (ulong)r;
                    r--;
                }
                return upper / down;
            }
        }

        //同じものを含む順列を計算(数が大きすぎるもののため)
        public double HomogeneousProduct(double p, double q, double r)
        {
            checked
            {
                return Combination(p + q + r, p) * Combination(q + r, r);
            }
        }

        //組み合わせを計算(数が大きすぎるもののため)
        public double Combination(double n, double r)
        {
            checked
            {
                double upper = 1, down = 1,top = r;
                while (r > 0)
                {
                    upper *= n - r + 1;
                    down *= top -r + 1;
                    r--;
                }
                return upper / down;
            }
        }
    }
}
