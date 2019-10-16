// [WPF] BehaviorのOnDetaching http://gacken.com/blog/program/wpf-115_20161211/
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace ImageComparisonViewer.Common.Mvvm
{
    public class BehaviorBase<T> : Behavior<T> where T : FrameworkElement
    {
        /// <summary>
        /// セットアップ状態
        /// </summary>
        private bool isSetup = false;

        /// <summary>
        /// Hook状態
        /// </summary>
        private bool isHookedUp = false;

        /// <summary>
        /// 対象オブジェクト
        /// </summary>
        private WeakReference? weakTarget;

        /// <summary>
        /// Changedハンドラ
        /// </summary>
        protected override void OnChanged()
        {
            base.OnChanged();

            var target = AssociatedObject;
            if (target != null)
            {
                HookupBehavior(target);
            }
            else
            {
                UnHookupBehavior();
            }
        }

        /// <summary>
        /// ビヘイビアをHookする
        /// </summary>
        /// <param name="target"></param>
        private void HookupBehavior(T target)
        {
            if (isHookedUp) return;

            isHookedUp = true;
            weakTarget = new WeakReference(target);
            target.Unloaded += OnTargetUnloaded;
            target.Loaded += OnTargetLoaded;
        }

        /// <summary>
        /// ビヘイビアをUnhookする
        /// </summary>
        private void UnHookupBehavior()
        {
            if (!isHookedUp) return;

            isHookedUp = false;
            var target = AssociatedObject ?? (weakTarget?.Target as T);
            if (target != null)
            {
                target.Unloaded -= OnTargetUnloaded;
                target.Loaded -= OnTargetLoaded;
            }
            weakTarget = null;
        }

        /// <summary>
        /// [関連オブジェクト] Loadedハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            if (isSetup) return;

            isSetup = true;
            OnLoaded();
        }

        /// <summary>
        /// [関連オブジェクト] Unloadedハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            if (!isSetup) return;

            isSetup = false;
            OnUnloaded();
        }

        /// <summary>
        /// Behaviorの設定
        /// </summary>
        protected virtual void OnLoaded() { }

        /// <summary>
        /// Behaviorの解除
        /// </summary>
        protected virtual void OnUnloaded() { }

        protected sealed override void OnAttached() => base.OnAttached();
        protected sealed override void OnDetaching() => base.OnDetaching();

    }
}