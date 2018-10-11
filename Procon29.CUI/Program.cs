using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Procon29.Core;
using Procon29.Core.Taku.Algorithms;
using Procon29.Core.Extensions;

namespace Procon29.CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new CalculateExistenceProbability(3);

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            test.Dump(test.MapPossibilities(new Point(5, 5), 11, 11, 120));
            test.Dump(test.MapPossibilities(new Point(5, 5), 11, 11, 0));
            sw.Stop();



            Console.WriteLine("■処理Aにかかった時間");
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"　{ts}");
            Console.WriteLine($"　{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
            Console.WriteLine($"　{sw.ElapsedMilliseconds}ミリ秒");

            Console.Read();
        }
    }
}
