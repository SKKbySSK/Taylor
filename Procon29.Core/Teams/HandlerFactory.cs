using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core
{
    /// <summary>
    /// <see cref="TeamHandlerBase"/>の生成を担う静的クラス。このクラスはスレッドセーフです
    /// </summary>
    public static class HandlerFactory
    {
        public const string StayName = "Stay";

        public const string ManualName = "Manual";

        public const string RandomName = "Random";

        static HandlerFactory()
        {
            Register(StayName, () => new StayTeamHandler());
            Register(ManualName, () => new ManualTeamHandler());
            Register(RandomName, () => new RandomTeamHandler());
        }

        private static object lockObj = new object();

        private static Dictionary<string, Func<TeamHandlerBase>> dict = new Dictionary<string, Func<TeamHandlerBase>>();

        public static IEnumerable<KeyValuePair<string, Func<TeamHandlerBase>>> GetEnumerable()
        {
            lock (lockObj)
            {
                //dictを返すとスレッドセーフでなくなるため、リストに変換している
                return dict.ToList();
            }
        }
        
        public static bool Register<T>(string name) where T : TeamHandlerBase
        {
            return Register(name, () => Activator.CreateInstance<T>());
        }

        /// <summary>
        /// 新しく生成用の関数を追加します
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        /// <returns>成功したらtrue</returns>
        public static bool Register(string name, Func<TeamHandlerBase> func)
        {
            lock (lockObj)
            {
                if (dict.ContainsKey(name)) return false;

                dict[name] = func;
                return true;
            }
        }

        /// <summary>
        /// 指定した名前と対応するハンドラを生成します
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"/>
        public static TeamHandlerBase CreateHandler(string name)
        {
            lock (lockObj)
            {
                if (dict.ContainsKey(name)) throw new InvalidOperationException();
                return dict[name]();
            }
        }

        /// <summary>
        /// 指定した名前と対応するハンドラの生成を試みます
        /// </summary>
        /// <param name="name"></param>
        /// <param name="teamHandler"></param>
        /// <returns></returns>
        public static bool TryCreateHandler(string name, out TeamHandlerBase teamHandler)
        {
            teamHandler = null;
            try
            {
                teamHandler = CreateHandler(name);
                return true;
            }
            catch (InvalidOperationException) { return false; }
        }
    }
}
