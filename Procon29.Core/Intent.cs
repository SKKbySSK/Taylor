using System;
using System.Collections.Generic;
using System.Linq;

namespace Procon29.Core
{
    /// <summary>
    /// チームが移動する際の意思を管理するクラス
    /// </summary>
    public class Intent
    {
        /// <summary>
        /// その場に居る動作を表します。<see cref="StayIntent"/>プロパティは毎回生成されるため、この内部プロパティを変更しても問題ありません
        /// </summary>
        public static Intent StayIntent => new Intent() { Intention = Intentions.Stay };

        public Intentions Intention { get; set; }

        public Direction Direction { get; set; }

        /// <summary>
        /// <see cref="Intention"/>が<see cref="Intentions.Move"/>または<see cref="Intentions.Remove"/>の時に次に行く座標を返します
        /// マップの領域等は考慮しません
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public Point GetNextPoint(Point current)
        {
            if (Intention != Intentions.Stay)
            {
                current = current.Move(Direction);
            }

            return current;
        }
    }
}
