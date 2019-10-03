using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Control.ThumbnailList
{
#if false
    class ImageSources : BindableBase
    {
        private const string DirPath = @"C:\data";

        public ReadOnlyObservableCollection<ImageSource> Sources =>
            new ReadOnlyObservableCollection<ImageSource>(_sources);

        private readonly ObservableCollection<ImageSource> _sources =
            new ObservableCollection<ImageSource>();

        public string? SelectedImagePath
        {
            get => _selectedImagePath;
            set => SetProperty(ref _selectedImagePath, value);
        }
        private string? _selectedImagePath;

        public ImageSources() { }

        public void Initialize(string dirPath = DirPath)
        {
            _sources.Clear();

            // ImageSourceのコンストラクタ内ではサムネイルを読み込まない
            foreach (var path in GetImagePaths(dirPath))
            {
                _sources.Add(new ImageSource(path));
            }

            // とりあえず先頭画像を選択しとく
            if (_sources.Any())
            {
                SelectedImagePath = _sources.First().FilePath;
            }
        }

        private static IEnumerable<string> GetImagePaths(string directoryPath)
        {
            var pat = ".jpg";
            var images = new List<string>();

            // patは "*.jpg" の形式にする
            foreach (var file in new DirectoryInfo(directoryPath).GetFiles($"*{pat}", SearchOption.TopDirectoryOnly))
            {
                // tiff画像が ".tif" と ".tiff" で二重検出されるので完全一致をチェック
                if (Path.GetExtension(file.FullName).ToLower() == pat)
                    images.Add(file.FullName);
            }
            return images.OrderBy(f => f);
        }

        // 表示候補画像の解放と読み出し
        public void UpdateThumbnail(double centerRatio, double viewportRatio)
        {
            if (centerRatio == 0) throw new ArgumentException(nameof(centerRatio));
            if (viewportRatio == 0) throw new ArgumentException(nameof(viewportRatio));

            var list = _sources;
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

        // リストからファイルをクリアする(実ファイルは削除されない)
        public bool ClearImageFile(string sourcePath)
        {
            // クリア後にアイテムがなくなるのは禁止(◆削除後の動作を決めてないので)
            if (_sources.Count() <= 1) return false;

            var imageSource = _sources.FirstOrDefault(x => x.FilePath == sourcePath);
            if (imageSource is null) return false;
            _sources.Remove(imageSource);
            return true;
        }

        // リストからファイルを削除する(実ファイルがゴミ箱送りになる)
        public bool DeleteImageFile(string sourcePath)
        {
            if (!ClearImageFile(sourcePath)) return false;

            // ゴミ箱に移動されない
            //File.Delete(sourcePath);

            // ゴミ箱に移動される(参照追加 Microsoft.VisualBasic が必要)
            // https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                sourcePath,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

            return true;
        }

#region テスト

        private string? _LoadStatus;
        public string? LoadStatus
        {
            get => _LoadStatus;
            private set => SetProperty(ref _LoadStatus, value);
        }

        private void LoadedItemText()
        {
            var sb = new System.Text.StringBuilder();
            foreach(var source in _sources)
            {
                sb.Append(source.IsThumbnailEmpty ? "□" : "■");
            }
            LoadStatus = sb.ToString();
        }

#endregion

    }
#endif
}
