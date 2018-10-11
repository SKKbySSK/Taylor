using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Extensions;

namespace Procon29.Core.Taku.ClassExtensions
{
    static class GameExtension
    {
        /// <summary>
        /// エージェントの移動方向から、適切なIntentを求める
        /// </summary>
        /// <param name="game"></param>
        /// <param name="agent"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Intent GetIntent(this Game game,Agent agent,Direction direction)
        {
            Intent Intent = new Intent()
            {
                Intention = Intentions.Stay,
                Direction = direction
            };
            //移動先に移動できれば移動する意志に、敵タイルがあれば除去
            if (game.CanMove(agent, direction))
            {
                Intent.Intention = Intentions.Move;
            }
            else if (game.CanRemove(agent, direction))
            {
                Intent.Intention = Intentions.Remove;
            }

            return Intent;
        }
    }
}
