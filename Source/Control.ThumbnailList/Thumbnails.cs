using ImageComparisonViewer.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Control.ThumbnailList
{
    class Thumbnails
    {
        public IReadOnlyList<Thumbnail> Sources { get; }

        public Thumbnails(IReadOnlyList<string> paths)
        {
            Sources = paths.Select(x => new Thumbnail(x)).ToList();
        }

        /// <summary>
        /// サムネイルの読み出し状態を切り替える(Load+Unload)
        /// </summary>
        /// <param name="centerRatio">表示領域の中央位置の割合(0~1)</param>
        /// <param name="viewportRatio">表示領域の割合(0~1)</param>
        public void UpdateThumbnail(double centerRatio, double viewportRatio)
        {
            // 画像が0個のときは0が通知される
            if (centerRatio == 0 || viewportRatio == 0) return;

            var list = Sources;
            int length = list.Count;
            if (length == 0) return;

            //Debug.WriteLine($"Thumbnail Update() center={centerRatio:f2} viewport={viewportRatio:f2}");

            int margin = 1; // 表示マージン(左右に1個余裕持たせる)
            int centerIndex = (int)Math.Floor(length * centerRatio);        // 切り捨て
            int countRaw = (int)Math.Ceiling(length * viewportRatio);       // 切り上げ
            int start = Math.Max(0, centerIndex - (countRaw / 2) - margin); // 一つ余分に描画する
            int end = Math.Min(length - 1, start + countRaw + margin);      // 一つ余分に描画する
            int count = end - start + 1;
            //Debug.WriteLine($"Thumbnail Update() total={length} start={start} end={end} count={count}");

            // 解放リスト(表示範囲外で読込み中)
            var unloadThumbs = Enumerable.Range(0, length)
                .Where(x => !(start <= x && x <= end))
                .Select(x => list[x])
                .Where(x => x.IsLoadImage);
            foreach (var thumb in unloadThumbs)
            {
                //Debug.WriteLine($"Thumbnail Update() Unload: {thumb.FilePath}");
                thumb.UnloadImage();
            }

            // 読込みリスト(表示範囲の未読込みを対象)
            var loadThumbs = Enumerable.Range(start, count)
                .Select(x => list[x])
                .Where(x => x.IsUnloadImage);
            foreach (var thumb in loadThumbs)
            {
                //Debug.WriteLine($"Thumbnail Update() Load: {thumb.FilePath}");
                Task.Run(() => thumb.LoadImage()); // 完了を待たない(高速化)
            }

            // 読み込み状況の表示テスト
            LoadedItemText();
        }

        /// <summary>
        /// 読み込み状況の表示テスト
        /// ◆アイテムが全て画面内に収まっているとScrollChangedが発生せず更新されないが、デバッグ用やからいいや
        /// </summary>

        [Conditional("DEBUG")]
        private void LoadedItemText()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var thumb in Sources)
            {
                sb.Append(thumb.IsUnloadImage ? "□" : "■");
            }
            Debug.WriteLine(sb.ToString());
        }

    }

    class Thumbnail : NotifyPropertyChangedBase
    {
        // サムネイルの最大幅
        private const int ThumbnailWidthMax = 80;

        public BitmapSource? Image
        {
            get => _image;
            private set => SetProperty(ref _image, value);
        }
        private BitmapSource? _image;

        public string FilePath { get; }
        public string Filename { get; }

        public bool IsLoadImage => !(Image is null);
        public bool IsUnloadImage => Image is null;

        public Thumbnail(string path)
        {
            FilePath = path;
            Filename = Path.GetFileName(path);
        }

        public void LoadImage() =>
            Image = FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax);

        public void UnloadImage() => Image = null;

    }

}
