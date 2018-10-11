using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Gimo
{
    class WayTest : TeamHandlerBase
    {
        Helper.Way Way { get; set; }

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }

        protected override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            int w = Field.Width - 1;
            int h = Field.Height - 1;
            if (Way == null) Way = new Helper.Way(Team.Agent1.Position, new Point(w, h), new Point(0, h), new Point(0, 0), new Point(w, 0));

            Way.Next(out var _, out var d);
            agent1(new Intent()
            {
                Intention = Intentions.Move,
                Direction = d
            });
            agent2(Intent.StayIntent);
        }
    }
}
