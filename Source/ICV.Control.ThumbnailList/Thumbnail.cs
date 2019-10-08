using Prism.Mvvm;
using System.IO;
using System.Windows.Media.Imaging;

namespace ICV.Control.ThumbnailList
{
    /// <summary>
    /// View用のサムネイル情報
    /// </summary>
    class Thumbnail : BindableBase
    {
        /// <summary>
        /// 画像ファイルPATH
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 画像ファイル名
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// サムネイル画像(非読込み時はnull)
        /// </summary>
        public BitmapSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private BitmapSource? _image;

        public Thumbnail(string path)
        {
            FilePath = path;
            Filename = Path.GetFileName(path);
        }
    }
}
