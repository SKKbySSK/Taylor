using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Extensions;

namespace Procon29.Core.Taku.AgentHandlers
{
    class AgentHandlerBase
    {
        protected Game Game { get; private set; }

        protected Team Team { get; private set; }   //呼び出し元のチームを保持

        protected Teams TeamEnum { get; private set; }

        protected Team Enemy { get; private set; }  //敵チームを保持

        protected Teams EnemyEnum { get; private set; }

        protected Agent Agent { get; private set; }

        protected Field Field => Game?.Field;

        protected internal virtual void GameStarted(Game game, Agent agent)
        {
            Game = game;
            Team = game.GetTeam(agent); 
            Agent = agent;
            TeamEnum = game.GetTeamFlag(agent);

            EnemyEnum = (Teams.Team1 | Teams.Team2) & ~TeamEnum;
            Enemy = game.GetTeam(EnemyEnum);
        }

        protected internal virtual void GameFinished()
        {
            Game = null;
            Team = null;
            Enemy = null;
            Agent = null;
        }
    }
}
