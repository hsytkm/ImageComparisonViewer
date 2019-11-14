using System;
using System.Collections.Generic;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// インデックス付きのIEnumerableを返却する
        /// 小ネタ インデックス付き foreach  https://ufcpp.net/blog/2016/12/tipsindexedforeach/
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="source">Sequence</param>
        /// <returns>SequenceWithIndex</returns>
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            IEnumerable<(T item, int index)> impl()
            {
                var i = 0;
                foreach (var item in source)
                {
                    yield return (item, i);
                    ++i;
                }
            }
            return impl();
        }

        /// <summary>
        /// ForEach for IEnumerable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="sequence">Sequence</param>
        /// <param name="action">Action</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence is null) throw new ArgumentNullException(nameof(sequence));
            if (action is null) throw new ArgumentNullException(nameof(action));

            foreach (T item in sequence)
                action(item);
        }

    }
}
