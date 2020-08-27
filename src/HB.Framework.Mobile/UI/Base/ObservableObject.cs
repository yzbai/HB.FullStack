using AsyncAwaitBestPractices;
using HB.Framework.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System
{
    public class ObservableObject : ValidatableObject, INotifyPropertyChanged
    {
        private readonly WeakEventManager _propertyChangedEventManager = new WeakEventManager();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            _propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(PropertyChanged));
        }

        protected void SetProperty<T>(ref T backingStore, T value, Action? onChanged = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyName);
        }

    }
}
