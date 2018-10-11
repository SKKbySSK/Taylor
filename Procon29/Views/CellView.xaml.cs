using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Procon29.Views
{
    public enum CellViewAgent
    {
        None,
        Team1Agent1,
        Team1Agent2,
        Team2Agent1,
        Team2Agent2,
    }

    /// <summary>
    /// CellView.xaml の相互作用ロジック
    /// </summary>
    public partial class CellView : UserControl
    {
        SimpleAnimation blinkAnim;

        public CellView()
        {
            InitializeComponent();
            AgentPropertyChanged(CellViewAgent.None, CellViewAgent.None);

            blinkAnim = new SimpleAnimation(TimeSpan.FromMilliseconds(500), d =>
            {
                if (d >= 0.5)
                {
                    blinkV.Opacity = d / 0.5 * 0.3;
                }
                else
                {
                    blinkV.Opacity = (1 - d) * 0.3;
                }
            });
            blinkAnim.Loop = true;
        }

        public static readonly DependencyProperty Team1BrushProperty = DependencyProperty.Register(nameof(Team1Brush),
            typeof(Brush), typeof(CellView),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 200, 200, 255)), (obj, e) => ((CellView)obj).Team1BrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void Team1BrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            UpdateBackground();
        }

        public Brush Team1Brush
        {
            get => (Brush)GetValue(Team1BrushProperty);
            set => SetValue(Team1BrushProperty, value);
        }

        public static readonly DependencyProperty Team2BrushProperty = DependencyProperty.Register(nameof(Team2Brush),
            typeof(Brush), typeof(CellView),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 255, 200, 200)), (obj, e) => ((CellView)obj).Team2BrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void Team2BrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            UpdateBackground();
        }

        public Brush Team2Brush
        {
            get => (Brush)GetValue(Team2BrushProperty);
            set => SetValue(Team2BrushProperty, value);
        }

        public static readonly DependencyProperty Region1BrushProperty = DependencyProperty.Register(nameof(Region1Brush),
            typeof(Brush), typeof(CellView),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(70, 200, 200, 255)), (obj, e) => ((CellView)obj).Region1BrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void Region1BrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            UpdateBackground();
        }

        public Brush Region1Brush
        {
            get => (Brush)GetValue(Region1BrushProperty);
            set => SetValue(Region1BrushProperty, value);
        }

        public static readonly DependencyProperty Region2BrushProperty = DependencyProperty.Register(nameof(Region2Brush),
            typeof(Brush), typeof(CellView),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(70, 255, 200, 200)), (obj, e) => ((CellView)obj).Region2BrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void Region2BrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            UpdateBackground();
        }

        public Brush Region2Brush
        {
            get => (Brush)GetValue(Region2BrushProperty);
            set => SetValue(Region2BrushProperty, value);
        }

        public static readonly DependencyProperty DefaultBrushProperty = DependencyProperty.Register(nameof(DefaultBrush),
            typeof(Brush), typeof(CellView),
            new PropertyMetadata(new SolidColorBrush(Colors.White), (obj, e) => ((CellView)obj).DefaultBrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void DefaultBrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            UpdateBackground();
        }

        public Brush DefaultBrush
        {
            get => (Brush)GetValue(DefaultBrushProperty);
            set => SetValue(DefaultBrushProperty, value);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(CellColor),
            typeof(Core.CellColor), typeof(CellView),
            new PropertyMetadata(default(Core.CellColor), (obj, e) => ((CellView)obj).ColorPropertyChanged((Core.CellColor)e.OldValue, (Core.CellColor)e.NewValue)));

        private void ColorPropertyChanged(Core.CellColor oldValue, Core.CellColor newValue)
        {
            if (newValue != null)
            {
                wrap.Fill = new SolidColorBrush(newValue.Fill);
            }
            else
            {
                wrap.Fill = null;
            }
        }

        public Core.CellColor CellColor
        {
            get => (Core.CellColor)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty CellProperty = DependencyProperty.Register(nameof(Cell),
            typeof(Core.ICell), typeof(CellView),
            new PropertyMetadata(default(Core.ICell), (obj, e) => ((CellView)obj).CellPropertyChanged((Core.ICell)e.OldValue, (Core.ICell)e.NewValue)));

        private void CellPropertyChanged(Core.ICell oldValue, Core.ICell newValue)
        {
            if(oldValue != null)
                oldValue.PropertyChanged -= Cell_PropertyChanged;

            label.Content = newValue?.Text;
            UpdateBackground();

            if (newValue != null)
                newValue.PropertyChanged += Cell_PropertyChanged;
        }

        private void Cell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateBackground();
        }

        public Core.ICell Cell
        {
            get => (Core.ICell)GetValue(CellProperty);
            set => SetValue(CellProperty, value);
        }

        public static readonly DependencyProperty CellFontSizeProperty = DependencyProperty.Register(nameof(CellFontSize),
            typeof(double), typeof(CellView),
            new PropertyMetadata(default(double), (obj, e) => ((CellView)obj).CellFontSizePropertyChanged((double)e.OldValue, (double)e.NewValue)));

        private void CellFontSizePropertyChanged(double oldValue, double newValue)
        {
            label.FontSize = newValue;
        }

        public double CellFontSize
        {
            get => (double)GetValue(CellFontSizeProperty);
            set => SetValue(CellFontSizeProperty, value);
        }

        public static readonly DependencyProperty AgentProperty = DependencyProperty.Register(nameof(Agent),
            typeof(CellViewAgent), typeof(CellView),
            new PropertyMetadata(CellViewAgent.None, (obj, e) => ((CellView)obj).AgentPropertyChanged((CellViewAgent)e.OldValue, (CellViewAgent)e.NewValue)));

        private void AgentPropertyChanged(CellViewAgent oldValue, CellViewAgent newValue)
        {
            agent1L.Visibility = newValue == CellViewAgent.Team1Agent1 || newValue == CellViewAgent.Team2Agent1 ? Visibility.Visible : Visibility.Collapsed;
            agent2L.Visibility = newValue == CellViewAgent.Team1Agent2 || newValue == CellViewAgent.Team2Agent2 ? Visibility.Visible : Visibility.Collapsed;
        }

        public CellViewAgent Agent
        {
            get => (CellViewAgent)GetValue(AgentProperty);
            set => SetValue(AgentProperty, value);
        }

        public static readonly DependencyProperty BlinkProperty = DependencyProperty.Register(nameof(Blink),
            typeof(bool), typeof(CellView),
            new PropertyMetadata(default(bool), (obj, e) => ((CellView)obj).BlinkPropertyChanged((bool)e.OldValue, (bool)e.NewValue)));

        private void BlinkPropertyChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                blinkAnim.Start();
            }
            else
            {
                blinkAnim.Stop();
                blinkV.Opacity = 0;
            }
        }

        public bool Blink
        {
            get => (bool)GetValue(BlinkProperty);
            set => SetValue(BlinkProperty, value);
        }

        void UpdateBackground()
        {
            bg.Children.Clear();

            var state1 = Cell?.State1 ?? Core.CellState.None;
            var state2 = Cell?.State2 ?? Core.CellState.None;

            var rect1 = GetRectForState(state1, Core.Teams.Team1);
            if (rect1 != null)
                bg.Children.Add(rect1);

            var rect2 = GetRectForState(state2, Core.Teams.Team2);
            if (rect2 != null)
                bg.Children.Add(rect2);

            double pri = ((int)Cell.Priority / 100.0);
            byte red = (byte)(pri * byte.MaxValue);
            BorderBrush = new SolidColorBrush(Color.FromRgb(red, (byte)(byte.MaxValue - red), (byte)(byte.MaxValue - red)));
        }

        Rectangle GetRectForState(Core.CellState state, Core.Teams team)
        {
            Brush brush = null;
            if (state.HasFlag(Core.CellState.InRegion))
            {
                // TODO 領域用のブラシを追加する
                brush =  team == Core.Teams.Team1 ? Region1Brush : Region2Brush;
            }
            else if (state.HasFlag(Core.CellState.Occupied))
            {
                brush = team == Core.Teams.Team1 ? Team1Brush : Team2Brush;
            }

            if (brush != null)
            {
                return new Rectangle()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = brush
                };
            }

            return null;
        }

        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Cell != null)
            {
                state1I.Header = $"チーム1 : {GetString(Cell.State1)}";
                state2I.Header = $"チーム2 : {GetString(Cell.State2)}";
                menu.IsOpen = true;
            }
        }

        private string GetString(Core.CellState state)
        {
            switch (state)
            {
                case Core.CellState.Occupied:
                    return "占有";
                case Core.CellState.InRegion:
                    return "領域内";
                case Core.CellState.None:
                    return "なし";
                default:
                    Console.WriteLine(Environment.StackTrace);
                    return "エラー";
            }
        }
    }
}
