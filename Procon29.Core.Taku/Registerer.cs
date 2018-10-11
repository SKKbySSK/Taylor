using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Taku.Algorithms;

namespace Procon29.Core.Taku
{
    public static class Registerer
    {
        public static void Register()
        {
            //ここでTeamHandlerBaseを登録してください（このメソッドはUIから自動的に呼び出されます
            //HandlerFactory.Register("name", () => new TeamHandlerBase());
            //HandlerFactory.Register("力任せ法", () => new BruteForceSearchTeamHandler());
           // HandlerFactory.Register("力任せ法　改D1", () => new NoneCollisionTeamHandler((g, a) => new BruteForceSearchAgentHandler(g, a, 1), (g, a) => new BruteForceSearchAgentHandler(g, a, 1)));
            HandlerFactory.Register("力任せ法　改D3", () => new NoneCollisionTeamHandler((g, a) => new BruteForceSearchAgentHandler(g, a, 3), (g, a) => new BruteForceSearchAgentHandler(g, a, 3)));
            HandlerFactory.Register("力任せ法　改D4", () => new NoneCollisionTeamHandler((g, a) => new BruteForceSearchAgentHandler(g, a, 4), (g, a) => new BruteForceSearchAgentHandler(g, a, 4)));
            //HandlerFactory.Register("敵領域妨害法", () => new EnemyTerritoryDeleteTeamHandler());
            //HandlerFactory.Register("敵領域妨害法　改", () => new NoneCollisionTeamHandler((g, a) => new EnemyTerritoryDeleteAgentHandler(g, a), (g, a) => new EnemyTerritoryDeleteAgentHandler(g, a)));
            HandlerFactory.Register("敵領域妨害法 Agent1", () => new NoneCollisionTeamHandler((g, a) => new EnemyTerritoryDeleteAgentHandler(g, a), (g, a) => new StayAgentHandler(g, a)));
            HandlerFactory.Register("敵領域妨害法 ", () => new NoneCollisionTeamHandler((g, a) => new EnemyTerritoryDeleteAgentHandler(g, a), (g, a) => new BruteForceSearchAgentHandler(g, a, 3)));
            //HandlerFactory.Register("待機", () => new NoneCollisionTeamHandler((g, a) => new StayAgentHandler(g, a), (g, a) => new StayAgentHandler(g, a)));
            HandlerFactory.Register("疑似最善経路探索法", () => new NoneCollisionTeamHandler((g, a) => new BetterRoutesSearchAgentHandler(g, a),(g, a) => new BetterRoutesSearchAgentHandler(g, a)));
            HandlerFactory.Register("アルゴリズム複合行動決定法", () => new SelectBestAlgorithmTeamHandler((g, a) => new StayAgentHandler(g, a), (g, a) => new BetterRoutesSearchAgentHandler(g, a), (g, a) => new EnemyTerritoryDeleteAgentHandler(g, a), (g, a) => new BruteForceSearchAgentHandler(g, a, 3)));
        }
    }
}
