using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace ImageComparisonViewer.Common.Mvvm
{
    public static class ViewHelper
    {
        /// <summary>
        /// 指定コントロールの以下の指定コントロールを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool TryGetChildControl<T>(this DependencyObject d, [NotNullWhen(true)] out T? child)
            where T : DependencyObject
        {
            if (d is T control1)
            {
                child = control1;
                return true;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                if (TryGetChildControl<T>(VisualTreeHelper.GetChild(d, i), out var control2))
                {
                    child = control2;
                    return true;
                }
            }

            child = default;
            return false;
        }

        /// <summary>
        /// 指定コントロールの子孫要素を取得  https://blog.xin9le.net/entry/2013/10/29/222336
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject d)
        {
            if (d is null) throw new ArgumentNullException(nameof(d));

            foreach (var child in Children(d))
            {
                yield return child;
                foreach (var grandChild in child.Descendants())
                    yield return grandChild;
            }

            static IEnumerable<DependencyObject> Children(DependencyObject dependencyObject)
            {
                var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
                if (count == 0)
                    yield break;

                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null)
                        yield return child;
                }
            }
        }

    }
}
