using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ImageComparisonViewer.Core.Images
{
    public class CompositeImageDirectory : BindableBase
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        //private readonly Guid guid = Guid.NewGuid();

        /// <summary>
        /// 画像元ディレクトリ(ViewModelで各要素のPropertyChangedを監視)
        /// </summary>
        public IList<ImageDirectory> ImageDirectries { get; } =
            new List<ImageDirectory>(Enumerable.Range(0, DirectriesCountMax).Select(_ => new ImageDirectory()));

        public CompositeImageDirectory() { }

        /// <summary>
        /// ドロップされた複数のPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        public void SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths)
        {
            if (baseIndex >= ImageDirectries.Count)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            // ドロップPATH(ディレクトリPATHかも)をファイルPATHに変換
            var filesPath = droppedPaths.Select(x => x.ToImagePath()).ToArray();
            var length = Math.Min(filesPath.Length, ImageDirectries.Count);

            for (int i = 0; i < length; i++)
            {
                int index = (baseIndex + i) % ImageDirectries.Count;
                ImageDirectries[index].SelectedFilePath = filesPath[i];
            }
        }

        /// <summary>
        /// 外部からの回転数通知に応じてコレクション要素をシフトする
        /// </summary>
        /// <param name="contentCount">通知元の最大コンテンツ数(2画面=2)</param>
        /// <param name="rightShiftCount">右シフト回数</param>
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
