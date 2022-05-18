using Microsoft.UI.Xaml.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui.Controls
{
    public partial class HybridWebViewHandler
    {
        protected override WebView2 CreatePlatformView()
        {
            WebView2 webview =  base.CreatePlatformView();

            webview.NavigationCompleted += Webview_NavigationCompleted;
            webview.WebMessageReceived += Webview_WebMessageReceived;

            return webview;
        }

        private async void Webview_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            await sender.ExecuteScriptAsync("function CallCSharp(data){chrome.webview.postMessage(data);}");
        }

        private void Webview_WebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            if (VirtualView is HybridWebView hybrid)
            {
                hybrid.OnJavascriptCall(args.WebMessageAsJson);
            }
        }

    }
}
