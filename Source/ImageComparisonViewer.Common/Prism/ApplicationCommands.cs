using Prism.Commands;

namespace ImageComparisonViewer.Common.Prism
{
    public interface IApplicationCommands
    {
        /// <summary>初期化時のタブの画像表示数(1画面=1, null=切替えなし)</summary>
        int? OnInitializedTabContentImageCount { get; set; }

        /// <summary>タブの画像表示数の切り替えコマンド</summary>
        CompositeCommand NavigateImageTabContent { get; }

        /// <summary>画像グループの右シフトコマンド</summary>
        CompositeCommand RightShiftImageGroupCommand { get; }

        /// <summary>画像グループの左シフトコマンド</summary>
        CompositeCommand LeftShiftImageGroupCommand { get; }

        /// <summary>表示画像の切り替え(進)コマンド</summary>
        CompositeCommand SelectNextImageCommand { get; }

        /// <summary>表示画像の切り替え(戻)コマンド</summary>
        CompositeCommand SelectPrevImageCommand { get; }

        /// <summary>画像ディレクトリの再読込み(F5)</summary>
        CompositeCommand ReloadImageDirectoryCommand { get; }

        /// <summary>画像サチリ部の点滅</summary>
        CompositeCommand ImageBlinkHighlightCommand { get; }
    }

    public class ApplicationCommands : IApplicationCommands
    {
        public int? OnInitializedTabContentImageCount { get; set; }
        public CompositeCommand NavigateImageTabContent { get; } = new CompositeCommand();

        public CompositeCommand RightShiftImageGroupCommand { get; } = new CompositeCommand();
        public CompositeCommand LeftShiftImageGroupCommand { get; } = new CompositeCommand();

        public CompositeCommand SelectNextImageCommand { get; } = new CompositeCommand();
        public CompositeCommand SelectPrevImageCommand { get; } = new CompositeCommand();

        public CompositeCommand ReloadImageDirectoryCommand { get; } = new CompositeCommand();

        public CompositeCommand ImageBlinkHighlightCommand { get; } = new CompositeCommand();

    }
}
