using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Procon29.Core.Views
{
    public class FieldView : Grid
    {
        CellView[,] viewField;

        public FieldView()
        {
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

            }
            viewField = null;

            Children.Clear();
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();

            if (newValue != null)
            {
                newValue.Evaluated += Field_Evaluated;

                for (int i = 0; newValue.Width > i; i++)
                    ColumnDefinitions.Add(new ColumnDefinition());
                for (int i = 0; newValue.Height > i; i++)
                    RowDefinitions.Add(new RowDefinition());

                var field = newValue.Map;

                foreach (var cell in field) cell.SynchronizationContext = System.Threading.SynchronizationContext.Current;

                var h = field.GetLength(0);
                var w = field.GetLength(1);
                viewField = new CellView[h, w];

                for (int y = 0; h > y; y++)
                {
                    for (int x = 0; w > x; x++)
                    {
                        CellView view = new CellView();
                        view.Margin = new Thickness(5);
                        view.Cell = field[y, x];

                        viewField[y, x] = view;

                        SetRow(view, y);
                        SetColumn(view, x);
                        Children.Add(view);
                    }
                }
            }
        }

        private void Field_Evaluated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Field Field
        {
            get => (Field)GetValue(FieldProperty);
            set => SetValue(FieldProperty, value);
        }
    }
}
