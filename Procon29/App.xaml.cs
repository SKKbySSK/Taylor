using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Procon29
{
    class STATask
    {
        public static Task Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task Run(Action act)
        {
            return Run(() =>
            {
                act();
                return true;
            });
        }
    }

    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static LogWindow LogWindow { get; private set; }

        public static Dispatcher LogDispatcher { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Core.Abstracts.SafeNotifyPropertyChanged.GlobalContext = System.Threading.SynchronizationContext.Current;
            Core.Taku.Registerer.Register();
            Core.Gimo.Registerer.Register();

            STATask.Run(() =>
            {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(
                        Dispatcher.CurrentDispatcher));
                LogDispatcher = Dispatcher.CurrentDispatcher;

                LogWindow = new LogWindow();

                LogWindow.Closed += LogWindow_Closed;

                LogWindow.Show();
                Dispatcher.Run();
            });
        }

        private void LogWindow_Closed(object sender, EventArgs e)
        {
            LogWindow.Closed -= LogWindow_Closed;
            Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
        }
    }
}
