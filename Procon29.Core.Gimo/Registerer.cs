using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Gimo
{
    public static class Registerer
    {
        public static void Register()
        {
            //ここでTeamHandlerBaseを登録してください（このメソッドはUIから自動的に呼び出されます
            //HandlerFactory.Register("name", () => new TeamHandlerBase());
            HandlerFactory.Register("有用領域探索", () => new UsefulRegionSearch());
            HandlerFactory.Register("近傍領域探索", () => new NearRegionSearch());
            HandlerFactory.Register("Wayテスト", () => new WayTest());
        }
    }
}
