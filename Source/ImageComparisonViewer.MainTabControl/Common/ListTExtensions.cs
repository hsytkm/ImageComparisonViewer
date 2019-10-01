using System;
using System.Collections.Generic;

namespace ImageComparisonViewer.MainTabControl.Common
{
    static class ListTExtensions
    {
        /// <summary>
        /// リスト要素を右周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        private static IList<T> RightShiftOnce<T>(this IList<T> list)
        {
            var tail = list[^1];
            for (int i = list.Count - 1; i > 0; i--)
            {
                list[i] = list[i - 1];
            }
            list[0] = tail;

            return list;
        }

        /// <summary>
        /// リスト要素を左周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        private static IList<T> LeftShiftOnce<T>(this IList<T> list)
        {
            var head = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                list[i] = list[i + 1];
            }
            list[^1] = head;

            return list;
        }

        /// <summary>
        /// リスト要素を右周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static IList<T> RightShift<T>(this IList<T> list, int rightShift)
        {
            if (rightShift < 0)
                return LeftShift(list, -rightShift);

            for (int i = 0; i < rightShift; i++)
            {
                list = RightShiftOnce(list);
            }
            return list;
        }

        /// <summary>
        /// リスト要素を左周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static IList<T> LeftShift<T>(this IList<T> list, int leftShift)
        {
            if (leftShift < 0)
                return RightShift(list, -leftShift);

            for (int i = 0; i < leftShift; i++)
            {
                list = LeftShiftOnce(list);
            }
            return list;
        }

    }
}
