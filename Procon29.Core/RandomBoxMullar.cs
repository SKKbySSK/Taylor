using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RandomBoxMuller
{
    //一様分布から正規分布を生成するクラス
    public class RandomBoxMuller
    {
        //一様分布発生クラス
        private Random random;

        //コンストラクタ
        public RandomBoxMuller()
        {
            random = new Random(Environment.TickCount);
        }
        //コンストラクタのオーバーロード
        public RandomBoxMuller(int seed)
        {
            random = new Random(seed);
        }
        //任意整数域での乱数（正規分布）を生成
        public int Next(int min, int max, double mu = 0.0, double sigma = 1.0, bool getCos = true)
        {
            double rnd = Math.Min(Math.Max(Next(mu, sigma, getCos), 0), 1);
            return (int)(rnd * (max - min) - min);
        }

        //正規分布に従う乱数の生成
        public double Next(double mu = 0.0, double sigma = 1.0, bool getCos = true)
        {
            double rand;
            while ((rand = random.NextDouble()) == 0.0) ;
            double rand2 = random.NextDouble();

            double normrand;
            if (getCos)
            {
                //標準正規分布をボックス=ミュラー法のcosの方から生成
                normrand = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Cos(2.0 * Math.PI * rand2);
            }
            else
            {
                //標準正規分布をボックス=ミュラー法のsinの方から生成
                normrand = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Sin(2.0 * Math.PI * rand2);
            }

            //標準正規分布に従う乱数から、平均mu　分散sigma　の正規分布に従う乱数を生成
            normrand = normrand * sigma + mu;
            return normrand;
        }
        //ボックス=ミュラー法で得られる正規分布に従う乱数を返す関数
        public double[] NextPair(double mu = 0.0, double sigma = 1.0)
        {
            double[] normrand = new double[2];
            double rand = 0.0;
            while ((rand = random.NextDouble()) == 0.0) ;
            double rand2 = random.NextDouble();
            normrand[0] = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Cos(2.0 * Math.PI * rand2);
            normrand[0] = normrand[0] * sigma + mu;
            normrand[1] = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Sin(2.0 * Math.PI * rand2);
            normrand[1] = normrand[1] * sigma + mu;
            return normrand;
        }
    }
}