using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Procon29.Core.Extensions;

namespace Procon29.Core
{
    public class IntentResultEventArgs
    {
        public IntentResultEventArgs(Intent[] intents, bool[] results)
        {
            Intents = intents;
            Results = results;
        }

        public Intent[] Intents { get; }

        public bool[] Results { get; }
    }

    /// <summary>
    /// 試合を表すクラス（未完成）
    /// </summary>
    public class Game
    {
        /// <summary>
        /// インテントの結果を一時的に保管するクラス
        /// </summary>
        class IntentResult
        {
            public Team Team { get; set; }

            public Agent Agent { get; set; }

            public Intent Intent { get; set; }

            public Point NextPosition { get; set; }

            public bool Result { get; set; }
        }

        //コンストラクタ
        public Game(Field field, Team team1, Team team2)
        {
            Field = field;
            Team1 = team1;
            Team2 = team2;

            Team1.UnsafePropertyChanged += Team_UnsafePropertyChanged;
            Team2.UnsafePropertyChanged += Team_UnsafePropertyChanged;
        }

        public Game(Export.SerializedGame serializedGame) : this(serializedGame.Field, serializedGame.Team1, serializedGame.Team2)
        {
            Turn = serializedGame.Turn;
            Length = serializedGame.Length;
        }

        private async void Team_UnsafePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Team.TeamHandler))
            {
                if (WriteConsole) Console.WriteLine("Handler changed. awaiting semaphore...");

                await HandlerSemaphore.WaitAsync();

                if (IsGaming)
                {
                    RaiseFinished(TeamHandler1, TeamHandler2);
                }

                TeamHandler1 = Team1.TeamHandler;
                TeamHandler2 = Team2.TeamHandler;

                if (IsGaming)
                {
                    RaiseStarted(TeamHandler1, TeamHandler2);
                }

                HandlerSemaphore.Release();
            }
        }

        public event EventHandler Started;

        public event EventHandler Finished;

        public event EventHandler TurnChanged;

        public event EventHandler<IntentResultEventArgs> Result;

        public bool IsGaming { get; private set; } = false;

        public bool WriteConsole { get; set; } = false;

        public Field Field { get; }

        public Team Team1 { get; }

        public Team Team2 { get; }

        private TeamHandlerBase TeamHandler1 { get; set; }

        private TeamHandlerBase TeamHandler2 { get; set; }

        private SemaphoreSlim HandlerSemaphore { get; } = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 試合の現在のターン。1から始まります
        /// </summary>
        public int Turn { get; private set; } = 0;
        
        /// <summary>
        /// 試合の総ターン。0以下の場合は無制限に行います
        /// </summary>
        public int Length { get; set; } = 100;
        
        public Team GetTeam(Teams team)
        {
            switch (team)
            {
                case Teams.Team1:
                    return Team1;
                case Teams.Team2:
                    return Team2;
                default:
                    throw new Exception("フラグはサポートされていません。Team1またはTeam2のみを指定してください");
            }
        }

        /// <summary>
        /// ゲームを非同期で開始します
        /// </summary>
        /// <param name="length">試合の総ターン数。0以下で無限となります</param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellationToken)
        {
            return Task.Run(action: () => StartInternal(cancellationToken));
        }

        private void StartInternal(CancellationToken cancellationToken)
        {
            Field.GetCell(Team1.Agent1.Position).SetState(Teams.Team1, CellState.Occupied);
            Field.GetCell(Team1.Agent2.Position).SetState(Teams.Team1, CellState.Occupied);
            Field.GetCell(Team2.Agent1.Position).SetState(Teams.Team2, CellState.Occupied);
            Field.GetCell(Team2.Agent2.Position).SetState(Teams.Team2, CellState.Occupied);

            IsGaming = true;
            RaiseStarted(TeamHandler1, TeamHandler2);
            Started?.Invoke(this, new EventArgs());

            int length = Length;
            bool infinite = length <= 0;
            if (infinite)
            {
                length = 1;
                if (WriteConsole) Console.WriteLine("Game Started : Infinity Turns");
            }
            else
                if (WriteConsole) Console.WriteLine("Game Started : " + length + " Turns");

            if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;

            for (Turn = 1; length >= Turn; Turn++)
            {
                //TeamHandler1, 2を排他ロックします
                HandlerSemaphore.Wait();

                if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;

                if (WriteConsole) Console.WriteLine("Turn " + Turn);

                TurnChanged?.Invoke(this, new EventArgs());
                var intents = MakeIntents(Turn, TeamHandler1, TeamHandler2);

                if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;

                IntentResult[] results = new IntentResult[4];

                //インテントを受け取って判断(マップの状態が正しいと仮定しています)
                for (int i = 0; intents.Length > i; i++)
                {
                    var intent = intents[i];
                    bool res = intent.Intention == Intentions.Stay;
                    GetParamsForResult(i, out var team, out var current, out var agent);
                    var teamEnum = team == Team1 ? Teams.Team1 : Teams.Team2;
                    var anotherEnum = (Teams.Team1 | Teams.Team2) & ~teamEnum;

                    if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;

                    if (!res)
                    {
                        Point next = intent.GetNextPoint(current);

                        if (next.X < 0 || next.Y < 0 || next.X >= Field.Width || next.Y >= Field.Height)
                            throw new Exception($"Invalid intent requested : {next}({intent.Intention}, {intent.Direction})");

                        switch (intent.Intention)
                        {
                            case Intentions.Move:
                                current = next;
                                res = true;
                                break;
                            //http://www.procon.gr.jp/wp-content/uploads//2016/12/e017e38ca89f7a0d04d6ad74319ffde0-1.pdf 10ページ目の試合の進行(3).ii.③を参照
                            case Intentions.Remove:
                                var cell = Field.GetCell(next);
                                var state = cell.GetState(anotherEnum);

                                //敵チームのセル状態が占有かどうかを判定
                                if (state == CellState.Occupied)
                                {
                                    current = next;
                                    res = true;
                                }
                                break;
                        }
                    }

                    IntentResult ir = new IntentResult()
                    {
                        Team = team,
                        Agent = agent,
                        Intent = intent,
                        NextPosition = current,
                        Result = res
                    };

                    results[i] = ir;
                }

                if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;
                
                //自分が動けるかをまず確認
                foreach (var res in results)
                {
                    if (res.Intent.Intention == Intentions.Move)
                    {
                        res.Result = this.CanMove(res.Agent, res.Intent.Direction);
                    }
                    else if (res.Intent.Intention == Intentions.Remove)
                    {
                        res.Result = Field.IsInField(res.Agent, res.Intent.Direction);
                    }

                    if (!res.Result)
                        res.NextPosition = res.Agent.Position;
                }

                var list = results.Where(r => r.Result).ToList();
                foreach (var res1 in list)
                {
                    foreach(var res2 in list)
                    {
                        if (res1 != res2)
                        {
                            bool r1 = false, r2 = false;

                            if(res1.NextPosition == res2.NextPosition)
                            {
                                r1 = true;
                                r2 = true;
                            }

                            if (res2.NextPosition == res1.Agent.Position)
                            {
                                r2 = true;
                            }

                            if (r1)
                            {
                                res1.NextPosition = res1.Agent.Position;
                                res1.Result = false;
                            }

                            if (r2)
                            {
                                res2.NextPosition = res2.Agent.Position;
                                res2.Result = false;
                            }
                        }
                    }
                }

                //実際に移動
                foreach (var res in results)
                {
                    if (res.Intent.Intention == Intentions.Move)
                    {
                        res.Agent.Position = res.NextPosition;

                        var team = res.Team == Team1 ? Teams.Team1 : Teams.Team2;
                        var cell = Field.GetCell(res.NextPosition);
                        var state = cell.GetState(team);
                        cell.SetState(team, CellState.Occupied);
                    }
                }

                var remList = results.Where(r => r.Intent.Intention == Intentions.Remove).ToList();
                var agentPos = GetAgentPositions(Team1.Agent1, Team1.Agent2, Team2.Agent1, Team2.Agent2);

                //削除ができるものを実際に削除
                foreach (var rem in remList.Where(r => r.Result))
                {
                    //除去動作は自チームも許可されるため修正
                    var cell = Field.GetCell(rem.NextPosition);
                    cell.State1 = CellState.None;
                    cell.State2 = CellState.None;
                }

                if (Abort(cancellationToken, TeamHandler1, TeamHandler2)) return;

                int intentIndex = 0;
                InvokeTeams(t => t.TeamHandler.IntentResult(results[intentIndex++].Result, results[intentIndex++].Result));
                Result?.Invoke(this, new IntentResultEventArgs(intents, results.Select(r => r.Result).ToArray()));
                
                if (infinite) length++;

                HandlerSemaphore.Release();
            }

            IsGaming = false;
            RaiseFinished(TeamHandler1, TeamHandler2);
            if (WriteConsole) Console.WriteLine("Game Finished");

            Finished?.Invoke(this, new EventArgs());

            Length = 0;
            Turn = 0;
        }

        private bool Abort(CancellationToken token, TeamHandlerBase teamHandler1, TeamHandlerBase teamHandler2)
        {
            bool ret = false;
            if (token.IsCancellationRequested) ret = true;

            if (ret)
            {
                if (WriteConsole) Console.WriteLine("Game Aborted");
                RaiseFinished(teamHandler1, teamHandler2);
                Finished?.Invoke(this, new EventArgs());
            }

            return ret;
        }

        private Point[] GetAgentPositions(params Agent[] agents)
        {
            return agents.Select(a => a.Position).ToArray();
        }

        private void GetParamsForResult(int i, out Team team, out Point current, out Agent agent)
        {
            team = null;
            agent = null;
            switch (i)
            {
                case 0:
                    team = Team1;
                    agent = Team1.Agent1;
                    break;
                case 1:
                    team = Team1;
                    agent = Team1.Agent2;
                    break;
                case 2:
                    team = Team2;
                    agent = Team2.Agent1;
                    break;
                case 3:
                    team = Team2;
                    agent = Team2.Agent2;
                    break;
            }

            current = agent.Position;
        }

        /// <summary>
        /// 各々のチームで次の動作を決定し、<see cref="Intent"/>配列(Agent1, Agent2, Agent1, Agent2の順)を返却します
        /// </summary>
        private Intent[] MakeIntents(int turn, TeamHandlerBase teamHandler1, TeamHandlerBase teamHandler2)
        {
            Intent[] intents = new Intent[4];
            teamHandler1.Turn(turn, i => intents[0] = i, i => intents[1] = i);
            teamHandler2.Turn(turn, i => intents[2] = i, i => intents[3] = i);

            for (int i = 0; intents.Length > i; i++)
            {
                if (intents[i] == null)
                {
                    if (WriteConsole) Console.WriteLine("Intent is null");
                    intents[i] = Intent.StayIntent;
                }
            }

            return intents;
        }

        private void RaiseStarted(TeamHandlerBase teamHandler1, TeamHandlerBase teamHandler2)
        {
            teamHandler1?.GameStarted(this, Teams.Team1);
            teamHandler2?.GameStarted(this, Teams.Team2);
        }

        private void RaiseFinished(TeamHandlerBase teamHandler1, TeamHandlerBase teamHandler2)
        {
            teamHandler1?.GameFinished();
            teamHandler2?.GameFinished();
        }

        /// <summary>
        /// それぞれのチームで同じデリゲートを実行します(Team1, Team2の順)
        /// </summary>
        /// <param name="action"></param>
        private void InvokeTeams(Action<Team> action)
        {
            action(Team1);
            action(Team2);
        }
    }
}
