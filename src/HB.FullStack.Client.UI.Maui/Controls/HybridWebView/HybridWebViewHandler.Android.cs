using Android.Webkit;

using Java.Interop;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AWebView = Android.Webkit.WebView;

namespace HB.FullStack.Client.UI.Maui.Controls
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "在DisconnectHandler方法中Dispose")]
    public partial class HybridWebViewHandler
    {
        private HybridWebViewClient? _webViewClient;
        private JSBridge? _jSBridge;

        public HybridWebViewHandler()
        {
            Mapper[nameof(WebViewClient)] = MapHybridWebViewClient;
        }

        public static void MapHybridWebViewClient(IWebViewHandler handler, IWebView? webView)
        {
            if (handler is HybridWebViewHandler platformHandler)
                handler.PlatformView.SetWebViewClient(platformHandler._webViewClient ??= new HybridWebViewClient(platformHandler));
        }

        protected override AWebView CreatePlatformView()
        {
            AWebView webview = base.CreatePlatformView();

            webview.AddJavascriptInterface(_jSBridge ??= new JSBridge(this), "jsBridge");

            return webview;
        }

        protected override void DisconnectHandler(AWebView platformView)
        {
            _webViewClient?.Dispose();
            _jSBridge?.Dispose();

            platformView.RemoveJavascriptInterface("jsBridge");

            base.DisconnectHandler(platformView);
        }
    }

    public class JSBridge : Java.Lang.Object
    {
        private readonly HybridWebViewHandler _handler;

        public JSBridge(HybridWebViewHandler handler)
        {
            _handler = handler;
        }

        [JavascriptInterface]
        [Export("CallCSharp")]
        public void CallCSharp(string data)
        {
            if (_handler.VirtualView is HybridWebView hybrid)
            {
                hybrid.OnJavascriptCall(data);
            }
        }
    }
}
