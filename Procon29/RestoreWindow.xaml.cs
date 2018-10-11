using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Procon29
{
    /// <summary>
    /// RestoreWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RestoreWindow : Window
    {
        public RestoreWindow()
        {
            InitializeComponent();

            pathsL.ItemsSource = Paths;
            pathsL.SelectionChanged += PathsL_SelectionChanged;
        }

        private void PathsL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pathsL.SelectedIndex > -1)
            {
                var game = Core.Export.GameSerializer.Deserialize(Paths[pathsL.SelectedIndex]);
                game.Field.EvaluateMap(Core.Teams.Team1);
                game.Field.EvaluateMap(Core.Teams.Team2);
                gameV.Game = new Core.Game(game);
            }
            else
                gameV.Game = null;
        }

        public System.Collections.ObjectModel.ObservableCollection<string> Paths { get; } = new System.Collections.ObjectModel.ObservableCollection<string>();
    }

    public class PathToFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return System.IO.Path.GetFileNameWithoutExtension(path);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
