using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Genetic;
using Procon29.Core.Extensions;
using System.Windows.Controls;

namespace Procon29.Core.Taku
{
    class NoneCollisionTeamHandler : TeamHandlerBase
    {
        private Func<Game, Agent, AgentAlgorithmBase> Agent1Algorithm;
        private Func<Game, Agent, AgentAlgorithmBase> Agent2Algorithm;

        public AgentAlgorithmBase Agent1 { get; set; }
        public AgentAlgorithmBase Agent2 { get; set; }


        //推定スコアをUI上に表示
        private Label Label1 = new Label()
        {
            Content = string.Format("Agent1 Effic:{0,4:F2} NTurn:{1,4:F2} Total:{2,4:F2}", 0, 0,0),
            FontSize = 12
        };
        private Label Label2 = new Label()
        {
            Content = string.Format("Agent2 Effic:{0,4:F2} NTurn:{1,4:F2} Total:{2,4:F2}", 0, 0,0),
            FontSize = 12
        };


        //このインスタンスを作るオブジェクトが、エージェントの従うアルゴリズムを指定できるようにワンクッション置く
        public NoneCollisionTeamHandler(Func<Game, Agent, AgentAlgorithmBase> agent1Algorithm, Func<Game, Agent, AgentAlgorithmBase> agent2Algorithm)
        {
            Agent1Algorithm = agent1Algorithm;
            Agent2Algorithm = agent2Algorithm;
        }

        protected override void GameStarted(Game game, Teams team)
        {
            base.GameStarted(game, team);
            Agent1 = Agent1Algorithm(game, Team.Agent1);
            Agent2 = Agent2Algorithm(game, Team.Agent2);

            InvokeOnUIThread(() =>
            {
                Label1.IsEnabled = true;
                Label2.IsEnabled = true;
            });
        }

        public override TeamHandlerUI ProvideUI()
        {
            return new TeamHandlerUI(Label1, Label2);
        }

        protected override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            Intent intent1 = Agent1.NextIntent(CalculateProhibitedPoints(Team.Agent1).ToArray()),
                    intent2 = Agent2.NextIntent(CalculateProhibitedPoints(Team.Agent2).ToArray());

            ScoringEfficiency scoringEfficiency1 = Agent1.Evaluate()
                            , scoringEfficiency2 = Agent2.Evaluate();

            //自エージェント同士の移動先が重なってしまった場合
            if (intent1.GetNextPoint(Team.Agent1.Position) == intent2.GetNextPoint(Team.Agent2.Position))
            {
                //点数が低い方のエージェントの移動を変える
                if (scoringEfficiency1.Efficiency > scoringEfficiency2.Efficiency)
                {//2を変える
                    List<Point> list = CalculateProhibitedPoints(Team.Agent2);
                    list.Add(intent1.GetNextPoint(Team.Agent1.Position));

                    intent2 = Agent2.NextIntent(list.ToArray());
                    scoringEfficiency2 = Agent2.Evaluate();
                }
                else
                {//1を変える
                    List<Point> list = CalculateProhibitedPoints(Team.Agent1);
                    list.Add(intent2.GetNextPoint(Team.Agent2.Position));

                    intent1 = Agent1.NextIntent(list.ToArray());
                    scoringEfficiency1 = Agent1.Evaluate();
                }
            }
            //盤面に表示
            InvokeOnUIThread(() => Label1.Content = string.Format("Agent1 Effic:{0,4:F2} NTurn:{1,4:F2} Total:{2,4:F2}",
                scoringEfficiency1.Efficiency, scoringEfficiency1.ExpectedTurns ,scoringEfficiency1.Efficiency * scoringEfficiency1.ExpectedTurns));
            InvokeOnUIThread(() => Label2.Content = string.Format("Agent2 Effic:{0,4:F2} NTurn:{1,4:F2} Total:{2,4:F2}", 
                scoringEfficiency2.Efficiency, scoringEfficiency2.ExpectedTurns, scoringEfficiency2.Efficiency * scoringEfficiency2.ExpectedTurns));

            agent1(intent1);
            agent2(intent2);
        }

        protected override void GameFinished()
        {
            base.GameFinished();
            InvokeOnUIThread(() =>
            {
                Label1.IsEnabled = false;
                Label2.IsEnabled = false;
            });
        }


        protected List<Point> CalculateProhibitedPoints(Agent agent)
        {
            List<Point> prohibited = new List<Point>();
            //エージェントの周りのセルにいるエージェントの情報を取得
            Agent anoAgent = Game.GetTeam(agent).GetAnotherAgent(agent);
            Team anoTeam = Game.GetTeam(Game.GetTeamFlag(agent).GetEnemyTeam());
            Point p = agent.Position - anoAgent.Position;

            if (Math.Abs(p.X) <= 1 && Math.Abs(p.Y) <= 1)
            {
                prohibited.Add(anoAgent.Position);
            }

            p = agent.Position - anoTeam.Agent1.Position;

            if (Math.Abs(p.X) <= 1 && Math.Abs(p.Y) <= 1)
            {
                prohibited.Add(anoTeam.Agent1.Position);
            }
            p = agent.Position - anoTeam.Agent2.Position;

            if (Math.Abs(p.X) <= 1 && Math.Abs(p.Y) <= 1)
            {
                prohibited.Add(anoTeam.Agent2.Position);
            }

            return prohibited;
        }
    }
}
