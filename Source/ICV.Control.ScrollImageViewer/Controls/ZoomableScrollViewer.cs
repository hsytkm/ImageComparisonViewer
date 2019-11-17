using ImageComparisonViewer.Common.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors;
using ICV.Control.ScrollImageViewer.Behaviors;
using System.Collections.Generic;

namespace ICV.Control.ScrollImageViewer.Controls
{
    class ZoomableScrollViewer : ScrollViewer, IDisposable
    {
        private static readonly Type SelfType = typeof(ZoomableScrollViewer);

        // Controls
        private readonly Grid _contentGrid;
        private readonly Image _mainImage;

        #region ImageSourceProperty(OneWay/Inherits)

        /// <summary>ソース画像は親から継承する</summary>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        private static readonly DependencyProperty ImageSourceProperty =
            BasePanel.ImageSourceProperty.AddOwner(SelfType,
                new FrameworkPropertyMetadata(default,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits,
                    (d, e) => ((ZoomableScrollViewer)d).OnImageSourceChanged((BitmapSource)(e.NewValue))));

        private void OnImageSourceChanged(BitmapSource bitmapSource)
        {
            _mainImage.Source = bitmapSource;
        }

        /// <summary>元画像の読込み中フラグ</summary>
        public bool IsLoadingImage
        {
            get => (bool)GetValue(IsLoadingImageProperty);
            set => SetValue(IsLoadingImageProperty, value);
        }
        private static readonly DependencyProperty IsLoadingImageProperty =
            BasePanel.IsLoadingImageProperty.AddOwner(SelfType,
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        public ZoomableScrollViewer()
        {
            /*
             * [] ControlのリストをGridに詰め込んで、Contentにしたい
             * [] ViewModelから倍率と表示位置を要求する
             * [] ViewModelに倍率と表示位置を通知する
             * [] ダブルクリックでズーム倍率を変更する
             * [] シングルクリックでズーム倍率を一時的に変更する
             * [] マウスホイールでズーム倍率を変更する
             * [] ViewModelにカーソル位置を通知する
             * [] ViewModelからサンプリング枠の表示を切り替えたい
             * [] ViewModelにサンプリング枠の位置を通知したい
             * [] 
             * [] 
             * [] 
             */

            var selfBehaviors = Interaction.GetBehaviors(this);
            selfBehaviors.Add(new MouseHorizontalShiftBehavior());

            _mainImage = CreateMainImageControl();
            _contentGrid = CreateContentGridControl(_mainImage);
            this.Content = _contentGrid;
        }

        /// <summary>主画像</summary>
        private static Image CreateMainImageControl()
        {
            var image = new Image();
            var behaviors = Interaction.GetBehaviors(image);
            behaviors.Add(new ImageBitmapScalingBehavior());
            behaviors.Add(new ImageMouseCursorBehavior());
            return image;
        }

        /// <summary>コンテントに設定されるGridで内部にImageを持つ</summary>
        private static Grid CreateContentGridControl(Image image)
        {
            var grid = new Grid();
            grid.Children.Add(image);
            return grid;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) { /*TODO: マネージ状態を破棄します (マネージ オブジェクト)*/ }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                Interaction.GetBehaviors(this).Detach();
                foreach (var child in _contentGrid.Children)
                {
                    if (child is DependencyObject depObj)
                        Interaction.GetBehaviors(depObj).Detach();
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

    }
}
