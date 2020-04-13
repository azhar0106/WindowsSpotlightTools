using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WallpaperFetcherGui
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(object obj, string propertyName)
        {
            PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            NotifyPropertyChanged(this, propertyName);
        }
    }
}
