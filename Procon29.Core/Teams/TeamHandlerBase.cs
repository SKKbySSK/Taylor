using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Procon29.Core
{
    /*
     * TeamHandlerBaseライフサイクル
     * 
     * ゲーム開始
     * ↓
     * GameStarted()
     * ｜
     * ｜←――――――
     * ↓　　　　　　　｜
     * Turn()　　　　　｜
     * ↓　　　　　　　｜
     * IntentResult()　｜
     * ｜　　　　　　　｜
     * ｜――――――→
     * ↓
     * GameFinished
     * 
     */

    /// <summary>
    /// チームのゲーム内での動きを取り扱う抽象クラス
    /// ひとチーム１つ保持
    /// </summary>
    public abstract class TeamHandlerBase
    {
        public event EventHandler CellColorsChanged;

        public event EventHandler CellUIChanged;

        public CellColor[] CellColors { get; private set; }

        protected Game Game { get; private set; }
       
        protected Team Team { get; private set; }   //呼び出し元のチームを保持

        protected Teams TeamEnum { get; private set; }

        protected Team Enemy { get; private set; }  //敵チームを保持

        protected Teams EnemyEnum { get; private set; }

        protected Field Field => Game?.Field;

        protected internal virtual void GameStarted(Game game, Teams team)
        {
            Game = game;

            TeamEnum = team;
            Team = game.GetTeam(team);

            EnemyEnum = (Teams.Team1 | Teams.Team2) & ~team;
            Enemy = game.GetTeam(EnemyEnum);
        }
        
        protected internal virtual void GameFinished()
        {
            Game = null;
            Team = null;
            Enemy = null;
            SetCellColors(null);
        }

        /// <summary>
        /// UIスレッド上で指定された関数を実行します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected T InvokeOnUIThread<T>(Func<T> func)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                return func();
            }

            TaskCompletionSource<T> task = new TaskCompletionSource<T>();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                task.SetResult(func());
            }));

            return task.Task.Result;
        }

        /// <summary>
        /// UIスレッド上で指定された関数を実行します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected void InvokeOnUIThread(Action action)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == Application.Current.Dispatcher.Thread.ManagedThreadId)
            {
                action();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                action();
            }));
        }

        /// <summary>
        /// 設定用のUI要素を返す
        /// </summary>
        /// <returns></returns>
        public abstract TeamHandlerUI ProvideUI();

        /// <summary>
        /// セルのUI変更を通知する際に呼び出してください
        /// </summary>
        protected void OnCellUIChanged()
        {
            CellUIChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// セルのUI上での色を設定します
        /// </summary>
        /// <param name="colors"></param>
        protected void SetCellColors(params CellColor[] colors)
        {
            CellColors = colors;
            InvokeOnUIThread(() => CellColorsChanged?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// 指定したエージェントの方向がフィールド内であるかを判定します
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool IsInField(Agent agent, Direction direction)
        {
            return IsInField(agent.Position, direction);
        }

        /// <summary>
        /// 指定した点を中心として移動した方向がフィールド内であるかを判定します
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool IsInField(Point point, Direction direction)
        {
            var p = point.Move(direction);

            if (p.X < 0 || p.Y < 0 || p.X >= Field.Width || p.Y >= Field.Height)
                return false;

            return true;
        }

        /// <summary>
        /// 指定したエージェントの方向にあるセルへ移動可能かを取得します
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected bool CanMove(Agent agent, Direction direction)
        {
            if (IsInField(agent, direction))
            {
                Teams team = agent == Team.Agent1 || agent == Team.Agent2 ? EnemyEnum : TeamEnum;

                var p = agent.Position.Move(direction);
                var cell = Field.GetCell(p);
                return cell.GetState(team) != CellState.Occupied;
            }

            return false;
        }

        /// <summary>
        /// 指定したエージェントの方向にあるセルが撤去可能かを取得します
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected bool CanRemove(Agent agent, Direction direction)
        {
            if (direction == Direction.None) return false;

            if (IsInField(agent, direction))
            {
                var p = agent.Position.Move(direction);
                return Field.GetCell(p).GetState(EnemyEnum).HasFlag(CellState.Occupied);
            }

            return false;
        }

        /// <summary>
        /// ターンが回ってきたときに呼び出されます
        /// メモ：Action経由にした理由はGUI操作を行う場合に別スレッド動作になるため、待機する必要があるからです
        /// </summary>
        /// <param name="turn">何回目のターンかを表します</param>
        /// <param name="agent1"><see cref="Team.Agent1"/>の行う動作を引数に渡して実行してください</param>
        /// <param name="agent2"><see cref="Team.Agent2"/>の行う動作を引数に渡して実行してください</param>
        protected internal abstract void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2);

        /// <summary>
        /// 直前に行った動作の結果を返却します。各引数はtrueで成功、falseで失敗を表します
        /// </summary>
        /// <param name="agent1"></param>
        /// <param name="agent2"></param>
        protected internal virtual void IntentResult(bool agent1, bool agent2) { }
    }
}