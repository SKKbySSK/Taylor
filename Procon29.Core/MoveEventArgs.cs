using System;

namespace Procon29.Core
{
    /// <summary>
    /// 移動を表すイベントのデータ
    /// </summary>
    public class MoveEventArgs : EventArgs
    {
        //コンストラクタ
        public MoveEventArgs(Direction direction)
        {
            Direction = direction;
        }

        public Direction Direction { get; }
    }
}
