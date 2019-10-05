using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Control.ThumbnailList
{
    // https://blog.okazuki.jp/entry/2015/05/09/124333
    class MyBindableBase : INotifyPropertyChanged
    {
#pragma warning disable CS8618
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore  CS8618

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = default!)
        {
            if (Equals(field, value)) return false;
            field = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
