using ImageComparisonViewer.Common.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// DirectoryPathNode.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryPathNode : DisposableUserControl
    {
        // ディレクトリの更新を親クラスに通知
        private readonly Action<string> _updateDirectoryAction;

        public DirectoryPathNode(DirectoryNode node, Action<string> action)
        {
            InitializeComponent();

            UpdateDirectoryNode(node);
            _updateDirectoryAction = action;
        }

        /// <summary>
        /// DirectoryNode更新時の処理
        /// </summary>
        private void UpdateDirectoryNode(DirectoryNode node)
        {
            //Button
            DirectorySelectButton.Content = node.AbbrName;
            DirectorySelectButton.CommandParameter = node;

            //ComboBox
            if (node.HasChildDirectory)
            {
                // 子ディレクトリのリストを更新する
                ChildDirectoryComboBox.Visibility = Visibility.Visible;
                ChildDirectoryComboBox.ItemsSource = node.GetChildDirectoryNodes();
            }
            else
            {
                // 子ディレクトリがなければコンボボックスを表示しない
                ChildDirectoryComboBox.Visibility = Visibility.Collapsed;
                ChildDirectoryComboBox.ItemsSource = null;
            }
        }

        /// <summary>
        /// ディレクトリボタン押下時の処理
        /// </summary>
        private void DirectorySelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.CommandParameter is DirectoryNode node))
                throw new ArgumentNullException(nameof(node));

            // 親クラスに選択ディレクトリPATHを通知
            _updateDirectoryAction(node.FullPath);
        }

        /// <summary>
        /// コンボボックス選択時の処理
        /// </summary>
        private void ChildDirectoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChildDirectoryComboBox.SelectedValue is DirectoryNode node)
            {
                // 親クラスに選択ディレクトリPATHを通知
                _updateDirectoryAction(node.FullPath);
            }
        }

    }
}
