using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Procon29
{
    /// <summary>
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window
    {
        class LogWriter : System.IO.TextWriter
        {
            public LogWriter(System.IO.TextWriter defaultOut)
            {
                DefaultOut = defaultOut;
            }

            private System.IO.TextWriter DefaultOut { get; }

            public event Action<char> WriteChar;

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                WriteChar?.Invoke(value);
                DefaultOut?.Write(value);
            }
        }

        private LogWriter Output { get; } = new LogWriter(Console.Out);

        private System.IO.TextWriter DefaultOut { get; } = Console.Out;

        private int UIThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

        private bool AutoScroll { get; set; } = true;

        public LogWindow()
        {
            InitializeComponent();
            scroll.ScrollChanged += Scroll_ScrollChanged;
            Output.WriteChar += Output_WriteChar;
            Console.SetOut(Output);
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            AutoScroll = e.VerticalOffset == scroll.ScrollableHeight;
        }

        private void Output_WriteChar(char obj)
        {
            if (UIThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                Append(obj);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => Append(obj)));
            }
        }

        private void Append(char obj)
        {
            text.Text += obj;
            if (AutoScroll)
            {
                scroll.ScrollChanged -= Scroll_ScrollChanged;
                scroll.ScrollToEnd();
                scroll.ScrollChanged += Scroll_ScrollChanged;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Console.SetOut(DefaultOut);
            Output.Dispose();
        }
    }
}
