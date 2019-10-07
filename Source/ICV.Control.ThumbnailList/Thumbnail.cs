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
        public BitmapSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private BitmapSource? _image;

        public string FilePath { get; }
        public string Filename { get; }

        public Thumbnail(string path)
        {
            FilePath = path;
            Filename = Path.GetFileName(path);
        }
    }
}
