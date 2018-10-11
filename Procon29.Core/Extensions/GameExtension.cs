using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Extensions
{
    public static class GameExtension
    {
        /// <summary>
        /// 指定したエージェントの方向にあるセルへ移動可能かを取得します。敵チームが占領している場合や味方と重なる場合もfalseを返します
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool CanMove(this Game game, Agent agent, Direction direction)
        {
            var p = agent.Position.Move(direction);
            if (game.Field.IsInField(p))
            {
                Teams team = game.GetTeamFlag(agent).GetEnemyTeam();
                var cell = game.Field.GetCell(p);
                return cell.GetState(team) != CellState.Occupied && game.GetTeam(agent).GetAnotherAgent(agent).Position != p;
            }

            return false;
        }


        /// <summary>
        /// 指定したエージェントの方向にあるセルが撤去可能かを取得します。他のエージェントが除去したいタイルにいる場合もfalseとなります
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool CanRemove(this Game game, Agent agent, Direction direction)
        {
            if (direction == Direction.None) return false;

            if (game.Field.IsInField(agent, direction))
            {
                var team = game.GetTeamFlag(agent);
                var enemy = team.GetEnemyTeam();
                var p = agent.Position.Move(direction);
                return (game.Field.GetCell(p).GetState(enemy).HasFlag(CellState.Occupied) 
                    && game.GetTeam(team).GetAnotherAgent(agent).Position != p 
                    && game.GetTeam(enemy).Agent1.Position != p
                    && game.GetTeam(enemy).Agent2.Position != p) ;
            }

            return false;
        }

        /// <summary>
        /// エージェントのチームを取得します
        /// </summary>
        /// <param name="game"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static Team GetTeam(this Game game, Agent agent)
        {
            Team team = game.Team1;

            if (game.Team2.Agent1 == agent || game.Team2.Agent2 == agent)
                team = game.Team2;

            return team;
        }

        /// <summary>
        /// エージェントの所属するチームのフラグを取得します
        /// </summary>
        /// <param name="game"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static Teams GetTeamFlag(this Game game, Agent agent)
        {
            var team = GetTeam(game, agent);
            return game.Team1 == team ? Teams.Team1 : Teams.Team2;
        }
    }
}
