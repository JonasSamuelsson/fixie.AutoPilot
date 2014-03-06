using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fixie.AutoRun
{
    public class Observable<T> : INotifyPropertyChanged
    {
        private T _value;

        public Observable(T value = default(T))
        {
            _value = value;
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;
                _value = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public static implicit operator T(Observable<T> observable)
        {
            return observable.Value;
        }
    }
}