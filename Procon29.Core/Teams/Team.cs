using System;
using System.Collections.Generic;

namespace Procon29.Core
{
    /// <summary>
    /// チームとチームの試合内での動きを表すクラス
    /// </summary>
    public class Team : Abstracts.SafeNotifyPropertyChanged
    {
        public Team(Point agent1, Point agent2)
        {
            Agent1.Position = agent1;
            Agent2.Position = agent2;
        }

        public Agent Agent1 { get; } = new Agent();

        public Agent Agent2 { get; } = new Agent();

        TeamHandlerBase _teamHandler;
        public TeamHandlerBase TeamHandler
        {
            get => _teamHandler;
            set
            {
                if (value == _teamHandler) return;
                _teamHandler = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// エージェント1, 2を列挙します
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agent> EnumerateAgents()
        {
            yield return Agent1;
            yield return Agent2;
        }
    }
}
