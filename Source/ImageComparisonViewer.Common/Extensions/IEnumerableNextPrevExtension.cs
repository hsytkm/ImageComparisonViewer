using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class IEnumerableNextPrevExtension
    {
        // 要素数を取得
        private static int GetCount<TSource>(this IEnumerable<TSource> source) =>
            source switch
            {
                TSource[] x => x.Length,
                IList<TSource> x => x.Count,
                IReadOnlyList<TSource> x => x.Count,
                ICollection<TSource> x => x.Count,
                IReadOnlyCollection<TSource> x => x.Count,
                IEnumerable<TSource> x => x.Count(),
                _ => throw new NotSupportedException()
            };


        // 指定要素を取得
        private static TSource GetElement<TSource>(this IEnumerable<TSource> source, int index) =>
            source switch
            {
                TSource[] x => x[index],
                IList<TSource> x => x[index],
                IReadOnlyList<TSource> x => x[index],
                ICollection<TSource> x => x.ElementAt(index),
                IReadOnlyCollection<TSource> x => x.ElementAt(index),
                IEnumerable<TSource> x => x.ElementAt(index),
                _ => throw new NotSupportedException()
            };

        /// <summary>
        /// predicateがtrueになる要素の配列番号を返します。存在しない場合は負数を返します。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static bool TryGetFirstIndex<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate, out int index)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (predicate is null)
            {
                index = int.MinValue;
                return false;
            }

            int i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    index = i;
                    return true;
                }
                i++;
            }
            index = int.MinValue;
            return false;
        }

        /// <summary>
        /// predicateがtrueになる要素の次の要素を返します。存在しない場合はdefaultを返します。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static TSource? NextOrTargetOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            where TSource : class
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            // ヒットしなければdefault返す
            if (!TryGetFirstIndex(source, predicate, out var targetIndex))
                return default;

            var nextIndex = targetIndex + 1;
            var index = (nextIndex < source.GetCount()) ? nextIndex : targetIndex;
            return source.GetElement(index);
        }

        /// <summary>
        /// predicateがtrueになる要素の前の要素を返します。存在しない場合はdefaultを返します。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static TSource? PrevOrTargetOrDefault<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            where TSource : class
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            // ヒットしなければdefault返す
            if (!TryGetFirstIndex(source, predicate, out var targetIndex))
                return default;

            var prevIndex = targetIndex - 1;
            var index = (0 <= prevIndex) ? prevIndex : targetIndex;
            return source.GetElement(index);
        }
    }
}
