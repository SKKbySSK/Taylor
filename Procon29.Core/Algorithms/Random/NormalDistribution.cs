using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Algorithms
{
    public class NormalDistribution : IRandom
    {
        public NormalDistribution(int seed)
        {
            RandomBoxMuller = new RandomBoxMuller.RandomBoxMuller(seed);
        }

        public NormalDistribution()
        {
            RandomBoxMuller = new RandomBoxMuller.RandomBoxMuller();
        }

        /// <summary>
        /// ボックスミュラーにCosを用いるかSinを用いるか
        /// </summary>
        public bool UseCos { get; set; } = true;

        /// <summary>
        /// 平均
        /// </summary>
        public double Mu { get; set; } = 0;

        /// <summary>
        /// 分散
        /// </summary>
        public double Sigma { get; set; } = 1;

        private RandomBoxMuller.RandomBoxMuller RandomBoxMuller { get; }

        public int Next(int min, int max)
        {
            double rnd;

            do
            {
                rnd = RandomBoxMuller.Next(Mu, Sigma, UseCos);
            } while (max <= rnd || rnd < min);

            return (int)Math.Floor(rnd);
        }
    }
}
