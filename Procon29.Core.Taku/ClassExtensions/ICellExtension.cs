using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29.Core.Taku.ClassExtensions
{
    static class ICellExtension
    {
        public static ICell[,] Clones(this ICell[,] a1)
        {
            ICell[,] a2 = new ICell[a1.GetLength(0), a1.GetLength(1)];
            for (int i = 0; i < a1.GetLength(0); i++)
            {
                for (int j = 0; j < a1.GetLength(1); j++)
                {
                    a2[i, j] = a1[i, j].Clone(true,true,true);
                }
            }
            return a2;
        }
    }
}
