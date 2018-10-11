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
using Procon29.Core;

namespace Procon29.Views
{
    public class CellClickedEventArgs : EventArgs
    {
        public CellClickedEventArgs(CellView view, Core.Point position)
        {
            View = view;
            Position = position;
        }

        public CellView View { get; }

        public Core.Point Position { get; }
    }

    /// <summary>
    /// FieldView.xaml の相互作用ロジック
    /// </summary>
    public partial class FieldView : UserControl
    {
        CellView[,] viewField;

        public CellView[,] GetCellViews() => viewField;

        public event EventHandler<CellClickedEventArgs> Clicked;

        public FieldView()
        {
            InitializeComponent();

            xAxis.Fill = XAxisBrush;
            yAxis.Fill = YAxisBrush;
        }

        public static readonly DependencyProperty FieldProperty = DependencyProperty.Register(nameof(Field),
            typeof(Field), typeof(FieldView),
            new PropertyMetadata(default(Field), (obj, e) => ((FieldView)obj).FieldPropertyChanged((Field)e.OldValue, (Field)e.NewValue)));

        private void FieldPropertyChanged(Field oldValue, Field newValue)
        {
            if (oldValue != null)
            {
                oldValue.Evaluated -= Field_Evaluated;
            }

            if (viewField != null)
            {
                foreach(var cell in viewField)
                {
                    cell.MouseLeftButtonUp -= View_MouseLeftButtonUp;
                }
            }

            viewField = null;

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            if (newValue != null)
            {
                newValue.Evaluated += Field_Evaluated;
                team1L.Content = newValue.Score1;
                team2L.Content = newValue.Score2;

                for (int i = 0; newValue.Width > i; i++)
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                for (int i = 0; newValue.Height > i; i++)
                    grid.RowDefinitions.Add(new RowDefinition());
                
                var field = newValue.Map;
                
                var h = field.GetLength(0);
                var w = field.GetLength(1);
                viewField = new CellView[h, w];

                for (int y = 0; h > y; y++)
                {
                    for (int x = 0; w > x; x++)
                    {
                        CellView view = new CellView();
                        view.MouseLeftButtonUp += View_MouseLeftButtonUp;
                        view.Margin = new Thickness(5);
                        view.Cell = field[y, x];
                        view.CellFontSize = 20;

                        viewField[y, x] = view;

                        Grid.SetRow(view, y);
                        Grid.SetColumn(view, x);
                        grid.Children.Add(view);
                    }
                }
            }

            UpdateCellColors();
        }

        private void View_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var cell = (CellView)sender;

            Core.Extensions.ArrayExtension.ForEach(viewField, (p, v) =>
            {
                if (v == cell)
                {
                    Clicked?.Invoke(this, new CellClickedEventArgs(cell, p));
                }
            });
        }

        private void Field_Evaluated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //フィールドがnullになる可能性あり
                if (Field != null)
                {
                    team1L.Content = Field.Score1;
                    team2L.Content = Field.Score2;
                }
            }));
        }

        public Field Field
        {
            get => (Field)GetValue(FieldProperty);
            set => SetValue(FieldProperty, value);
        }

        public static readonly DependencyProperty YAxisBrushProperty = DependencyProperty.Register(nameof(YAxisBrush),
            typeof(Brush), typeof(FieldView),
            new PropertyMetadata(Brushes.Transparent, (obj, e) => ((FieldView)obj).YAxisBrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void YAxisBrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            yAxis.Fill = newValue;
        }

        public Brush YAxisBrush
        {
            get => (Brush)GetValue(YAxisBrushProperty);
            set => SetValue(YAxisBrushProperty, value);
        }

        public static readonly DependencyProperty XAxisBrushProperty = DependencyProperty.Register(nameof(XAxisBrush),
            typeof(Brush), typeof(FieldView),
            new PropertyMetadata(Brushes.Transparent, (obj, e) => ((FieldView)obj).XAxisBrushPropertyChanged((Brush)e.OldValue, (Brush)e.NewValue)));

        private void XAxisBrushPropertyChanged(Brush oldValue, Brush newValue)
        {
            xAxis.Fill = newValue;
        }

        public Brush XAxisBrush
        {
            get => (Brush)GetValue(XAxisBrushProperty);
            set => SetValue(XAxisBrushProperty, value);
        }

        public static readonly DependencyProperty Handler1Property = DependencyProperty.Register(nameof(Handler1),
            typeof(TeamHandlerBase), typeof(FieldView),
            new PropertyMetadata(default(TeamHandlerBase), (obj, e) => ((FieldView)obj).Handler1PropertyChanged((TeamHandlerBase)e.OldValue, (Core.TeamHandlerBase)e.NewValue)));

        private void Handler1PropertyChanged(TeamHandlerBase oldValue, TeamHandlerBase handler)
        {
            if (oldValue != null)
            {
                oldValue.CellColorsChanged -= Handler_CellColorsChanged;
            }

            if (handler != null)
            {
                handler.CellColorsChanged += Handler_CellColorsChanged;
            }

            UpdateCellColors();
        }

        public TeamHandlerBase Handler1
        {
            get => (TeamHandlerBase)GetValue(Handler1Property);
            set => SetValue(Handler1Property, value);
        }

        public static readonly DependencyProperty Handler2Property = DependencyProperty.Register(nameof(Handler2),
            typeof(TeamHandlerBase), typeof(FieldView),
            new PropertyMetadata(default(TeamHandlerBase), (obj, e) => ((FieldView)obj).Handler2PropertyChanged((TeamHandlerBase)e.OldValue, (TeamHandlerBase)e.NewValue)));

        private void Handler2PropertyChanged(TeamHandlerBase oldValue, TeamHandlerBase handler)
        {
            if (oldValue != null)
            {
                oldValue.CellColorsChanged -= Handler_CellColorsChanged;
            }

            if (handler != null)
            {
                handler.CellColorsChanged += Handler_CellColorsChanged;
            }

            UpdateCellColors();
        }

        public TeamHandlerBase Handler2
        {
            get => (TeamHandlerBase)GetValue(Handler2Property);
            set => SetValue(Handler2Property, value);
        }

        private void Handler_CellColorsChanged(object sender, EventArgs e)
        {
            UpdateCellColors();
        }

        private void UpdateCellColors()
        {
            if (viewField != null)
            {
                foreach (var v in viewField)
                    v.CellColor = null;

                List<CellColor> colors = new List<CellColor>();

                if (Handler1?.CellColors != null)
                    colors.AddRange(Handler1.CellColors);
                if (Handler2?.CellColors != null)
                    colors.AddRange(Handler2.CellColors);

                foreach (var c in colors)
                {
                    var cell = GetCellView(c.Cell);
                    cell.CellColor = c;
                }
            }
        }

        public CellView GetCellView(Core.Point position)
        {
            return viewField[position.Y, position.X];
        }
    }
}
