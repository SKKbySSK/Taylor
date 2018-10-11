using System;

namespace Procon29.Core
{
    /// <summary>
    /// 向きを表す列挙体
    /// </summary>
    [Flags]
    public enum Direction
    {
        None = 0,
        Up = 0b0001,
        Down = 0b0010,
        Left = 0b0100,
        Right = 0b1000,
        All = 0b1111
    }
}
