using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ImageComparisonViewer.Core.Images
{
    public interface ICompositeImageDirectory
    {
        /// <summary>
        /// 画像元ディレクトリ(ViewModelで各要素のPropertyChangedを監視)
        /// </summary>
        IList<IImageDirectory> ImageDirectries { get; }

        /// <summary>
        /// ドロップされた複数のPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        /// <returns>画像タブの指定(1画面=1, null=切替えなし)</returns>
        int? SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths);

        /// <summary>
        /// 外部からの回転数通知に応じてコレクション要素をシフトする
        /// </summary>
        /// <param name="contentCount">通知元の最大コンテンツ数(2画面=2)</param>
        /// <param name="rightShiftCount">右シフト回数(0=シフトなし)</param>
        void AdaptImageListTracks(int contentCount, int rightShiftCount);

        /// <summary>
        /// 保持リソースを破棄する(指定された画像グループ以降のリソース破棄に使用)
        /// </summary>
        /// <param name="targetContentCount">対象の画像コンテンツ数(2なら2画面なので3画面以上の情報を削除)</param>
        void ReleaseResources(int targetContentCount);
    }

    public class CompositeImageDirectory : BindableBase, ICompositeImageDirectory
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        //private readonly Guid guid = Guid.NewGuid();

        private static readonly ImageContentBackyard _imageContentBackyard = new ImageContentBackyard();

        /// <summary>
        /// 画像元ディレクトリ(ViewModelで各要素のPropertyChangedを監視)
        /// </summary>
        public IList<IImageDirectory> ImageDirectries { get; } =
            new List<IImageDirectory>(Enumerable.Range(0, DirectriesCountMax).Select(_ => new ImageDirectory(_imageContentBackyard)));

        public CompositeImageDirectory() { }

        /// <summary>
        /// 各画像ディレクトリが1つも読み込まれていないか判定
        /// </summary>
        /// <returns></returns>
        private bool UnloadedAllImageDirectories()
        {
            foreach (var imageDir in ImageDirectries)
            {
                if (imageDir.IsLoaded()) return false;
            }
            return true;
        }

        /// <summary>
        /// ドロップされた複数のPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        /// <returns>画像タブの指定(1画面=1, null=切替えなし)</returns>
        public int? SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths)
        {
            if (baseIndex >= ImageDirectries.Count)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            // 画像が1グループも読み込まれていなかったらドロップ後に画像タブを切り替え(読み込み前に状態を取得)
            var isNavigate = UnloadedAllImageDirectories();

            // ドロップPATH(ディレクトリPATHかも)をファイルPATHに変換
            var filesPath = droppedPaths.Select(x => x.ToImagePath()).ToArray();
            var length = Math.Min(filesPath.Length, ImageDirectries.Count);

            for (int i = 0; i < length; i++)
            {
                int index = (baseIndex + i) % ImageDirectries.Count;
                ImageDirectries[index].SetDroppedFilePath(filesPath[i]);
            }

            return !isNavigate ? default(int?) : length;
        }

        /// <summary>
        /// 外部からの回転数通知に応じてコレクション要素をシフトする
        /// </summary>
        /// <param name="contentCount">通知元の最大コンテンツ数(2画面=2)</param>
        /// <param name="rightShiftCount">右シフト回数(0=シフトなし)</param>
        public void AdaptImageListTracks(int contentCount, int rightShiftCount)
        {
            if (rightShiftCount == 0) return;   // 処理不要
            if (contentCount <= 1) return;      // ループの概念ない
            var tailIndex = contentCount - 1;

            var sourceList = ImageDirectries;
            if (contentCount > sourceList.Count)
                throw new ArgumentOutOfRangeException(nameof(contentCount));

            // 周回する分は捨てる
            rightShiftCount %= contentCount;

            // このメソッド呼ばれてるときはViewからObserveされていないのでコレクションを入れ替え
            if (rightShiftCount > 0)
            {
                for (int i = 0; i < rightShiftCount; i++)
                {
                    var tail = sourceList[tailIndex];
                    for (int j = tailIndex; j > 0; j--)
                    {
                        sourceList[j] = sourceList[j - 1];
                    }
                    sourceList[0] = tail;
                }
            }
            else if (rightShiftCount < 0)
            {
                for (int i = 0; i < -rightShiftCount; i++)
                {
                    var head = sourceList[0];
                    for (int j = 0; j < tailIndex; j++)
                    {
                        sourceList[j] = sourceList[j + 1];
                    }
                    sourceList[tailIndex] = head;
                }
            }
        }

        /// <summary>
        /// 保持リソースを破棄する(指定された画像グループ以降のリソース破棄に使用)
        /// </summary>
        /// <param name="targetContentCount">対象の画像コンテンツ数(2なら2画面なので3画面以上の情報を削除)</param>
        public void ReleaseResources(int targetContentCount)
        {
            if (targetContentCount < 1) return;
            for (int i = targetContentCount; i < ImageDirectries.Count; i++)
            {
                ImageDirectries[i].ReleaseResources();
            }
        }

    }
}
