using System;

namespace ImageComparisonViewer.Common.Extensions
{
#if false
    public static class ArrayShiftExtension
    {
        /// <summary>
        /// コレクション要素を右周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        private static T[] RightShiftOnce<T>(this T[] array)
        {
            var tail = array[^1];
            for (int i = array.Length - 1; i > 0; i--)
            {
                array[i] = array[i - 1];
            }
            array[0] = tail;

            return array;
        }

        /// <summary>
        /// コレクション要素を左周りシフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        private static T[] LeftShiftOnce<T>(this T[] array)
        {
            var head = array[0];
            for (int i = 0; i < array.Length - 1; i++)
            {
                array[i] = array[i + 1];
            }
            array[^1] = head;

            return array;
        }

        /// <summary>
        /// コレクション要素を右周りに指定回数シフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static T[] RightShift<T>(this T[] array, int rightShift)
        {
            if (rightShift == 0)
            {
                return array;
            }
            else if (rightShift < 0)
            {
                return LeftShift(array, -rightShift);
            }
            else
            {
                for (int i = 0; i < rightShift; i++)
                    array = RightShiftOnce(array);

                return array;
            }
        }

        /// <summary>
        /// コレクション要素を左周りに指定回数シフト
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static T[] LeftShift<T>(this T[] array, int leftShift)
        {
            if (leftShift == 0)
            {
                return array;
            }
            else if (leftShift < 0)
            {
                return RightShift(array, -leftShift);
            }
            else
            {
                for (int i = 0; i < leftShift; i++)
                    array = LeftShiftOnce(array);

                return array;
            }
        }

    }
#endif
}
