using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

using Procon29.Core;
using Procon29.Core.Taku.Algorithms;



namespace Procon29.Core.Taku
{
    class BruteForceSearchTeamHandler:TeamHandlerBase
    {
        //UIにテキストボックスを表示させるためのインスタンス
        private TextBox DepthControllTextBox { get; set; } = new TextBox();

        //力任せ法アルゴリズム
        private BruteForceSearchAlgorithm[] BruteForceSearch { get; set; }

        //コンストラクタ
        public BruteForceSearchTeamHandler()
        {
            DepthControllTextBox.Text = "";
        }

        //ゲーム開始時にインスタンスを取得
        protected override void GameStarted(Game game, Teams team)
        {
            base.GameStarted(game, team);
            BruteForceSearch = new BruteForceSearchAlgorithm[2];
            BruteForceSearch[0] = new BruteForceSearchAlgorithm(game,team,game.GetTeam(team).Agent1);
            BruteForceSearch[1] = new BruteForceSearchAlgorithm(game, team, game.GetTeam(team).Agent2);
        }

        //UI設定
        public override TeamHandlerUI ProvideUI()
        {
            return new TeamHandlerUI(DepthControllTextBox);    
        }

        //エージェント意志取得
        protected override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            int depth = BruteForceSearch[0].Depth;
            if (InvokeOnUIThread(() =>int.TryParse(DepthControllTextBox.Text, out depth)))
            {
                BruteForceSearch[0].Depth = BruteForceSearch[1].Depth = depth;
            }                
            else
                InvokeOnUIThread(()=>DepthControllTextBox.Text = BruteForceSearch[0].Depth.ToString());

            agent1(BruteForceSearch[0].NextIntent());
            agent2(BruteForceSearch[1].NextIntent());
        }
    }
}
