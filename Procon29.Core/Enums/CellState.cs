using System;

namespace Procon29.Core
{
    public enum CellState
    {
        /// <summary>
        /// セルは占有されておらず領域内にもありません
        /// </summary>
        None = 0,
        /// <summary>
        /// セルは占有されています
        /// </summary>
        Occupied = 1,
        /// <summary>
        /// セルは領域内にあります
        /// </summary>
        InRegion = 2,
    }
}
