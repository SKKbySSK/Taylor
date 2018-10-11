using System.Windows.Controls;

namespace Procon29.Core
{
    /// <summary>
    /// TeamHandlerのUIを表すクラス
    /// </summary>
    public class TeamHandlerUI
    {
        /// <summary>
        /// UIを配列から作成します。各UIは縦に並ぶ形で上から順に配置されます
        /// </summary>
        /// <param name="elements"></param>
        public TeamHandlerUI(params System.Windows.UIElement[] elements)
        {
            Elements = elements;
        }

        public System.Windows.UIElement[] Elements { get; }

        public StackPanel GetStackPanel()
        {
            StackPanel stackPanel = new StackPanel();

            if (Elements != null && Elements.Length > 0)
            {
                foreach (var e in Elements)
                {
                    stackPanel.Children.Add(e);
                }
            }
            else
            {
                stackPanel.Children.Add(new Label() { Content = "表示するものがありません" });
            }

            return stackPanel;
        }
    }
}