using System.ComponentModel;

namespace Procon29.Core
{
    /// <summary>
    /// セルを表すインターフェイス（マス目）
    /// </summary>
    public interface ICell : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティが変更された際に発生します。同期コンテキストを無視するためGUIスレッド上で行った場合クラッシュする恐れがあります
        /// </summary>
        event PropertyChangedEventHandler UnsafePropertyChanged;

        System.Threading.SynchronizationContext SynchronizationContext { get; set; }

        CellPriority Priority { get; set; }

        CellState State1 { get; set; }

        CellState State2 { get; set; }

        int Score { get; }

        string Text { get; }

        ICell Clone(bool priority, bool state1, bool state2);

        CellState GetState(Teams team);

        void SetState(Teams team, CellState state);
    }
}
