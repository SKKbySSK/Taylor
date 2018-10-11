using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core;

namespace Procon29.Core.Taku.ClassExtensions
{
    static class FieldExtension
    {
        //
        /// <summary>
        /// エージェントの移動に際してマップの情報を書き換える拡張メソッド
        /// 
        /// 力任せ法用に作成
        /// 相手エージェントの位置は考慮せずに移動させる。なぜなら、力任せ法は先読みであり、相手エージェントがその時そこにいるかなどわからないから
        /// </summary>
        /// <param name="field"></param>
        /// <param name="obj"></param>
        /// <param name="m"></param>
        public static void AgentMove(this Field field, Agent agent, Direction direction, bool evaluate = true)
        {
            //複数回セル情報書き換えるので、一回だけ得点を計算する
            bool remember = field.AutoEvaluate;
            field.AutoEvaluate = false;


            //移動前の場所のタイル情報からどちらのチームのエージェントかを判断
            ICell cell = field.GetCell(agent.Position);
            Teams TeamEnum = (cell.State1 == CellState.Occupied) ? Teams.Team1 : Teams.Team2;
            Teams EnemyEnum = (TeamEnum == Teams.Team1) ? Teams.Team2 : Teams.Team1;

            //エージェントが移動したときの場所
            Point agentNextPos = agent.Position.FastMove(direction);

            //エージェント移動先セルに、相手タイルが置かれているかどうか
            if (field.Map[agentNextPos.Y, agentNextPos.X].GetState(EnemyEnum) == CellState.Occupied)
            {
                //置かれていたら、相手タイルを取り除く
                field.Map[agentNextPos.Y, agentNextPos.X].SetState(EnemyEnum, CellState.None);
            }
            else
            {
                //自チームタイルがまだ置かれていないか
                if (field.Map[agentNextPos.Y, agentNextPos.X].GetState(TeamEnum) != CellState.Occupied)
                {
                    //自チームタイルをおく
                    field.Map[agentNextPos.Y, agentNextPos.X].SetState(TeamEnum, CellState.Occupied);
                }
                //エージェントを移動
                agent.Position = agentNextPos;
            }
            //盤面を評価するか
            if (evaluate)
            {
                field.EvaluateMap(Teams.Team1);
                field.EvaluateMap(Teams.Team2);
            }

            //設定を戻す
            field.AutoEvaluate = remember;
        }
    }
}
