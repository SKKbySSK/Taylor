using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Helper;

namespace Procon29.Core.Taku.Algorithms
{
    /// <summary>
    /// 探索アルゴリズムで継承してほしい基底クラス
    /// 
    /// エージェント１つにたいして１つの探索
    /// </summary>
    public abstract class SearchAlgorithmBase
    {
        //探索結果の保持
        private Way _Way;
        public Way Way
        {
            get => _Way;
            protected set
            {
                _Way = value;
                NeedTurn = value.MovePoints.Count();
            }
        }

        public Intent Intent { get; protected set; }

        private int _NeedTurn;
        public int NeedTurn
        {
            get => _NeedTurn;
            protected set
            {
                if (_NeedTurn != value)
                {
                    _NeedTurn = value;
                    if (value != 0)
                        Efficiency = _EstimatedEarnScore / value;
                }
            }
        }

        private int _EstimatedTeamScore;
        public int EstimatedTeamScore
        {
            get => _EstimatedTeamScore;
            protected set
            {
                if (_EstimatedTeamScore != value)
                {
                    _EstimatedTeamScore = value;
                    EstimatedEarnScore = value - EstimatedEnemyScore;
                }
            }
        }

        private int _EstimatedEnemyScore;
        public int EstimatedEnemyScore
        {
            get => _EstimatedEnemyScore;
            protected set
            {
                if (EstimatedEnemyScore != value)
                {
                    _EstimatedEnemyScore = value;
                    EstimatedEarnScore = EstimatedTeamScore - value;
                }
            }
        }

        private int _EstimatedEarnScore;
        public int EstimatedEarnScore
        {
            get => _EstimatedEarnScore;
            private set
            {
                if (_EstimatedEarnScore != value)
                {
                    _EstimatedEarnScore = value;
                    if (NeedTurn != 0)
                        Efficiency = ((double)value) / NeedTurn;
                }
            }
        }

        public double Efficiency { get; protected set; }


        //アルゴリズム用に情報を保持
        protected Game Game { get; private set; }

        protected Team Team { get; private set; }   //呼び出し元のチームを保持

        protected Agent Agent { get; private set; } //探索するチームのエージェント

        protected Teams TeamEnum { get; private set; }

        protected Team Enemy { get; private set; }  //敵チームを保持

        protected Teams EnemyEnum { get; private set; }

        protected Field Field => Game?.Field;

        //検索用、必要に応じてオーバーライドしてくださいしてください
        //アルゴリズムを使って経路(Way)を探索する
        public virtual void Search() { }

        //次の指示を求める
        public abstract Intent NextIntent();


        //コンストラクタで、必ずゲームに関する情報を保持させる
        public SearchAlgorithmBase(Game game, Teams team,Agent agent)
        {
            Game = game;

            TeamEnum = team;
            Team = game.GetTeam(team);
            Agent = agent;

            EnemyEnum = (Teams.Team1 | Teams.Team2) & ~team;
            Enemy = game.GetTeam(EnemyEnum);


            //プロパティを設定
            Way = new Way(Agent, Direction.None);
            Intent = new Intent();
        }

        /// <summary>
        /// 指定したエージェントの方向がフィールド内であるかを判定します
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool IsInField(Agent agent, Direction direction)
        {
            var p = agent.Position.Move(direction);

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
    }
}
