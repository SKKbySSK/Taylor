using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Procon29.Core
{
    public class Agent : Abstracts.SafeNotifyPropertyChanged
    {
        public event EventHandler<MoveEventArgs> Moved;

        int _X;
        public int X
        {
            get => _X;
            set
            {
                if (value == _X) return;
                _X = value;
                //プロパティーの変更通知イベントを発生させる
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Position));
            }
        }

        int _Y;
        public int Y
        {
            get => _Y;
            set
            {
                if (value == _Y) return;
                _Y = value;
                //プロパティーの変更通知イベントを発生させる
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Position));
            }
        }

        public Point Position
        {
            get => new Point(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        
        //directionに基づいてエージェントを移動
        public void Move(Direction direction)
        {
            if (direction.HasFlag(Direction.Up))
                Y--;
            if (direction.HasFlag(Direction.Down))
                Y++;
            if (direction.HasFlag(Direction.Left))
                X--;
            if (direction.HasFlag(Direction.Right))
                X++;

            //移動したことを通知するイベントを発行し、イベントに登録された処理を実行
            Moved?.Invoke(this, new MoveEventArgs(direction));
        }
        //現在のエージェントの位置からdirection方向にsteps分移動したときの場所を返す関数
        public Point GetPosition(Direction direction, int steps)
        {
            Point pos = new Point(X, Y);

            if (direction.HasFlag(Direction.Up))
                pos.Y -= steps;
            if (direction.HasFlag(Direction.Down))
                pos.Y += steps;
            if (direction.HasFlag(Direction.Left))
                pos.X -= steps;
            if (direction.HasFlag(Direction.Right))
                pos.X += steps;

            return pos;
        }
    }
}
