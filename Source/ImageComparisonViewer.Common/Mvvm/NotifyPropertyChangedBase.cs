using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImageComparisonViewer.Common.Mvvm
{
    // https://blog.okazuki.jp/entry/2015/05/09/124333
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
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
