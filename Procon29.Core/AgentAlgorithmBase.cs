using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Procon29.Core.Extensions;

namespace Procon29.Core
{
    /// <summary>
    /// エージェント単位で行う探索アルゴリズムの基底クラス
    /// Searchを実装するときには、その行動の必要ターン数とゲームの残りターン数を確認して、意味のない行動をしないようにすること！
    /// </summary>
    public abstract class AgentAlgorithmBase
    {
        public AgentAlgorithmBase(Game game, Agent agent)
        {
            Game = game;
            TeamEnum = game.GetTeamFlag(agent);
            EnemyEnum = TeamEnum.GetEnemyTeam();
            Team = game.GetTeam(TeamEnum);
            Enemy = game.GetTeam(EnemyEnum);
            Agent = agent;
        }

        /// <summary>
        /// 対象となる試合
        /// </summary>
        public Game Game { get; }

        /// <summary>
        /// 操作の対象となるエージェント
        /// </summary>
        public Agent Agent { get; }

        /// <summary>
        /// 捜査対象チーム
        /// </summary>
        public Team Team { get; }

        /// <summary>
        /// 敵チーム
        /// </summary>
        public Team Enemy { get;  }  //敵チームを保持

        /// <summary>
        /// 操作対象チームのフラグ
        /// </summary>
        public Teams TeamEnum { get; }

        /// <summary>
        /// 敵チームのフラグ
        /// </summary>
        public Teams EnemyEnum { get; }


        /// <summary>
        /// 現在の得点能率。<see cref="Evaluate"/>が呼び出された際に更新されます
        /// </summary>
        public Genetic.ScoringEfficiency Efficiency { get; private set; }

        /// <summary>
        ///  得点能率を計算した結果を返し、<see cref="Efficiency"/>プロパティを更新します。
        /// </summary>
        /// <returns></returns>
        public Genetic.ScoringEfficiency Evaluate()
        {
            var eff = EvaluateGame();
            Efficiency = eff;
            return eff;
        }

        /// <param name="gameLength">試合の総ターン数</param>
        /// <param name="turn">試合の現在のターン</param>
        protected abstract Genetic.ScoringEfficiency EvaluateGame();

        /// <summary>
        /// 実装したアルゴリズムに従って次のエージェントの行動を取得する
        /// </summary>
        /// <param name="prohibitedAreas">タイル除去・移動禁止エリア指定</param>
        /// <returns>エージェントが取るべき最善の意志</returns>
        public abstract Intent NextIntent(params Point[] prohibitedPoints);

        /// <summary>
        /// 実装したアルゴリズムに従って次のエージェントの行動を探索させる。
        /// </summary>
        /// <param name="prohibitedAreas">タイル除去・移動禁止エリア指定</param>
        /// <returns>エージェントが取るべき最善の意志</returns>
        public abstract Intent Search(params Point[] prohibitedPoints);

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

    }
}
