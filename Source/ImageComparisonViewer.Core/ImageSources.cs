using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ImageComparisonViewer.Core
{
    public class ImageSources
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        private readonly IList<string> DirectriesPath =
            Enumerable.Repeat<string>(default!, DirectriesCountMax).ToList();

        //private readonly Guid guid = Guid.NewGuid();

        public ImageSources()
        {

        }

        public void SetDirectryPath(int index, string path)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            DirectriesPath[index] = path;
        }

        public string GetDirectryPath(int index)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            return DirectriesPath[index];
        }

        public void AdaptImageListTracks(int contentCount, int loopCount)
        {
            if (contentCount <= 1) return;
            if (loopCount == 0) return;

            var list = DirectriesPath;
            if (contentCount > list.Count)
                throw new ArgumentOutOfRangeException(nameof(contentCount));

            if (loopCount > 0)
            {
                for (int i = 0; i < loopCount; i++)
                {
                    var tail = list[contentCount - 1];
                    for (int j = contentCount - 1; j > 0; j--)
                    {
                        list[j] = list[j - 1];
                    }
                    list[0] = tail;
                }
            }
            else if (loopCount < 0)
            {
                for (int i = 0; i < -loopCount; i++)
                {
                    var head = list[0];
                    for (int j = 0; j < contentCount - 1; j++)
                    {
                        list[j] = list[j + 1];
                    }
                    list[contentCount - 1] = head;
                }
            }
        }

    }
}
