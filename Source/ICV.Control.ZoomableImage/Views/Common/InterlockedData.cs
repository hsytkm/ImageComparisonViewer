using System.Threading;

namespace ICV.Control.ZoomableImage.Views.Common
{
    class UniqueId
    {
        private static int BaseId = 0;  // 1からID割り振られる
        public int Id { get; }

        // newする度に一意IDをインクリ
        public UniqueId() => Id = Interlocked.Increment(ref BaseId);
    }

    readonly struct InterlockedData<T>
    {
        public readonly int PublisherId;
        public readonly T Data;

        public InterlockedData(int id, T data)
        {
            PublisherId = id;
            Data = data;
        }
    }

}
