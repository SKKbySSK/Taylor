using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Extensions
{
    public static class TeamExtension
    {
        public static Agent GetAnotherAgent(this Team team, Agent agent)
        {
            if (team.Agent1 == agent)
                return team.Agent2;

            if (team.Agent2 == agent)
                return team.Agent1;

            throw new InvalidOperationException("チーム内に対象のエージェントが存在しません");
        }
    }
}
