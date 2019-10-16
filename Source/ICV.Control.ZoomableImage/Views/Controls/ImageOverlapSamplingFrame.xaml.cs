using ImageComparisonViewer.Common.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ZoomableImage.Views.Controls
{
    /// <summary>
    /// ImageOverlapSamplingFrame.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageOverlapSamplingFrame : DisposableUserControl
    {
        private static readonly Type SelfType = typeof(ImageOverlapSamplingFrame);

        #region FrameRectRatioProperty(OneWayToSource)

        // Viewに表示されているサンプリング枠の位置割合
        private static readonly DependencyProperty FrameRectRatioProperty =
            DependencyProperty.Register(
                nameof(FrameRectRatio),
                typeof(Rect),
                SelfType);

        public Rect FrameRectRatio
        {
            get => (Rect)GetValue(FrameRectRatioProperty);
            set => SetValue(FrameRectRatioProperty, value);
        }

        #endregion

        #region GroundPanelTopLeftProperty(OneWay)

        // 下地パネルの開始位置(全体表示なら設定されて、拡大画面なら0になる)
        private static readonly DependencyProperty GroundPanelTopLeftProperty =
            DependencyProperty.Register(
                nameof(GroundPanelTopLeft),
                typeof(Point),
                SelfType);

        public Point GroundPanelTopLeft
        {
            get => (Point)GetValue(GroundPanelTopLeftProperty);
            set => SetValue(GroundPanelTopLeftProperty, value);
        }

        #endregion

        #region IsFrameInterlockProperty(OneWay)

        // スクロール/サイズを他コントロールと連動
        private static readonly DependencyProperty IsFrameInterlockProperty =
            DependencyProperty.Register(
                nameof(IsFrameInterlock),
                typeof(bool),
                SelfType);

        public bool IsFrameInterlock
        {
            get => (bool)GetValue(IsFrameInterlockProperty);
            set => SetValue(IsFrameInterlockProperty, value);
        }

        #endregion

        public ImageOverlapSamplingFrame()
        {
            InitializeComponent();
        }

    }
}
