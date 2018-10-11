using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Procon29.Core.Export
{
    public class SerializedGame
    {
        public SerializedGame(Field field, Team team1, Team team2, int lenth, int turn)
        {
            Field = field;
            Team1 = team1;
            Team2 = team2;
            Length = lenth;
            Turn = turn;
        }

        public int Turn { get; }

        public int Length { get; }

        public Field Field { get; }

        public Team Team1 { get; }

        public Team Team2 { get; }

        public Game CreateGame()
        {
            return new Game(this);
        }
    }

    public static class GameSerializer
    {
        private const int Version = 2;

        public static void Serialize(Game game, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                Serialize(game, fs);
            }
        }

        public static SerializedGame Deserialize(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Deserialize(fs);
            }
        }

        public static void Serialize(Game game, Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write(Version);
                bw.Write(game.Length);
                bw.Write(game.Turn);

                var f = game.Field;
                bw.Write(f.Width);
                bw.Write(f.Height);

                foreach (var c in f.Map)
                {
                    bw.Write(c.Score);
                    bw.Write(c.Text);
                    bw.Write((int)c.Priority);
                    bw.Write((int)c.State1);
                    bw.Write((int)c.State2);
                }

                WriteAgent(game.Team1.Agent1);
                WriteAgent(game.Team1.Agent2);
                WriteAgent(game.Team2.Agent1);
                WriteAgent(game.Team2.Agent2);

                void WriteAgent(Agent agent)
                {
                    bw.Write(agent.Position.X);
                    bw.Write(agent.Position.Y);
                }
            }
        }

        public static SerializedGame Deserialize(Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                var version = br.ReadInt32();

                if (version == 1)
                    throw new Exception("互換性が無いので削除してください");
                
                var len = br.ReadInt32();
                var turn = br.ReadInt32();

                var width = br.ReadInt32();
                var height = br.ReadInt32();

                ICell[,] map = DeserializeCells(br, width, height);

                Point agent11 = ReadAgent(), agent12 = ReadAgent(), agent21 = ReadAgent(), agent22 = ReadAgent();

                Point ReadAgent()
                {
                    return new Point(br.ReadInt32(), br.ReadInt32());
                }

                return new SerializedGame(new Field(map), new Team(agent11, agent12), new Team(agent21, agent22), len, turn);
            }
        }
        
        private static ICell[,] DeserializeCells(BinaryReader br, int width, int height)
        {
            ICell[,] map = new ICell[height, width];
            for (int y = 0; height > y; y++)
            {
                for (int x = 0; width > x; x++)
                {
                    var score = br.ReadInt32();
                    var text = br.ReadString();
                    var priority = (CellPriority)br.ReadInt32();
                    var state1 = (CellState)br.ReadInt32();
                    var state2 = (CellState)br.ReadInt32();

                    map[y, x] = new ScoreCell(score)
                    {
                        Priority = priority,
                        State1 = state1,
                        State2 = state2,
                    };
                }
            }

            return map;
        }
    }
}
