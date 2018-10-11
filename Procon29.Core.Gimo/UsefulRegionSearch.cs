using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Procon29.Core.Extensions;
using System.Windows.Media;

namespace Procon29.Core.Gimo
{
    class UsefulRegionSearch : TeamHandlerBase
    {
        Algorithms.UsefulRegionSearchAgentHandler algorithm1;
        Algorithms.UsefulRegionSearchAgentHandler algorithm2;

        private CheckBox ag1C = new CheckBox() { Content = "エージェント1を動かす", IsChecked = true };

        private CheckBox ag2C = new CheckBox() { Content = "エージェント2を動かす", IsChecked = false };

        private CheckBox dumpEffiC = new CheckBox() { Content = "得点能率のダンプ", IsChecked = true };
        
        protected override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            bool ag1 = InvokeOnUIThread(() => ag1C.IsChecked ?? false);
            bool ag2 = InvokeOnUIThread(() => ag2C.IsChecked ?? false);

            if (ag1)
                agent1(algorithm1.NextIntent());
            else
                agent1(Intent.StayIntent);

            if (ag2)
                agent2(algorithm2.NextIntent());
            else
                agent2(Intent.StayIntent);
        }

        protected override void GameStarted(Game game, Teams team)
        {
            base.GameStarted(game, team);
            algorithm1 = new Algorithms.UsefulRegionSearchAgentHandler(game, Team.Agent1);
            algorithm1 = new Algorithms.UsefulRegionSearchAgentHandler(game, Team.Agent2);
        }

        protected override void GameFinished()
        {
            base.GameFinished();
        }
        
        public override TeamHandlerUI ProvideUI()
        {
            return null;//new TeamHandlerUI(diagonalC, ag1C, ag2C, dumpEffiC, calcB);
        }
    }
}