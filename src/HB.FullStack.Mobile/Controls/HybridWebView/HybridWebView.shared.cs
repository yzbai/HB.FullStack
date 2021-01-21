using AsyncAwaitBestPractices;

using HB.FullStack.Mobile.Base;

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

using WeakEventManager = AsyncAwaitBestPractices.WeakEventManager;

namespace HB.FullStack.Mobile.Controls
{
    public class HybridWebView : WebView, IBaseContentView
    {
        private readonly WeakEventManager _eventManager = new WeakEventManager();
        private Action<string>? _cSharpAction;

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            propertyName: nameof(Uri),
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "<Pending>")]
        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public event EventHandler Loaded
        {
            add => _eventManager.AddEventHandler(value); 
            remove => _eventManager.RemoveEventHandler(value);
        }

        public HybridWebView()
        {
            Navigated += HybridWebView_Navigated;
        }

        private void HybridWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            
        }

        public void OnLoaded()
        {
            _eventManager.RaiseEvent(this, new EventArgs(), nameof(Loaded));
        }

        public void RegisterCSharpAction(Action<string> action)
        {
            _cSharpAction = action;
        }

        public void Cleanup()
        {
            _cSharpAction = null;
        }

        public void InvokeAction(string data)
        {
            if (_cSharpAction == null || data == null)
            {
                return;
            }
            _cSharpAction.Invoke(data);
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
