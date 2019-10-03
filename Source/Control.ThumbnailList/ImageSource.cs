using Prism.Mvvm;
using System.IO;
using System.Windows.Media.Imaging;

namespace Control.ThumbnailList
{
    class ImageSource : BindableBase
    {
        private const int ThumbnailWidth = 80;

        public string FilePath { get; }

        public string Filename { get => Path.GetFileName(FilePath); }

        private BitmapSource? _Thumbnail;
        public BitmapSource? Thumbnail
        {
            get => _Thumbnail;
            private set => SetProperty(ref _Thumbnail, value);
        }

        public bool IsThumbnailEmpty { get => Thumbnail == null; }

        public ImageSource(string path)
        {
            FilePath = path;
        }

        public void LoadThumbnail()
        {
            //if (Thumbnail is null)
            //    Thumbnail = FilePath.LoadThumbnail(ThumbnailWidth);
        }

        public void UnloadThumbnail()
        {
            Thumbnail = null;
        }

    }
}
