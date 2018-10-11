using System;

namespace Procon29.Core
{
    /// <summary>
    /// チームを表す列挙体
    /// </summary>
    [Flags]
    public enum Teams
    {
        Team1 = 0b0000_0001,
        Team2 = 0b0000_0010
    }
}
