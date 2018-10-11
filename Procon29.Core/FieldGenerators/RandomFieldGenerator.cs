using Procon29.Core.Algorithms;
using System;
using System.Linq;

namespace Procon29.Core.FieldGenerators
{
    /// <summary>
    /// ランダムなフィールを生成するクラス
    /// </summary>
    public class RandomFieldGenerator : IFieldGenerator
    {
        public RandomFieldGenerator(int width, int height, IRandom random)
        {
            Width = width;
            Height = height;
            Random = random;
        }

        public RandomFieldGenerator(int width, int height) : this(width, height, new DefaultRandom()) { }

        public IRandom Random { get; }

        public int Width { get; }

        public int Height { get; }

        public double NegativeRatio { get; } = 0.1;

        public ICell[,] Generate()
        {
            int halfw = (int)Math.Ceiling(Width / 2.0);
            int halfh = (int)Math.Ceiling(Height / 2.0);
            ICell[,] field = new ICell[halfh, halfw];

            //TODO ランダムがいい感じになるようにする
            for (int y = 0; halfh > y; y++)
            {
                for (int x = 0; halfw > x; x++)
                {
                    field[y, x] = new ScoreCell(Random.Next(1, Rule.MaximumCellScore + 1));
                }
            }
            
            //負の数を割り当てる
            int negCount = (int)(field.Length * NegativeRatio);
            (int, int)[] hist = new (int, int)[negCount];

            //無視する値を代入しないと(0, 0)になりwhileで(0, 0)に代入されないため
            int ignore = halfh + 1;

            for (int i = 0; negCount > i; i++)
                hist[i] = (ignore, ignore);

            for (int i = 0; negCount > i; i++)
            {
                int y = ignore, x = ignore;

                do
                {
                    y = Random.Next(0, halfh);
                    x = Random.Next(0, halfw);
                } while (hist.Contains((y, x)));

                hist[i] = (y, x);
                field[y, x] = new ScoreCell(Random.Next(Rule.MinimumCellScore, 1));
            }

            return Helper.FieldUtils.ToSymmetry(field, Width, Height);
        }
    }
}
