using Prism.Mvvm;
using System;

namespace ICV.Control.ZoomableImage.Models
{
    class ViewSettings : BindableBase
    {

        // 各画像の表示エリアを連動させるかフラグ(false=連動しない)
        public bool IsImageViewerInterlock
        {
            get => _isImageViewerInterlock;
            set => SetProperty(ref _isImageViewerInterlock, value);
        }
        private bool _isImageViewerInterlock;

        // 縮小画像の表示可能フラグ(false=表示禁止)
        public bool CanVisibleReducedImage
        {
            get => _canVisibleReducedImage;
            set => SetProperty(ref _canVisibleReducedImage, value);
        }
        private bool _canVisibleReducedImage;

        // 画像上のサンプリング枠の表示フラグ(false=表示しない)
        public bool IsVisibleImageOverlapSamplingFrame
        {
            get => _isVisibleImageOverlapSamplingFrame;
            set => SetProperty(ref _isVisibleImageOverlapSamplingFrame, value);
        }
        private bool _isVisibleImageOverlapSamplingFrame;

        // サンプリング枠のレイヤー(true=画像上, false=スクロール上)
        public bool IsVisibleSamplingFrameOnImage
        {
            get => _isVisibleSamplingFrameOnImage;
            set => SetProperty(ref _isVisibleSamplingFrameOnImage, value);
        }
        private bool _isVisibleSamplingFrameOnImage;

    }
}
