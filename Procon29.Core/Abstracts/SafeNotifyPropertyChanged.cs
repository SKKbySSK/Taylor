using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Procon29.Core.Abstracts
{
    /// <summary>
    ///　別スレッドからのプロパティ変更通知を同期コンテキスト上のスレッドで実行します。（UIがクラッシュするのを防ぐ抽象クラス）
    /// </summary>
    public abstract class SafeNotifyPropertyChanged : INotifyPropertyChanged
    {
        public static System.Threading.SynchronizationContext GlobalContext { get; set; }

        //プロパティーの変更を扱うイベント
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler UnsafePropertyChanged;

        //プロパティーの変更を通知するイベントを発生させる関数
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                if (SynchronizationContext != null)
                {
                    SynchronizationContext.Post(o =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }, null);
                }
                else if (GlobalContext != null)
                {
                    GlobalContext.Post(o =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }, null);
                }
                else
                {
                    Console.WriteLine("You should set GlobalContext to avoid ui crashing");
                }
            }

            UnsafePropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 同期コンテキスト
        /// </summary>
        public System.Threading.SynchronizationContext SynchronizationContext { get; set; }

    }
}
