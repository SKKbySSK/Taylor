using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon29.Core.Extensions;

namespace Procon29.Core.Gimo
{
    class NearRegionSearch : TeamHandlerBase
    {
        Algorithms.NearRegionSearchAgentHandler algorithm1;
        Algorithms.NearRegionSearchAgentHandler algorithm2;

        public NearRegionSearch()
        {
            Slider.ValueChanged += Slider_ValueChanged;
            Slider_ValueChanged(Slider, new System.Windows.RoutedPropertyChangedEventArgs<double>(Slider.Value, Slider.Value));
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Label.Content = "Around : " + e.NewValue;

            if (algorithm1 != null)
                algorithm1.Around = (int)e.NewValue;
        }

        private System.Windows.Controls.Label Label { get; } = new System.Windows.Controls.Label();

        private System.Windows.Controls.Slider Slider { get; } = new System.Windows.Controls.Slider()
        {
            Maximum = 7,
            Minimum = 1,
            Value = 4,
            IsSnapToTickEnabled = true,
            LargeChange = 1,
            SmallChange = 1
        };

        public override TeamHandlerUI ProvideUI()
        {
            return new TeamHandlerUI(Label, Slider);
        }

        protected override void GameStarted(Game game, Teams team)
        {
            base.GameStarted(game, team);
            algorithm1 = new Algorithms.NearRegionSearchAgentHandler(game, Team.Agent1, InvokeOnUIThread(() => (int)Slider.Value));
            algorithm2 = new Algorithms.NearRegionSearchAgentHandler(game, Team.Agent2, InvokeOnUIThread(() => (int)Slider.Value));
        }

        protected override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            agent1(algorithm1.NextIntent());
            agent2(algorithm2.NextIntent());
        }
    }
}
