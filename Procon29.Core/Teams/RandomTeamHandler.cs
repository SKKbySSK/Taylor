using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Controls;

namespace Procon29.Core
{
    /// <summary>
    /// ランダムな指示を出す
    /// </summary>
    public class RandomTeamHandler : TeamHandlerBase
    {
        private CheckBox AcceptRemove = new CheckBox()
        {
            Content = "消去動作を許可する",
            IsChecked = true
        };

        private Random random;

        public int RetryCount { get; set; } = 10;

        //コンストラクタ
        public RandomTeamHandler()
        {
            random = new Random(Environment.TickCount);
        }

        protected internal override void Turn(int turn, Action<Intent> agent1, Action<Intent> agent2)
        {
            Intent intent1 = new Intent(), intent2 = new Intent();

            bool acceptRemove = InvokeOnUIThread(() => AcceptRemove.IsChecked) ?? false;
            intent1.Intention = (Intentions)random.Next(0, acceptRemove ? 3 : 2);
            intent2.Intention = (Intentions)random.Next(0, acceptRemove ? 3 : 2);

            int retry = 0;
            Direction d;
            //意志がStay以外だった場合のみ方向を指定(9方向)
            if (intent1.Intention != Intentions.Stay)
            {
                //移動可能と判定されるまでループ
                do
                {
                    //試行回数に達したらその場に留める
                    if (RetryCount <= ++retry)
                    {
                        d = Direction.None;
                        intent1.Intention = Intentions.Stay;
                        break;
                    }

                    d = DirectionSelect(random.Next(1, 10));
                }
                while (!(intent1.Intention == Intentions.Move ? CanMove(Team.Agent1, d) : CanRemove(Team.Agent1, d)));

                intent1.Direction = d;
            }

            retry = 0;
            if (intent2.Intention != Intentions.Stay)
            {
                do
                {
                    if (RetryCount <= ++retry)
                    {
                        d = Direction.None;
                        intent2.Intention = Intentions.Stay;
                        break;
                    }

                    d = DirectionSelect(random.Next(1, 10));
                }
                while (!(intent2.Intention == Intentions.Move ? CanMove(Team.Agent2, d) : CanRemove(Team.Agent2, d)));

                intent2.Direction = d;
            }

            agent1(intent1);
            agent2(intent2);
        }

        //移動方向をテンキーの数字に対応させて指定
        private Direction DirectionSelect(int n)
        {
            switch (n)
            {
                case 1:
                    return Direction.Left | Direction.Down;
                case 2:
                    return Direction.Down;
                case 3:
                    return Direction.Right | Direction.Down;
                case 4:
                    return Direction.Left;
                case 5:
                    return Direction.None;
                case 6:
                    return Direction.Right;
                case 7:
                    return Direction.Left | Direction.Up;
                case 8:
                    return Direction.Up;
                case 9:
                    return Direction.Right | Direction.Up;
                default:
                    return Direction.None;
            }
        }

        public override TeamHandlerUI ProvideUI()
        {
            return new TeamHandlerUI(AcceptRemove);
        }
    }
}
