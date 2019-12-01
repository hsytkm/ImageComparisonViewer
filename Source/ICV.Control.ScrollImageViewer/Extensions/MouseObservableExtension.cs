using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class MouseObservableExtension
    {
        // 指定コントロール上のマウスポインタの絶対座標を取得
        //public static IObservable<Point> MouseMovePointAsObservable(this UIElement control, bool handled = false) =>
        //    control.MouseMoveAsObservable(handled).Select(e => e.GetPosition((IInputElement)control));

        /// <summary>
        /// マウスドラッグ中の絶対座標を流す
        /// </summary>
        /// <param name="control">対象コントロール</param>
        /// <param name="originControl">マウス移動量の原点コントロール</param>
        /// <returns>絶対座標</returns>
        //public static IObservable<Point> MouseLeftDragPointAsObservable(this UIElement control, bool handled = false, IInputElement? originControl = null)
        //{
        //    if (originControl is null) originControl = control;

        //    var mouseDown = control.MouseLeftButtonDownAsObservable(handled).ToUnit();
        //    var mouseUp = control.MouseLeftButtonUpAsObservable(handled).ToUnit();

        //    return control.MouseMoveAsObservable(handled)
        //        .Select(e => e.GetPosition(originControl))
        //        .SkipUntil(mouseDown)
        //        .TakeUntil(mouseUp)
        //        .Repeat();
        //}

        /// <summary>
        /// マウスドラッグ中の移動差分量を流す
        /// </summary>
        /// <param name="control">対象コントロール</param>
        /// <param name="originControl">マウス移動量の原点コントロール</param>
        /// <returns>移動差分量</returns>
        public static IObservable<Vector> MouseLeftDragVectorAsObservable(this UIElement control, bool handled = false, IInputElement? originControl = null)
        {
            if (originControl is null) originControl = control;

            // Down～Upまでマウス捕捉
            var mouseDown = control.MouseLeftButtonDownEventAsObservable(handled)
                .Do(_ => control.CaptureMouse())
                .ToUnit();

            var mouseUp = control.MouseLeftButtonUpEventAsObservable(handled)
                .Do(_ => control.ReleaseMouseCapture())
                .ToUnit();

            return control.MouseMoveEventAsObsAsObservable(handled)
                .Select(e => e.GetPosition(originControl))
                .Pairwise()     // クリック前からPairwiseしておく
                .Select(x => x.NewItem - x.OldItem)
                .SkipUntil(mouseDown)
                .TakeUntil(mouseUp)
                .Repeat();
        }

        /// <summary>
        /// マウスドラッグ中の絶対座標と移動差分量を流す
        /// </summary>
        /// <param name="control">対象コントロール</param>
        /// <param name="originControl">マウス移動量の原点コントロール</param>
        /// <returns>絶対座標と移動差分量</returns>
        //public static IObservable<(Point point, Vector vector)> MouseLeftDragPointVectorAsObservable(this UIElement control, bool handled = false, IInputElement? originControl = null)
        //{
        //    if (originControl is null) originControl = control;

        //    // Down～Upまでマウス捕捉
        //    var mouseDown = control.MouseLeftButtonDownAsObservable(handled)
        //        .Do(_ => control.CaptureMouse()).ToUnit();
        //    var mouseUp = control.MouseLeftButtonUpAsObservable(handled)
        //        .Do(_ => control.ReleaseMouseCapture()).ToUnit();

        //    return control.MouseMoveAsObservable(handled)
        //        .Select(e => e.GetPosition(originControl))
        //        .Pairwise()     // クリック前からPairwiseしておく
        //        .Select(x => (point: x.NewItem, vector: x.NewItem - x.OldItem))
        //        .SkipUntil(mouseDown)
        //        .TakeUntil(mouseUp)
        //        .Repeat();
        //}

    }
}
