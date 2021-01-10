using AsyncAwaitBestPractices;
using HB.FullStack.Client.Base;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HB.FullStack.Client.Controls
{
    public class HybridWebView : WebView, IBaseContentView
    {
        private readonly WeakEventManager _eventManager = new WeakEventManager();

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            propertyName: nameof(Uri),
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

#pragma warning disable CA1056 // URI-like properties should not be strings
        public string Uri
#pragma warning restore CA1056 // URI-like properties should not be strings
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public event EventHandler Loaded { add => _eventManager.AddEventHandler(value); remove => _eventManager.RemoveEventHandler(value); }

        public void OnLoaded()
        {
            _eventManager.RaiseEvent(this, new EventArgs(), nameof(Loaded));
        }

        Action<string>? _action;

        public void RegisterAction(Action<string> callback)
        {
            _action = callback;
        }

        public void Cleanup()
        {
            _action = null;
        }

        public void InvokeAction(string data)
        {
            if (_action == null || data == null)
            {
                return;
            }
            _action.Invoke(data);
        }

        public void OnAppearing()
        {
            IsAppearing = true;
        }

        public void OnDisappearing()
        {
            IsAppearing = false;
        }

        public IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }

        public bool IsAppearing { get; private set; }
    }
}
