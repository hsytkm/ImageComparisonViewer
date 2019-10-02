using System;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class SpanShiftExtension
    {
        /// <summary>
        /// コレクション要素を右周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        private static Span<T> RightShiftOnce<T>(this Span<T> span)
        {
            var tail = span[^1];
            for (int i = span.Length - 1; i > 0; i--)
            {
                span[i] = span[i - 1];
            }
            span[0] = tail;

            return span;
        }

        /// <summary>
        /// コレクション要素を左周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        private static Span<T> LeftShiftOnce<T>(this Span<T> span)
        {
            var head = span[0];
            for (int i = 0; i < span.Length - 1; i++)
            {
                span[i] = span[i + 1];
            }
            span[^1] = head;

            return span;
        }

        /// <summary>
        /// コレクション要素を右周りに指定回数シフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        public static Span<T> RightShift<T>(this Span<T> span, int rightShift)
        {
            if (rightShift == 0)
            {
                return span;
            }
            else if (rightShift < 0)
            {
                return LeftShift(span, -rightShift);
            }
            else
            {
                for (int i = 0; i < rightShift; i++)
                    span = RightShiftOnce(span);

                return span;
            }
        }

        /// <summary>
        /// コレクション要素を左周りに指定回数シフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span"></param>
        public static Span<T> LeftShift<T>(this Span<T> span, int leftShift)
        {
            if (leftShift == 0)
            {
                return span;
            }
            else if (leftShift < 0)
            {
                return RightShift(span, -leftShift);
            }
            else
            {
                for (int i = 0; i < leftShift; i++)
                    span = LeftShiftOnce(span);

                return span;
            }
        }

    }
}
