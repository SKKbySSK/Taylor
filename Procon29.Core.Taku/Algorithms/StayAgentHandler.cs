using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Procon29.Core.Genetic;

namespace Procon29.Core.Taku.Algorithms
{
    class StayAgentHandler : AgentAlgorithmBase
    {
        public StayAgentHandler(Game game, Agent agent) : base(game, agent)
        {
        }

        public override Intent NextIntent(params Point[] prohibitedPoints)
        {
            return new Intent();
        }

        public override TeamHandlerUI ProvideUI()
        {
            throw new NotImplementedException();
        }

        public override Intent Search(params Point[] prohibitedPoints)
        {
            return new Intent();
        }

        protected override ScoringEfficiency EvaluateGame()
        {
            return new ScoringEfficiency(0, 0);
        }
    }
}
