using Microsoft.Maui;
using Microsoft.Maui.Controls;

using System;
using System.Threading.Tasks;
using System.Windows.Input;


namespace HB.FullStack.Client.UI.Maui.Controls
{
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

        public HybridWebView()
        {
            PageFinished += HybridWebView_InitEveryTime;
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

        private async void HybridWebView_InitEveryTime(object? sender, EventArgs e)
        {
#if ANDROID
            //必须放在第一个
            //为了统一调用接口，不必在iOS下是CallCSharp,而在Android下是jsBridge.CallCSharp
            //另外一个办法就是在Handler.Mapper中，覆盖Mapper[nameof(WebViewClient)]，指向一个新的WebViewClient，然后在WebViewClient中添加这段js
            //后续：Navigated事件在直接html字符串作为source时，不触发事件。所以只能覆盖WebViewClient了。
            _ = await EvaluateJavaScriptAsync(@"function CallCSharp(data){jsBridge.CallCSharp(data);}").ConfigureAwait(true);
#else
            await Task.CompletedTask.ConfigureAwait(true);
#endif
        }
    }
}