using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Procon29.Views
{
    class GameView : FieldView
    {
        public static readonly DependencyProperty GameProperty = DependencyProperty.Register(nameof(Game),
            typeof(Core.Game), typeof(GameView),
            new PropertyMetadata(default(Core.Game), (obj, e) => ((GameView)obj).GamePropertyChanged((Core.Game)e.OldValue, (Core.Game)e.NewValue)));

        private void GamePropertyChanged(Core.Game oldValue, Core.Game newValue)
        {
            if (oldValue != null)
            {
                oldValue.Team1.Agent1.PropertyChanged -= Cell_PropertyChanged;
                oldValue.Team1.Agent2.PropertyChanged -= Cell_PropertyChanged;
                oldValue.Team2.Agent1.PropertyChanged -= Cell_PropertyChanged;
                oldValue.Team2.Agent2.PropertyChanged -= Cell_PropertyChanged;
            }

            Field = null;

            if (newValue != null)
            {
                Field = newValue.Field;

                newValue.Team1.SynchronizationContext = System.Threading.SynchronizationContext.Current;
                newValue.Team2.SynchronizationContext = System.Threading.SynchronizationContext.Current;

                newValue.Team1.Agent1.PropertyChanged += Cell_PropertyChanged;
                newValue.Team1.Agent2.PropertyChanged += Cell_PropertyChanged;
                newValue.Team2.Agent1.PropertyChanged += Cell_PropertyChanged;
                newValue.Team2.Agent2.PropertyChanged += Cell_PropertyChanged;

                UpdateCellAgents();
            }
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCellAgents();
        }

        public Core.Game Game
        {
            get => (Core.Game)GetValue(GameProperty);
            set => SetValue(GameProperty, value);
        }

        private void UpdateCellAgents()
        {
            var views = GetCellViews();

            foreach (var v in views) v.Agent = CellViewAgent.None;

            Update(Game.Team1.Agent1, CellViewAgent.Team1Agent1);
            Update(Game.Team1.Agent2, CellViewAgent.Team1Agent2);
            Update(Game.Team2.Agent1, CellViewAgent.Team2Agent1);
            Update(Game.Team2.Agent2, CellViewAgent.Team2Agent2);

            void Update(Core.Agent a, CellViewAgent agentFlag)
            {
                var pos = a.Position;
                views[pos.Y, pos.X].Agent = agentFlag;
            }
        }
    }
}
