using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Control.ThumbnailList
{
    class Thumbnails : MyBindableBase
    {
        public IList<Thumbnail> Items { get; } = new List<Thumbnail>();


        public Thumbnails(IReadOnlyCollection<string> paths)
        {
            foreach (var path in paths)
            {
                Items.Add(new Thumbnail(path));
            }

        }

        public void UpdateThumbnail(double centerRatio, double viewportRatio)
        {
            if (centerRatio == 0) throw new ArgumentException(nameof(centerRatio));
            if (viewportRatio == 0) throw new ArgumentException(nameof(viewportRatio));

            var list = Items;
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
            var unloads = Enumerable.Range(0, length)
                .Where(x => !(start <= x && x <= end))
                .Select(x => list[x])
                .Where(x => !x.IsThumbnailEmpty);
            foreach (var source in unloads)
            {
                //Debug.WriteLine($"Thumbnail Update() Unload: {source.FilePath}");
                source.UnloadThumbnail();
            }

            // 読込みリスト(表示範囲の未読込みを対象)
            var loads = Enumerable.Range(start, count)
                .Select(x => list[x])
                .Where(x => x.IsThumbnailEmpty);
            foreach (var source in loads)
            {
                //Debug.WriteLine($"Thumbnail Update() Load: {source.FilePath}");
                source.LoadThumbnail();
            }

            // 読み込み状況の表示テスト
            // ◆アイテムが全て画面内に収まっているとScrollChangedが発生せず更新されないが、テスト用やからいいや
            LoadedItemText();
        }


        #region テスト

        public string? LoadStatus
        {
            get => _loadStatus;
            private set => SetProperty(ref _loadStatus, value);
        }
        private string? _loadStatus;

        private void LoadedItemText()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var source in Items)
            {
                sb.Append(source.IsThumbnailEmpty ? "□" : "■");
            }
            LoadStatus = sb.ToString();
            Debug.WriteLine(LoadStatus);
        }

        #endregion

    }

    class Thumbnail : MyBindableBase
    {
        public BitmapSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private BitmapSource? _image;
        public string? FilePath { get; }
        public string? Filename { get; }

        public bool IsThumbnailEmpty => Image is null;
        public Thumbnail(string path)
        {
            Image = null;
            FilePath = path;
            Filename = Path.GetFileName(path);
        }
        public void LoadThumbnail()
        {
            Image = new BitmapImage(new Uri(@"C:\data\Image0.JPG"));
        }

        public void UnloadThumbnail()
        {
            Image = null;
        }
    }

}
