using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Controls
{
    public class HybridWebViewClient : MauiWebViewClient
    {
        private HybridWebViewHandler _handler;

        public HybridWebViewClient(HybridWebViewHandler handler) : base(handler)
        {
            _handler = handler;
        }

        public override async void OnPageFinished(Android.Webkit.WebView? view, string? url)
        {
            base.OnPageFinished(view, url);

            if (_handler.VirtualView is HybridWebView hybrid)
            {
                //为了使各个平台调用接口统一
                _ = await hybrid.EvaluateJavaScriptAsync(@"function CallCSharp(data){jsBridge.CallCSharp(data);}");

                hybrid.OnPageFinished();
            }
        }
    }
}
