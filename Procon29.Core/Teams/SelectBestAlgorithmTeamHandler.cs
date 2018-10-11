using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Extensions;
using Procon29.Core.Genetic;

namespace Procon29.Core
{
    /// <summary>
    ///　コンストラクタで指定したエージェントアルゴリズムハンドラーの中から最も能率が高いものを選ぶ
    /// </summary>
    public class SelectBestAlgorithmTeamHandler : TeamHandlerBase
    {
        //エージェントアルゴリズムを生成するデリゲート
        private Func<Game, Agent, AgentAlgorithmBase>[] AgentAlgorithmCreators { get; set; }

        public AgentAlgorithmBase[] Agent1Algorithms { get; set; }
        public AgentAlgorithmBase[] Agent2Algorithms { get; set; }

        public SelectBestAlgorithmTeamHandler(params Func<Game, Agent, AgentAlgorithmBase>[] agentAlgorithms)
        {
            AgentAlgorithmCreators = agentAlgorithms;
        }

        protected internal override void GameStarted(Game game, Teams team)
        {
            base.GameStarted(game, team);
            //取得しているエージェントデリゲートの数を取得
            int count = AgentAlgorithmCreators.Count();

            //デリゲートが設定されているか
            if (count > 0)
            {
                //エージェントのインスタンスたちをデリゲートから生成
                Agent1Algorithms = new AgentAlgorithmBase[count];
                Agent2Algorithms = new AgentAlgorithmBase[count];

                //エージェントアルゴリズムのインスタンスたちをセット
                for (int i = 0; i < count; i++)
                {
                    Agent1Algorithms[i] = AgentAlgorithmCreators[i](Game, Team.Agent1);
                    Agent2Algorithms[i] = AgentAlgorithmCreators[i](Game, Team.Agent2);
                }
            }
            else
            {
                throw new Exception("デリゲートが一つも設定されていません！");
            }

        }

        public override TeamHandlerUI ProvideUI()
        {
            return null;
        }

        protected internal override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            /*
             *  最善手の組を選ぶ方法（愚直法）：
             *  エージェントごとに、味方のもう一方のエージェントと移動先が重なる可能性のある場所（最高で4つ存在）の一つ一つを禁止領域として、最善手を計算する。
             *  それらの生成したアルゴリズムをエージェントごとにリストに追加する。
             *  その２つのリストをそれぞれアルゴリズムの能率で降順ソートする。
             *  ソート済み２つのそれぞれのリストの中から先頭要素を取り出し（先頭が最も能率が高いため）、もう一方のエージェントと移動先がかぶっていないかを確認。
             *  移動先がかぶっていなければそれでおｋ。
             *  移動先がかぶっていたら得点が低いほうのエージェントのリストの次の要素を取り出す。これが移動先が重複しなくなるまで行う。
             *  
             *  最善手の組を選ぶ方法（ちょっとまし）：
             *  エージェントそれぞれに対して、アルゴリズムたちを実行し、そのアルゴリズムの中での最善手を求める。
             *  それらの最善手をエージェントごとにリストに降順で追加する
             *  それぞれのエージェントの最善手をリストから取得する。
             *  その２つの最善手の移動先が重複しているか確認。
             *  重複していなければそれで決定。
             *  重複していた場合、重複を許さない場合の最善手の組を取得する。
             *  その方法：
             *  移動先が重複しているものの組を見つけ、それぞれの迂回ルートを探索し、エージェントごとにリストに保持する。
             *  今まで作成した最善手のリストの先頭から最善手を取得していき、一方のエージェントの最善手を固定したときに他方のエージェントと移動先が重ならない最善手を求める。
             *  同じことを役割を逆にして行う。
             *  上の２つの最善手の中から、得点が高い方が最善手である。
             *  
             *  
             *  マスごとの最善手を取得する。ますごとの最善手のなかから、利得が最も大きくなる手段を求める。
             *  マスごとのすべての
             *  
             *  
             *  
             *  
             *  マスごとに最善手を埋めながら、最善手の組が確定した時点で終了
             *  最善手の組というのは、他の組の値のどれよりも、値が大きいか等しい
             *  
             */

            //選択アルゴリズム候補保持
            List<(Intent Intent, ScoringEfficiency ScoringEfficiency, AgentAlgorithmBase Algorithm)> Agent1IntentCandidates
                = new List<(Intent, ScoringEfficiency, AgentAlgorithmBase)>();
            List<(Intent Intent, ScoringEfficiency ScoringEfficiency, AgentAlgorithmBase Algorithm)> Agent2IntentCandidates
                = new List<(Intent, ScoringEfficiency, AgentAlgorithmBase)>();

