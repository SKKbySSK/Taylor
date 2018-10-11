using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core
{
    class StayTeamHandler : TeamHandlerBase
    {
        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }

        protected internal override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            agent1(Intent.StayIntent);
            agent2(Intent.StayIntent);
        }
    }
}
