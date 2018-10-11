using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Procon29.Core.Helper
{
    /// <summary>
    /// 処理にかかる時間を連続で計測して割合をダンプするクラス
    /// Release構成ではすべての機能を無効化するため注意してください
    /// </summary>
    public class LoadDumper
    {
#if DEBUG
        private Stopwatch Stopwatch { get; } = new Stopwatch();

        private Dictionary<string, TimeSpan> Records { get; } = new Dictionary<string, TimeSpan>();

        private object lockObj { get; } = new object();

        public bool IsRecording => Stopwatch.IsRunning;

        private string CurrentLabel { get; set; }

        public TimeSpan TotalElapsed { get; private set; } = TimeSpan.Zero;
#endif

        /// <summary>
        /// 計測を開始する関数。既存の計測がある場合はそれを停止して記録してから開始します
        /// </summary>
        /// <param name="label">計測に使用するラベル。既存のラベルと同名のラベルを使用した場合、既存の計測時間に加算されます</param>
        public void Record(string label)
        {
#if DEBUG
            lock (lockObj)
            {
                if (IsRecording)
                {
                    Stopwatch.Stop();
                    var elapsed = Stopwatch.Elapsed;
                    TotalElapsed += elapsed;

                    if (Records.ContainsKey(CurrentLabel))
                        Records[CurrentLabel] += elapsed;
                    else
                        Records[CurrentLabel] = elapsed;

                    CurrentLabel = null;
                    Stopwatch.Reset();
                }

                CurrentLabel = label;
                Stopwatch.Start();
            }
#endif
        }

        /// <summary>
        /// 計測を停止します。既存の計測は記録されます
        /// </summary>
        public void Stop()
        {
#if DEBUG
            lock (lockObj)
            {
                if (IsRecording)
                {
                    Stopwatch.Stop();
                    var elapsed = Stopwatch.Elapsed;
                    TotalElapsed += elapsed;

                    if (Records.ContainsKey(CurrentLabel))
                        Records[CurrentLabel] += elapsed;
                    else
                        Records[CurrentLabel] = elapsed;

                    CurrentLabel = null;
                    Stopwatch.Reset();
                }
            }
#endif
        }

        /// <summary>
        /// 計測を停止し、記録をすべて削除します
        /// </summary>
        public void Reset()
        {
#if DEBUG
            Stop();
            lock (lockObj)
            {
                TotalElapsed = TimeSpan.Zero;
                Records.Clear();
            }
#endif
        }

        /// <summary>
        /// コンソールへ計測結果を出力します
        /// </summary>
        public void Dump()
        {
#if DEBUG
            lock (lockObj)
            {
                foreach (var pair in Records)
                {
                    var perc = pair.Value.TotalMilliseconds / TotalElapsed.TotalMilliseconds * 100;
                    Console.WriteLine($"LoadDumper : {pair.Key ?? "ラベル無し"} : {Math.Round(perc, 1)}%({pair.Value.TotalMilliseconds}ms)");
                }
                Console.WriteLine($"LoadDumper : 総処理時間 : {TotalElapsed}");
            }
#endif
        }
    }
}
