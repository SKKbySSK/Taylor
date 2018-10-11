using System.Collections.Generic;
using System.Text;

namespace Procon29.Core.FieldGenerators
{
    /// <summary>
    /// フィールド生成を定義するインターフェイス
    /// </summary>
    public interface IFieldGenerator
    {
        /// <summary>
        /// 横幅
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 縦幅
        /// </summary>
        int Height { get; }

        /// <summary>
        /// フィールド生成する際に呼び出されます
        /// </summary>
        /// <returns>生成されたフィールドのセル情報（[y, x]の順）</returns>
        ICell[,] Generate();
    }
}