            //エージェントごとに移動不可エリアの取得
            var agent1ProhibitedPoints = CalculateProhibitedPoints(Team.Agent1);
            var agent2ProhibitedPoints = CalculateProhibitedPoints(Team.Agent2);

            var agent1ProhibitedPointsArray = agent1ProhibitedPoints.ToArray();
            var agent2ProhibitedPointsArray = agent2ProhibitedPoints.ToArray();

            //エージェントごとに、アルゴリズムから最善手を取得するとともに、同時に降順でソート
            foreach (var a in Agent1Algorithms)
            {
                var n = (a.NextIntent(agent1ProhibitedPointsArray), a.Evaluate(), a);
                var count = Agent1IntentCandidates.FindIndex(i => i.ScoringEfficiency.Efficiency < n.Item2.Efficiency);
                Agent1IntentCandidates.Insert(count >= 0 ? count : 0, n);
            }
            foreach (var a in Agent2Algorithms)
            {
                var n = (a.NextIntent(agent2ProhibitedPointsArray), a.Evaluate(), a);
                var count = Agent2IntentCandidates.FindIndex(i => i.ScoringEfficiency.Efficiency < n.Item2.Efficiency);
                Agent2IntentCandidates.Insert(count >= 0 ? count : 0, n);
            }

            //最善手を取得して、エージェント間の移動先が重複していなければ行動が確定
            if (Agent1IntentCandidates.First().Intent.GetNextPoint(Team.Agent1.Position) != Agent2IntentCandidates.First().Intent.GetNextPoint(Team.Agent2.Position))
            {
                agent1(Agent1IntentCandidates.First().Intent);
                agent2(Agent2IntentCandidates.First().Intent);
            }
            else  //移動先が重複する場合
            {
                //迂回する
                var Agent1DetourIntent = new List<(Intent Intent, ScoringEfficiency ScoringEfficiency, AgentAlgorithmBase Algorithm)>();
                var Agent2DetourIntent = new List<(Intent Intent, ScoringEfficiency ScoringEfficiency, AgentAlgorithmBase Algorithm)>();

                //衝突する可能性があるアルゴリズムを探索
                foreach (var a1 in Agent1IntentCandidates)
                {
                    foreach (var a2 in Agent2IntentCandidates)
                    {
                        //エージェントどうしで衝突したもの
                        if (a1.Intent.GetNextPoint(Team.Agent1.Position) == a2.Intent.GetNextPoint(Team.Agent2.Position))
                        {
                            //まだ追加していなければリストに追加
                            if (Agent1DetourIntent.Any(n => n.Algorithm == a1.Algorithm))
                            {
                                Agent1DetourIntent.Add(a1);
                            }
                            //まだ追加していなければリストに追加
                            if (Agent2DetourIntent.Any(n => n.Algorithm == a2.Algorithm))
                            {
                                Agent2DetourIntent.Add(a2);
                            }
                        }
                    }
                }
                int i = 0;

                List<(int Index, AgentAlgorithmBase Algorithm)> exchangeAgent1Algorithm = new List<(int Index, AgentAlgorithmBase Algorithm)>();
                List<(int Index, AgentAlgorithmBase Algorithm)> exchangeAgent2Algorithm = new List<(int Index, AgentAlgorithmBase Algorithm)>();


                //衝突する可能性のあるアルゴリズムを迂回させたときの最善手を計算
                foreach (var a1 in Agent1DetourIntent)
                {
                    var index = Array.IndexOf(Agent1Algorithms, a1.Algorithm);   //迂回させるのに用いるアルゴリズムを生成するジェネレータの添字を保持
                    var algorithm = AgentAlgorithmCreators[index](Game, Team.Agent1);
                    var prohibited = agent1ProhibitedPoints.Concat(new List<Point> { a1.Intent.GetNextPoint(Team.Agent1.Position) });

                    var Intent = algorithm.NextIntent(prohibited.ToArray());
                    var Efficiency = algorithm.Evaluate();

                    //最終的に、選んだ最善手が迂回ルートであった場合に、Agent1Algorithmsのアルゴリズムと入れ替える必要があるので保持
                    exchangeAgent1Algorithm.Add((index, algorithm));
                    //迂回ルートを、最善ルート候補に追加(降順になるように挿入)
                    Agent1IntentCandidates.Insert(Agent1IntentCandidates.FindIndex(n => n.ScoringEfficiency.Efficiency < Agent1DetourIntent[i].ScoringEfficiency.Efficiency), Agent1DetourIntent[i]);
                    i++;
                }
                i = 0;
                foreach (var a2 in Agent2DetourIntent)
                {
                    var index = Array.IndexOf(Agent2Algorithms, a2.Algorithm);   //迂回させるのに用いるアルゴリズムを生成するジェネレータの添字を保持
                    var algorithm = AgentAlgorithmCreators[index](Game, Team.Agent2);
                    var prohibited = agent2ProhibitedPoints.Concat(new List<Point> { a2.Intent.GetNextPoint(Team.Agent2.Position) });

                    var Intent = algorithm.NextIntent(prohibited.ToArray());
                    var Efficiency = algorithm.Evaluate();

                    //最終的に、選んだ最善手が迂回ルートであった場合に、Agent2Algorithmsのアルゴリズムと入れ替える必要があるので保持
                    exchangeAgent2Algorithm.Add((index, algorithm));
                    //迂回ルートを、最善ルート候補に追加(降順になるように挿入)
                    Agent2IntentCandidates.Insert(Agent2IntentCandidates.FindIndex(n => n.ScoringEfficiency.Efficiency < Agent2DetourIntent[i].ScoringEfficiency.Efficiency), Agent2DetourIntent[i]);
                    i++;
                }

                //Agent1を最善、Agent2を迂回させたときの最高能率
                int j;
                var Agent1BestIntent = Agent1IntentCandidates.First();
                i = j = 0;
                while (Agent1BestIntent.Intent.GetNextPoint(Team.Agent1.Position) == Agent2IntentCandidates[i].Intent.GetNextPoint(Team.Agent2.Position)) i++;

                var Agent2BestIntent = Agent2IntentCandidates.First();
                while (Agent2BestIntent.Intent.GetNextPoint(Team.Agent1.Position) == Agent1IntentCandidates[j].Intent.GetNextPoint(Team.Agent2.Position)) j++;

                (Intent, ScoringEfficiency, AgentAlgorithmBase) bestAgent1Algorithm, bestAgent2Algorithm;

                int selectedIndex1, selectedIndex2;

                //能率が高い方を最終的な最善手とする
                if (Agent1BestIntent.ScoringEfficiency.Efficiency + Agent2IntentCandidates[i].ScoringEfficiency.Efficiency >
                    Agent2BestIntent.ScoringEfficiency.Efficiency + Agent1IntentCandidates[j].ScoringEfficiency.Efficiency)
                {
                    //Agent2を迂回させる場合
                    bestAgent1Algorithm = Agent1BestIntent;
                    bestAgent2Algorithm = Agent2IntentCandidates[i];


                    //迂回させる場合
                    if (exchangeAgent2Algorithm.Any((ex) => ex.Algorithm == bestAgent2Algorithm.Item3))
                    {
                        var d = exchangeAgent2Algorithm.Find((ex) => ex.Algorithm == bestAgent2Algorithm.Item3);
                        Agent2Algorithms[d.Index] = bestAgent2Algorithm.Item3;
                        selectedIndex2 = d.Index;                    //選択されたアルゴリズムを保持
                    }
                    else
                    {
                        //選択されたアルゴリズムを保持
                        selectedIndex2 = Array.IndexOf(Agent2Algorithms, bestAgent2Algorithm);
                    }
                    //選択されたアルゴリズムを保持
                    selectedIndex1 = Array.IndexOf(Agent1Algorithms, bestAgent1Algorithm);
                }
                else
                {
                    //Agent1を迂回させる場合
                    bestAgent2Algorithm = Agent2BestIntent;
                    bestAgent1Algorithm = Agent1IntentCandidates[j];

                    //迂回させる場合
                    if (exchangeAgent1Algorithm.Any((ex) => ex.Algorithm == bestAgent1Algorithm.Item3))
                    {
                        var d = exchangeAgent1Algorithm.Find((ex) => ex.Algorithm == bestAgent1Algorithm.Item3);
                        Agent1Algorithms[d.Index] = bestAgent1Algorithm.Item3;
                        selectedIndex1 = d.Index;                    //選択されたアルゴリズムを保持
                    }
                    else
                    {
                        //選択されたアルゴリズムを保持
                        selectedIndex1 = Array.IndexOf(Agent1Algorithms, bestAgent1Algorithm);
                    }
                    //選択されたアルゴリズムを保持
                    selectedIndex2 = Array.IndexOf(Agent2Algorithms, bestAgent2Algorithm);
                }
                //この行動に用いたアルゴリズム以外は入れ替える
                for (int k = 0; i < Agent1Algorithms.Count(); i++)
                {
                    if (k != selectedIndex1)
                        Agent1Algorithms[k] = AgentAlgorithmCreators[k](Game, Team.Agent1);
                    if(k != selectedIndex2)
                        Agent2Algorithms[k] = AgentAlgorithmCreators[k](Game, Team.Agent2);
                }


                //エージェントの行動を実行
                agent1(bestAgent1Algorithm.Item1);
                agent2(bestAgent2Algorithm.Item1);
            }
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
