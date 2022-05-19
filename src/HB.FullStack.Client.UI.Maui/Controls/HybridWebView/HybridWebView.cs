using Microsoft.Maui;
using Microsoft.Maui.Controls;

using System;
using System.Threading.Tasks;
using System.Windows.Input;


namespace HB.FullStack.Client.UI.Maui.Controls
{
    /// <summary>
    /// 可以在js中，使用CallCSharp(data)。调用OnJavascriptCallCommand
    /// </summary>
    public class HybridWebView : WebView
    {
        public static readonly BindableProperty OnJavascriptCallCommandProperty = BindableProperty.Create(nameof(OnJavascriptCallCommand), typeof(ICommand), typeof(HybridWebView), null);

        private readonly WeakEventManager _eventManager = new WeakEventManager();

        public event EventHandler PageFinished
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public ICommand? OnJavascriptCallCommand
        {
            get { return (ICommand?)GetValue(OnJavascriptCallCommandProperty); }
            set { SetValue(OnJavascriptCallCommandProperty, value); }
        }

        internal void OnPageFinished()
        {
            _eventManager.HandleEvent(this, new EventArgs(), nameof(PageFinished));
        }

        internal void OnJavascriptCall(string data)
        {
            if(OnJavascriptCallCommand == null)
            {
                return;
            }

            OnJavascriptCallCommand?.Execute(data);
        }
    }
}