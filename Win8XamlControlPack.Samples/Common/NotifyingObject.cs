using System;
using System.ComponentModel;

namespace Win8XamlControlPack.Samples.Common
{
    public class NotifyingObject
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

        protected virtual void OnPropertyChanged(String propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
