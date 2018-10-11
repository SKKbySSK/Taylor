using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Procon29.Views
{
    partial class TeamHandlerView : UserControl
    {
        TaskCompletionSource<object> ThreadPauseTask;

        public TeamHandlerView()
        {
            //0 Team1
            Parent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            //1 Team1 Children
            Parent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            //2 Team2
            Parent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            //3 Team2 Children
            Parent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            //4 Game
            Parent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            InitTeamsStack();
            InitGameStack();

            //初期状態に設定
            SetDefault();

            Content = Parent;
        }

        private void InitGameStack()
        {
            Grid.SetRow(GameStack, 4);
            Parent.Children.Add(GameStack);

            Label turnsL = new Label() { Content = Turns + "ターン" };

            TurnsSlider.Value = Turns;
            TurnsSlider.ValueChanged += (sender, e) =>
            {
                Turns = (int)e.NewValue;
                turnsL.Content = Turns + "ターン";
            };

            Label delayL = new Label() { Content = Math.Round(Delay.TotalMilliseconds) + "ms" };

            DelaySlider.Value = Delay.TotalMilliseconds;
            DelaySlider.ValueChanged += (sender, e) =>
            {
                Delay = TimeSpan.FromMilliseconds(e.NewValue);
                delayL.Content = Math.Round(Delay.TotalMilliseconds) + "ms";
            };

            Button playB = new Button() { Content = "一時停止する" };

            playB.Click += (sender, e) =>
            {
                if (IsPaused)
                {
                    playB.Content = "一時停止する";
                    IsPaused = false;
                }
                else
                {
                    playB.Content = "自動再生する";
                    IsPaused = true;
                }
            };

            Button nextB = new Button() { Content ="次のターン" };

            nextB.Click += (sender, e) => ThreadPauseTask?.TrySetResult(null);

            GameStack.Children.Add(turnsL);
            GameStack.Children.Add(TurnsSlider);
            GameStack.Children.Add(delayL);
            GameStack.Children.Add(DelaySlider);
            GameStack.Children.Add(playB);
            GameStack.Children.Add(nextB);
        }

        private void InitTeamsStack()
        {
            //チームのグリッド生成
            Grid.SetRow(Team1View, 1);
            Grid.SetRow(Team2View, 3);
            Parent.Children.Add(Team1View);
            Parent.Children.Add(Team2View);
            
            //チームのラジオボタン生成
            Team1Radio.Click += (sender, e) => Team = Core.Teams.Team1;
            Team2Radio.Click += (sender, e) => Team = Core.Teams.Team2;
            Grid.SetRow(Team2Radio, 2);
            Parent.Children.Add(Team1Radio);
            Parent.Children.Add(Team2Radio);
        }

        private void SetDefault()
        {
            Team = Core.Teams.Team1;
            TeamPropertyChanged(Core.Teams.Team1, Core.Teams.Team1);
        }

        private new Grid Parent { get; } = new Grid();

        public TeamView Team1View { get; } = new TeamView(Core.Teams.Team1) { Margin = new Thickness(0, 10, 0, 0) };

        public TeamView Team2View { get; } = new TeamView(Core.Teams.Team2) { Margin = new Thickness(0, 10, 0, 0) };

        private StackPanel GameStack { get; } = new StackPanel() { Margin = new Thickness(10, 0, 0, 0) };

        private RadioButton Team1Radio { get; } = new RadioButton() { Content = "Team1", Margin = new Thickness(0, 10, 0, 0) };

        private RadioButton Team2Radio { get; } = new RadioButton() { Content = "Team2", Margin = new Thickness(0, 10, 0, 0) };

        private Core.TeamHandlerBase TeamHandler1 { get; set; } = new Core.RandomTeamHandler();

        private Core.TeamHandlerBase TeamHandler2 { get; set; } = new Core.RandomTeamHandler();

        private Slider TurnsSlider { get; } = new Slider()
        {
            Maximum = 500,
            Minimum = 0
        };

        private Slider DelaySlider { get; } = new Slider()
        {
            Maximum = 1000,
            Minimum = 10,
        };
        
        public static readonly DependencyProperty TeamProperty = DependencyProperty.Register(nameof(Team),
            typeof(Core.Teams), typeof(TeamHandlerView),
            new PropertyMetadata(Core.Teams.Team1, (obj, e) => ((TeamHandlerView)obj).TeamPropertyChanged((Core.Teams)e.OldValue, (Core.Teams)e.NewValue)));

        private void TeamPropertyChanged(Core.Teams oldValue, Core.Teams newValue)
        {
            switch (newValue)
            {
                case Core.Teams.Team1:
                    Team1Radio.IsChecked = true;
                    Team1View.Visibility = Visibility.Visible;
                    Team2View.Visibility = Visibility.Collapsed;
                    break;
                case Core.Teams.Team2:
                    Team2Radio.IsChecked = true;
                    Team1View.Visibility = Visibility.Collapsed;
                    Team2View.Visibility = Visibility.Visible;
                    break;
            }
        }

        public Core.Teams Team
        {
            get => (Core.Teams)GetValue(TeamProperty);
            private set => SetValue(TeamProperty, value);
        }

        public static readonly DependencyProperty GameProperty = DependencyProperty.Register(nameof(Game),
            typeof(Core.Game), typeof(TeamHandlerView),
            new PropertyMetadata(default(Core.Game), (obj, e) => ((TeamHandlerView)obj).GamePropertyChanged((Core.Game)e.OldValue, (Core.Game)e.NewValue)));

        private void GamePropertyChanged(Core.Game oldValue, Core.Game game)
        {
            if (oldValue != null)
            {
                oldValue.Started -= Game_Started;
                oldValue.Finished -= Game_Finished;
                oldValue.TurnChanged -= Game_TurnChanged;

                Team1View.Team = null;
                Team2View.Team = null;
            }

            if (game != null)
            {
                game.Started += Game_Started;
                game.Finished += Game_Finished;
                game.TurnChanged += Game_TurnChanged;

                game.Team1.TeamHandler = TeamHandler1;
                game.Team2.TeamHandler = TeamHandler2;

                Team1View.Team = game.Team1;
                Team2View.Team = game.Team2;
            }
        }

        private void Game_Finished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TurnsSlider.IsEnabled = true;
                Team1View.IsGaming = false;
                Team2View.IsGaming = false;
            }));
        }

        private void Game_Started(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TurnsSlider.IsEnabled = false;
                Team1View.IsGaming = true;
                Team2View.IsGaming = true;
            }));
        }

        public Core.Game Game
        {
            get => (Core.Game)GetValue(GameProperty);
            set => SetValue(GameProperty, value);
        }

        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register(nameof(IsPaused),
            typeof(bool), typeof(TeamHandlerView),
            new PropertyMetadata(default(bool), (obj, e) => ((TeamHandlerView)obj).IsPausedPropertyChanged((bool)e.OldValue, (bool)e.NewValue)));

        private void IsPausedPropertyChanged(bool oldValue, bool newValue)
        {
            if (!newValue) ThreadPauseTask?.TrySetResult(null);
        }

        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay),
            typeof(TimeSpan), typeof(TeamHandlerView),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500), (obj, e) => ((TeamHandlerView)obj).DelayPropertyChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue)));

        private void DelayPropertyChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            DelaySlider.Value = newValue.TotalMilliseconds;
        }

        public TimeSpan Delay
        {
            get => (TimeSpan)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public static readonly DependencyProperty TurnsProperty = DependencyProperty.Register(nameof(Turns),
            typeof(int), typeof(TeamHandlerView),
            new PropertyMetadata(100, (obj, e) => ((TeamHandlerView)obj).TurnsPropertyChanged((int)e.OldValue, (int)e.NewValue)));

        private void TurnsPropertyChanged(int oldValue, int newValue)
        {

        }

        public int Turns
        {
            get => (int)GetValue(TurnsProperty);
            set => SetValue(TurnsProperty, value);
        }

        private T GetDependencyProperty<T>(DependencyProperty property)
        {
            TaskCompletionSource<T> task = new TaskCompletionSource<T>();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                task.SetResult((T)GetValue(property));
            }));

            return task.Task.Result;
        }

        private void Game_TurnChanged(object sender, EventArgs e)
        {
            var delay = GetDependencyProperty<TimeSpan>(DelayProperty);

            if (GetDependencyProperty<bool>(IsPausedProperty))
            {
                ThreadPauseTask = new TaskCompletionSource<object>();
                ThreadPauseTask.Task.Wait();
                ThreadPauseTask = null;
            }
            else if (delay > TimeSpan.Zero)
            {
                System.Threading.Thread.Sleep(delay);
            }
        }

        public void NextTurn()
        {
            ThreadPauseTask?.TrySetResult(null);
        }
    }
}
