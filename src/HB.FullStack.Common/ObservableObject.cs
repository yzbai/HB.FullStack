using AsyncAwaitBestPractices;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common
{
	/// <summary>
	/// Observable object with INotifyPropertyChanged implemented using WeakEventManager
	/// </summary>
	public abstract class ObservableObject : INotifyPropertyChanging, INotifyPropertyChanged
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		public event PropertyChangingEventHandler? PropertyChanging 
		{
			add=>_weakEventManager.AddEventHandler(value); 
			remove =>_weakEventManager.RemoveEventHandler(value);
		}

		public event PropertyChangedEventHandler? PropertyChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = "")
		{
			_weakEventManager.RaiseEvent(this, new PropertyChangingEventArgs(propertyName), nameof(PropertyChanging));
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = "")
		{
			_weakEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(PropertyChanged));
		}

		protected virtual bool SetProperty<T>(
			ref T backingStore,
			T value,
			[CallerMemberName] string? propertyName = "",
			//Action? onChanging = null,
			//Action? onChanged = null,
			Func<T, T, bool>? validateValue = null)
		{
			// if value didn't change
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			// if value changed but didn't validate
			if (validateValue != null && !validateValue(backingStore, value))
				return false;

			//onChanging?.Invoke();
			OnPropertyChanging(propertyName);

			backingStore = value;

			//onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			
			return true;
		}
	}
}
