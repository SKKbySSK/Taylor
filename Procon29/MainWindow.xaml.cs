using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Procon29.Core.Extensions;

namespace Procon29
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Core.Game game;
        DateTime began;
        System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();
        Task gameTask = null;

        Func<Core.Intent, bool> manualIntentAction = null;
        Core.Agent manualIntentAgent = null;
        Core.Teams manualIntentTeam;

        public MainWindow()
        {
            InitializeComponent();

            handlerView.Team1View.PropertyChanged += TeamView_PropertyChanged;
            handlerView.Team2View.PropertyChanged += TeamView_PropertyChanged;
        }

        private void TeamView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(handlerView.Team1View.Handler))
            {
                {
                    if (mainField.Handler1 is Core.ManualTeamHandler manual1)
                    {
                        manual1.NeedIntent1 -= Manual1_NeedIntent1;
                        manual1.NeedIntent2 -= Manual1_NeedIntent2;
                    }

                    if (mainField.Handler2 is Core.ManualTeamHandler manual2)
                    {
                        manual2.NeedIntent1 -= Manual2_NeedIntent1;
                        manual2.NeedIntent2 -= Manual2_NeedIntent2;
                    }
                }

                if (e.PropertyName == nameof(Views.TeamView.Handler))
                {
                    mainField.Handler1 = handlerView.Team1View.Handler;
                    mainField.Handler2 = handlerView.Team2View.Handler;

                    if (mainField.Handler1 is Core.ManualTeamHandler manual1)
                    {
                        manual1.NeedIntent1 += Manual1_NeedIntent1;
                        manual1.NeedIntent2 += Manual1_NeedIntent2;
                    }

                    if (mainField.Handler2 is Core.ManualTeamHandler manual2)
                    {
                        manual2.NeedIntent1 += Manual2_NeedIntent1;
                        manual2.NeedIntent2 += Manual2_NeedIntent2;
                    }
                }
            }
        }

        private void Manual2_NeedIntent2(object sender, EventArgs e)
        {
            var handler = (Core.ManualTeamHandler)sender;
            manualIntentAction = handler.SetIntent2;
            manualIntentTeam = Core.Teams.Team2;
            SetBlink(mainField.Game.Team2.Agent2);
        }

        private void Manual2_NeedIntent1(object sender, EventArgs e)
        {
            var handler = (Core.ManualTeamHandler)sender;
            manualIntentAction = handler.SetIntent1;
            manualIntentTeam = Core.Teams.Team2;
            SetBlink(mainField.Game.Team2.Agent1);
        }

        private void Manual1_NeedIntent2(object sender, EventArgs e)
        {
            var handler = (Core.ManualTeamHandler)sender;
            manualIntentAction = handler.SetIntent2;
            manualIntentTeam = Core.Teams.Team1;
            SetBlink(mainField.Game.Team1.Agent2);
        }

        private void Manual1_NeedIntent1(object sender, EventArgs e)
        {
            var handler = (Core.ManualTeamHandler)sender;
            manualIntentAction = handler.SetIntent1;
            manualIntentTeam = Core.Teams.Team1;
            SetBlink(mainField.Game.Team1.Agent1);
        }

        private void SetBlink(Core.Agent agent)
        {
            manualIntentAgent = agent;

            var cells = mainField.GetCellViews();
            foreach (var v in cells)
                v.Blink = false;

            var views = mainField.Field.GetPoints(agent.Position, Core.Direction.All.ParseFlags(true).ToArray())
                .Select(p => mainField.GetCellView(p));
            mainField.GetCellView(agent.Position).Blink = true;

            foreach (var v in views) v.Blink = true;
        }

        private void mainField_Clicked(object sender, Views.CellClickedEventArgs e)
        {
            if (manualIntentAgent == null) return;

            var delta = e.Position - manualIntentAgent.Position;

            if (delta.Length() >= 2) return;

            if (manualIntentAction != null && manualIntentAgent != null)
            {
                if (e.Position != manualIntentAgent.Position)
                {
                    var intention = Core.Intentions.Move;
                    var direction = Core.Direction.None;

                    if (delta.X >= 1) direction |= Core.Direction.Right;
                    else if (delta.X <= -1) direction |= Core.Direction.Left;
                    if (delta.Y >= 1) direction |= Core.Direction.Down;
                    else if (delta.Y <= -1) direction |= Core.Direction.Up;

                    var enemy = (Core.Teams.Team1 | Core.Teams.Team2) & ~manualIntentTeam;
                    if (mainField.Field.GetCell(e.Position).GetState(enemy) == Core.CellState.Occupied)
                        intention = Core.Intentions.Remove;

                    manualIntentAction(new Core.Intent()
                    {
                        Intention = intention,
                        Direction = direction
                    });
                }
                else
                {
                    manualIntentAction(new Core.Intent()
                    {
                        Intention = Core.Intentions.Stay
                    });
                }
            }
        }

        private async void newField_Click(object sender, RoutedEventArgs e)
        {
            await AbortGameAsync();

            var muB = double.TryParse(muT.Text, out var mu);
            var sigB = double.TryParse(sigmaT.Text, out var sig);

            if (!muB)
            {
                MessageBox.Show("μの値が数値ではありません");
                return;
            }

            if (!sigB)
            {
                MessageBox.Show("σの値が数値ではありません");
                return;
            }

            var random = new Core.Algorithms.NormalDistribution()
            {
                Mu = mu,
                Sigma = sig,
                UseCos = cosC.IsChecked ?? false
            };

            var field = new Core.Field(new Core.FieldGenerators.RandomFieldGenerator((int)widthS.Value, (int)heightS.Value, random));
            var hw = widthS.Value / 2;
            var hh = heightS.Value / 2;

            Random rnd = new Random();

            int agentMaxW = (int)(widthS.Value % 2 == 0 ? hw + 1 : hw);
            int agentMaxH= (int)(widthS.Value % 2 == 0 ? hh + 1 : hh);
            Core.Point agent = new Core.Point(rnd.Next(0, agentMaxH), rnd.Next(0, agentMaxW));

            int crossX = ((int)widthS.Value) - agent.X - 1;
            int crossY = ((int)heightS.Value) - agent.Y - 1;

            var g = new Core.Game(field, new Core.Team(agent, new Core.Point(crossX, crossY)), new Core.Team(new Core.Point(crossX, agent.Y), new Core.Point(agent.X, crossY)));
            await StartGame(g);
        }

        private void qrItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new Cv.Reader();
            window.Captured += Window_Captured;
            window.FormClosed += Window_FormClosed;
            window.Show();
        }

        private void Window_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            var window = (Cv.Reader)sender;
            window.Captured -= Window_Captured;
            window.FormClosed -= Window_FormClosed;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Window_Captured(ZXing.Result obj)
        {
            if(Core.Helper.QRParser.TryParse(obj.Text, out var game))
            {
                await StartGame(game);
            }
        }

        private async Task StartGame(Core.Game game)
        {
            game.Finished += Game_Finished;
            game.TurnChanged += Game_TurnChanged;
            this.game = game;
            began = DateTime.Now;

            mainField.Game = game;
            handlerView.Game = game;
            
            if (game.Team1.TeamHandler is Core.ManualTeamHandler manual1)
            {
                manual1.NeedIntent1 += Manual1_NeedIntent1;
                manual1.NeedIntent2 += Manual1_NeedIntent2;
            }

            if (game.Team2.TeamHandler is Core.ManualTeamHandler manual2)
            {
                manual2.NeedIntent1 += Manual2_NeedIntent1;
                manual2.NeedIntent2 += Manual2_NeedIntent2;
            }

            game.Length = handlerView.Turns;
            var t = game.Start(tokenSource.Token);
            gameTask = t;

            abortGameItem.Visibility = Visibility.Visible;
            newGameItem.Visibility = Visibility.Collapsed;

            await t;
        }

        private void Game_TurnChanged(object sender, EventArgs e)
        {
            ExportGame(game, began);
        }

        private async void Game_Finished(object sender, EventArgs e)
        {
            var game = (Core.Game)sender;
            game.Finished -= Game_Finished;
            game.TurnChanged -= Game_TurnChanged;
            
            tokenSource?.Dispose();
            tokenSource = new System.Threading.CancellationTokenSource();
            gameTask = null;

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                abortGameItem.Visibility = Visibility.Collapsed;
                newGameItem.Visibility = Visibility.Visible;
            }));
        }

        private async Task AbortGameAsync()
        {
            abortGameItem.Visibility = Visibility.Collapsed;
            newGameItem.Visibility = Visibility.Collapsed;

            var t = gameTask;
            if (t != null)
            {
                tokenSource?.Cancel();
                handlerView.NextTurn();
                await t;
            }
        }

        private async void abortGameItem_Click(object sender, RoutedEventArgs e)
        {
            await AbortGameAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            App.LogDispatcher.BeginInvoke(new Action(() =>
            {
                App.LogWindow.Close();
            }));
        }

        private void ExportGame(Core.Game game, DateTime date)
        {
            System.IO.Directory.CreateDirectory("Games");
            var path = System.IO.Path.Combine("Games", $"{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}.pgame");
            Core.Export.GameSerializer.Serialize(game, path);
        }

        private void loadItem_Click(object sender, RoutedEventArgs e)
        {
            var w = new RestoreWindow();

            var paths = System.IO.Directory.GetFiles("Games/", "*.pgame").OrderByDescending(p => new System.IO.FileInfo(p).CreationTime.Ticks);
            foreach (var p in paths) w.Paths.Add(p);

            w.ShowDialog();
        }
    }
}
