using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Taku.Algorithms;

namespace Procon29.Core.Taku
{
    class EnemyTerritoryDeleteTeamHandler:TeamHandlerBase
    {
        private EnemyTerritoryDeleteAlgorithm[] Algorithms { get; set; }

        //コンストラクタ

        //UI設定
        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }

        //ゲーム開始時に、インスタンスを作成
        protected override void GameStarted(Game game,Teams team)
        {
            base.GameStarted(game,team);
            Algorithms = new EnemyTerritoryDeleteAlgorithm[2];
            Algorithms[0] = new EnemyTerritoryDeleteAlgorithm(Game, TeamEnum, Team.Agent1);
            Algorithms[1] = new EnemyTerritoryDeleteAlgorithm(Game, TeamEnum, Team.Agent2);
        }

        //エージェント意志取得
        protected override void Turn(int turn,Action<Intent> agent1,Action<Intent> agent2)
        {
            agent1(Algorithms[0].NextIntent());
            //agent2(Algorithms[1].Search(TeamEnum, Team.Agent2));
            agent2(new Intent());
        }

    }
}
