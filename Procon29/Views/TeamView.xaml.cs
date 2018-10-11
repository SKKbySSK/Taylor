using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// <summary>
    /// TeamView.xaml の相互作用ロジック
    /// </summary>
    public partial class TeamView : UserControl, INotifyPropertyChanged
    {
        class HandlerPanel : StackPanel
        {
            public event EventHandler Selected;

            public HandlerPanel(Core.Teams team, KeyValuePair<string, Func<Core.TeamHandlerBase>> pair)
            {
                Pair = pair;
                TeamHandler = pair.Value();
                Margin = new Thickness(10, 10, 0, 0);

                RadioButton.GroupName = team + "HandlerPanel";
                RadioButton.Content = pair.Key;
                Children.Add(RadioButton);

                RadioButton.Checked += RadioButton_Checked;
                RadioButton.Unchecked += RadioButton_Unchecked;
            }

            private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
            {
                TeamHandler.CellUIChanged -= TeamHandler_CellUIChanged;
                if (StackPanel != null)
                {
                    StackPanel.Children.Clear();
                    Children.Remove(StackPanel);
                    StackPanel = null;
                }
            }

            private void RadioButton_Checked(object sender, RoutedEventArgs e)
            {
                TeamHandler.CellUIChanged += TeamHandler_CellUIChanged;
                RefreshUI();
            }

            private void RefreshUI()
            {
                StackPanel = (TeamHandler.ProvideUI() ?? new Core.TeamHandlerUI()).GetStackPanel();
                StackPanel.Margin = new Thickness(10, 10, 0, 0);
                Children.Add(StackPanel);
                Selected?.Invoke(this, new EventArgs());
            }

            private void TeamHandler_CellUIChanged(object sender, EventArgs e)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (TeamHandler != null)
                        RefreshUI();
                }));
            }

            public RadioButton RadioButton { get; } = new RadioButton();

            public StackPanel StackPanel { get; set; }

            public KeyValuePair<string, Func<Core.TeamHandlerBase>> Pair { get; }

            public Core.TeamHandlerBase TeamHandler { get; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TeamView(Core.Teams team)
        {
            TeamEnum = team;
            InitializeComponent();
            RefreshHandlers();
        }

        public Core.Teams TeamEnum { get; }

        public static readonly DependencyProperty HandlerProperty = DependencyProperty.Register(nameof(Handler),
            typeof(Core.TeamHandlerBase), typeof(TeamView),
            new PropertyMetadata(default(Core.TeamHandlerBase)));

        public Core.TeamHandlerBase Handler
        {
            get => (Core.TeamHandlerBase)GetValue(HandlerProperty);
            private set => SetValue(HandlerProperty, value);
        }

        public static readonly DependencyProperty TeamProperty = DependencyProperty.Register(nameof(Team),
            typeof(Core.Team), typeof(TeamView),
            new PropertyMetadata(default(Core.Team), (obj, e) => ((TeamView)obj).TeamPropertyChanged((Core.Team)e.OldValue, (Core.Team)e.NewValue)));

        private void TeamPropertyChanged(Core.Team oldValue, Core.Team newValue)
        {
            if (oldValue != null) oldValue.TeamHandler = null;
            if (newValue != null) newValue.TeamHandler = Handler;
        }

        public Core.Team Team
        {
            get => (Core.Team)GetValue(TeamProperty);
            set => SetValue(TeamProperty, value);
        }

        public void RefreshHandlers()
        {
            foreach (HandlerPanel panel in this.panel.Children)
            {
                panel.Selected -= Panel_Selected;
            }
            this.panel.Children.Clear();

            bool f = true;
            var pairs = Core.HandlerFactory.GetEnumerable();
            foreach (var pair in pairs)
            {
                HandlerPanel panel = new HandlerPanel(TeamEnum, pair);
                panel.Selected += Panel_Selected;

                if (f)
                    panel.RadioButton.IsChecked = true;
                f = false;

                this.panel.Children.Add(panel);
            }
        }

        public static readonly DependencyProperty IsGamingProperty = DependencyProperty.Register(nameof(IsGaming),
            typeof(bool), typeof(TeamView),
            new PropertyMetadata(default(bool), (obj, e) => ((TeamView)obj).IsGamingPropertyChanged((bool)e.OldValue, (bool)e.NewValue)));

        private void IsGamingPropertyChanged(bool oldValue, bool newValue)
        {
        }

        public bool IsGaming
        {
            get => (bool)GetValue(IsGamingProperty);
            set => SetValue(IsGamingProperty, value);
        }

        private void Panel_Selected(object sender, EventArgs e)
        {
            var panel = (HandlerPanel)sender;
            Handler = panel.TeamHandler;
            if (Team != null)
            {
                Team.TeamHandler = panel.TeamHandler;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }
    }
}
