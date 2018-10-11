using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Taku.Algorithms
{
    static class NumToDirection
    {
        //移動方向をテンキーの数字に対応させて指定
        public static Direction DirectionSelect(int n)
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
    }
}
