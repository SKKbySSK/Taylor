using System;
using System.Collections.Generic;
using System.Text;

namespace Procon29.Core.Helper
{
    /// <summary>
    /// QR文字列を解析するクラス
    /// </summary>
    public static class QRParser
    {
        class Generator : FieldGenerators.IFieldGenerator
        {
            public Generator(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }

            public int Height { get; }

            public ICell[,] Cells { get; set; }

            public ICell[,] Generate()
            {
                return Cells;
            }
        }

        /// <summary>
        /// QRコード文字列を解析してフィールドへ変換します
        /// </summary>
        /// <param name="value">QRコード文字列</param>
        /// <returns>成功した場合はtrue</returns>
        public static bool TryParse(string value, out Game game)
        {
            game = null;
            try
            {
                game = Parse(value);
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// QRコード文字列を解析してフィールドへ変換します
        /// </summary>
        /// <param name="value">QRコード文字列</param>
        /// <returns>フィールド</returns>
        public static Game Parse(string value)
        {
            var spl = value.Split(' ', ':');

            int i = 0;
            int h = int.Parse(spl[i++]);
            int w = int.Parse(spl[i++]);

            var gen = new Generator(w, h);

            ICell[,] cells = new ICell[h, w];
            for (int y = 0; h > y; y++)
            {
                for (int x = 0; w > x; x++)
                {
                    cells[y, x] = new ScoreCell(int.Parse(spl[i++]));
                }
            }

            gen.Cells = cells;

            var field = new Field(gen);

            int y1 = int.Parse(spl[i++]);
            int x1 = int.Parse(spl[i++]);
            int y2 = int.Parse(spl[i++]);
            int x2 = int.Parse(spl[i++]);

            cells[--y1, --x1].SetState(Teams.Team1, CellState.Occupied);
            cells[--y2, --x2].SetState(Teams.Team1, CellState.Occupied);
            cells[y1, x2].SetState(Teams.Team2, CellState.Occupied);
            cells[y2, x1].SetState(Teams.Team2, CellState.Occupied);

            return new Game(field, new Team(new Point(x1, y1), new Point(x2, y2)), new Team(new Point(x2, y1), new Point(x1, y2)));
        }
    }
}
