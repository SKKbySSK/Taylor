using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Taku.Algorithms
{
    /// <summary>
    /// 指定されたターン数で取れる行動パターン（移動方向パターン）を列挙する
    /// </summary>
    class EnumerateRoutes
    {
        //ターン数保持
        private int _Turn;
        public int Turn
        {
            get { return _Turn; }
            set
            {
                //値をセットしてリセット
                _Turn = value;
                Reset();
            }
        }
        //手順保持
        private Direction[] Direction { get; set; }
        //手順算出用
        private int[] Counter { get; set; }
        //まだ行動パターンがあるか
        public bool HasNext { get; set; }

        //コンストラクタ
        public EnumerateRoutes()
        {
            Turn = 1;//初期値
        }

        public void Reset()
        {
            //カウンターセット
            Counter = new int[Turn];
            for (int i = 0; i < Turn; i++)
            {
                Counter[i] = 9;
            }

            HasNext = (Turn < 0) ? false : true;
        }

        public Direction[] Next()
        {
            //行動を取り出す
            Direction = Counter.Select((number)=> NumToDirection.DirectionSelect(number)).ToArray();

            //次のパターンの準備
            int num = Turn - 1;
            bool borrow = true;
            while (borrow && num >= 0)
            {
                Counter[num]--;
                switch (Counter[num])
                {
                    case 0:
                        Counter[num] = 9;
                        break;
                    case 5:
                        Counter[num]--;
                        borrow = false;
                        break;
                    default:
                        borrow = false;
                        break;
                }
                num--;
            }

            //すべての行動パターンを列挙したか
            if (borrow)
                HasNext = false;

            return Direction;
        }
    }
}
