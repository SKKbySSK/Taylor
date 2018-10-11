using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Procon29.Core.Views
{
    public class CellView : Grid
    {
        Label label = new Label()
        {
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            FontSize = 17
        };

        public CellView()
        {
            Children.Add(label);
        }

        public static readonly DependencyProperty CellProperty = DependencyProperty.Register(nameof(Cell),
            typeof(ICell), typeof(CellView),
            new PropertyMetadata(default(ICell), (obj, e) => ((CellView)obj).CellPropertyChanged((ICell)e.OldValue, (ICell)e.NewValue)));

        private void CellPropertyChanged(ICell oldValue, ICell newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= Cell_PropertyChanged;

            label.Content = newValue?.Text;

            if (newValue != null)
                newValue.PropertyChanged += Cell_PropertyChanged;
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            label.Content = Cell?.Text;
        }

        public ICell Cell
        {
            get => (ICell)GetValue(CellProperty);
            set => SetValue(CellProperty, value);
        }
    }
}
