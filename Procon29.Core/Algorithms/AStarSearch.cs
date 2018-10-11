using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Procon29.Core.Extensions;

namespace Procon29.Core.Algorithms
{
    /// <summary>
    ///  A*法（エースター法)を用いて、ある点からある点までの最良経路を探索する
    ///  
    /// 
    /// **************************注意*******************************
    /// 必ずNodesを保持してから探索してください。
    /// スタートとエンドノードはNodesのノードのどれかを指すようにしてください。
    /// プロパティを設定するときに、ノードの設定を変更する必要はないです。
    /// 
    /// </summary>
    public class AStarSearch
    {
        public Node StartNode { get; set; }
        public Node[] EndNodes { get; set; }

        public Node[,] Nodes { get; set; }
        ///スタート位置は含まない,ゴールの位置は含む
        public List<Node> BestWayNodes { get; set; }
        public List<Point> BestWayPoints { get; set; }

        //コンストラクタ
        public AStarSearch()
        {
        }

        /// <summary>
        /// 盤面も同時に作成
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Henght"></param>
        public AStarSearch(int Width, int Height)
        {
            Node[,] nodes = new Node[Height, Width];

            //ノードにデータを入れる
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    nodes[j, i] = new Node();
                }
            }

            Nodes = nodes;
        }
        //Setup
        public void Setup(Node[,] nodes, Node startNode, Node[] endNodes)
        {
            Nodes = nodes;
            StartNode = startNode;
            EndNodes = EndNodes;
        }


        /// <summary>
        /// スタートからゴールたちまでの最善経路を計算する
        /// </summary>
        /// <param name="team"></param>
        /// <param name="point"></param>
        /// <param name="Cost"></param>
        /// <param name="HeuristicCost">推定コスト</param>
        /// <param name="directionMode">false:４方位   true：８方位</param>
        public void Search(Func<Node, double> Cost, Func<Node, double> HeuristicCost)
        {
            Point p = new Point(StartNode.Point);

            Node child, parent;

            //ノードの大きさ
            int Height = Nodes.GetLength(0);
            int Width = Nodes.GetLength(1);

            //初期設定
            //Nodesのノードに自分がいる座標を保持させる
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Nodes[j, i].Point = new Point(i, j);
                }
            }


            //スタートノードに関する設定
            StartNode.Status = NodeStatus.Open;
            //コストを0に
            StartNode.Cost = 0;
            //推定コスト算出
            StartNode.HeuristicCost = HeuristicCost(StartNode);
            //総コスト
            StartNode.TotalCost = StartNode.Cost + StartNode.HeuristicCost;
            //親ノードなし
            StartNode.Parent = null;
            //初手なのでカウントは0
            StartNode.Turn = 0;
            //初手なので必ず成功とする
            StartNode.SuccessProbability = 1;


            //未探索ノード保持（コストが昇順でソートされるようにする）
            List<Node> openedNode = new List<Node>();

            parent = StartNode;
            //エンドノードに到達するまで無限ループ
            while (EndNodes.All((node) => node != parent))
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        //周りのセルの場所を取得
                        p = new Point(parent.Point.X + i, parent.Point.Y + j);

                        //pが配列を溢れないか
                        if (!(p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height) && !(i == 0 && j == 0))
                        {
                            child = Nodes[p.Y, p.X];
                            //周りのセルの状態が未探索である場合(周りのセルがゴール地点である場合もある）
                            if (child.Status == NodeStatus.None)
                            {
                                //子ノードをOpenに
                                child.Status = NodeStatus.Open;
                                //親ノードの指定
                                child.Parent = parent;

                                //いま何ターン目かを設定
                                child.Turn = parent.Turn + 1;

                                //そのセルの実コストと推定コスト、総コストを計算
                                child.Cost = Cost(child) + parent.Cost;
                                child.HeuristicCost = HeuristicCost(child);
                                child.TotalCost = child.Cost + child.HeuristicCost;

                                //childよりコストが高くなる位置を取得
                                var index = openedNode.FindIndex((node) => child.TotalCost < node.TotalCost);

                                //コストで昇順にソート
                                if (index < 0) //自分よりコストが高くなるノードがない->一番うしろにノードを挿入
                                    openedNode.Insert(openedNode.Count, child);
                                else
                                    openedNode.Insert(index, child);

                            }
                        }
                    }
                }

                //親ノードの状態をClosedに
                parent.Status = NodeStatus.Closed;

                //最もコストが低いノードを取り出して次の親ノードとする
                if (openedNode.Any())
                    parent = openedNode.First();
                else
                {   //ノードがない場合＝＝移動ができない場合なので、諦める
                    break;
                }
                //取り出したノードをリストから消去する
                openedNode.RemoveAt(0);
            }

            //parentを辿って、最善経路を取得
            BestWayNodes = new List<Node>();
            //スタートのセルまで続行
            while (parent != StartNode)
            {
                //先頭にノードを追加
                BestWayNodes.Insert(0, parent);
                //parentの親ノードに移動
                parent = parent.Parent;
            }
            //方向のみを取り出す
            BestWayPoints = BestWayNodes.Select((node) => node.Point).ToList();

        }


        //探索結果をリセットする
        public void ResetResult()
        {
            ArrayExtension.ForEach(Nodes, (p, n) =>
            {
                n.Status = NodeStatus.None;
                n.Cost = int.MaxValue;
                n.HeuristicCost = int.MaxValue;
                n.TotalCost = int.MaxValue;
                n.Turn = 0;
                n.SuccessProbability = 1.0;
                n.Parent = null;
            });
            BestWayNodes = null;
            BestWayPoints = null;
        }
       
        public void Clear()
        {
            Nodes = null;
            StartNode = null;
            EndNodes = null;
            BestWayNodes = null;
            BestWayPoints = null;
        }

    }
    /// <summary>
    /// 探索用のノード
    /// </summary>
    public class Node
    {
        public Point Point { get; set; }
        public NodeStatus Status { get; set; }
        public double Cost { get; set; }
        public double HeuristicCost { get; set; }
        public double TotalCost { get; set; }
        public int Turn { get; set; }
        public double SuccessProbability { get; set; }
        public Node Parent { get; set; }

        //コンストラクタ
        public Node()
        {
            Point = new Point();
            Status = NodeStatus.None;
            Cost = int.MaxValue;
            HeuristicCost = int.MaxValue;
            TotalCost = int.MaxValue;
            Turn = 0;
            SuccessProbability = 1.0;
            Parent = null;
        }

        public Node Copy()
        {
            return new Node()
            {
                Point = new Point(Point),
                Status = Status,
                Cost = Cost,
                HeuristicCost = HeuristicCost,
                TotalCost = TotalCost,
                Turn = Turn,
                SuccessProbability = SuccessProbability,
                Parent = Parent
            };
        }
    }

    /// <summary>
    /// ノード状態保持
    /// </summary>
    public enum NodeStatus
    {
        None, Open, Closed
    }
}