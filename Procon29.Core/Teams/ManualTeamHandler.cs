using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Procon29.Core
{
    /// <summary>
    /// 手動で動かすチーム。（内部実装が難しいと思うので見ない方がいいです）
    /// </summary>
    public class ManualTeamHandler : TeamHandlerBase
    {
        /// <summary>
        /// エージェント1が動作する必要がある際に発生します
        /// </summary>
        public event EventHandler NeedIntent1;

        /// <summary>
        /// エージェント2が動作する必要がある際に発生します
        /// </summary>
        public event EventHandler NeedIntent2;

        /// <summary>
        /// 順番が回ってきた際に発生します
        /// </summary>
        public event EventHandler TurnStarted;

        /// <summary>
        /// 順番が終了した際に発生します
        /// </summary>
        public event EventHandler TurnEnded;

        TaskCompletionSource<Intent> task1, task2;
        
        protected internal override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            InvokeOnUIThread(() => TurnStarted?.Invoke(this, new EventArgs()));

            List<Task> tasks = new List<Task>();

            task1 = null;
            task2 = null;

            if (NeedIntent1 == null)
            {
                Console.WriteLine($"{nameof(NeedIntent1)}へのハンドラが登録されていません！！");
                agent1(Intent.StayIntent);
            }
            else
            {
                task1 = new TaskCompletionSource<Intent>();
                tasks.Add(task1.Task);

                InvokeOnUIThread(() => NeedIntent1(this, new EventArgs()));
                task1.Task.Wait();
            }

            if (NeedIntent2 == null)
            {
                Console.WriteLine($"{nameof(NeedIntent2)}へのハンドラが登録されていません！！");
                agent2(Intent.StayIntent);
            }
            else
            {
                task2 = new TaskCompletionSource<Intent>();
                tasks.Add(task2.Task);

                InvokeOnUIThread(() => NeedIntent2(this, new EventArgs()));
                task2.Task.Wait();
            }

            if (task1 != null)
                agent1(task1.Task.Result);

            if (task2 != null)
                agent2(task2.Task.Result);

            InvokeOnUIThread(() => TurnEnded?.Invoke(this, new EventArgs()));
        }

        public bool SetIntent1(Intent intent) => task1?.TrySetResult(intent) ?? false;

        public bool SetIntent2(Intent intent) => task2?.TrySetResult(intent) ?? false;

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }
    }
}
