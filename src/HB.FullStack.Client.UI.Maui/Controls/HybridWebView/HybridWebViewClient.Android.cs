using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui.Controls
{
    public class HybridWebViewClient : MauiWebViewClient
    {
        private HybridWebViewHandler _handler;

        public HybridWebViewClient(HybridWebViewHandler handler) : base(handler)
        {
            _handler = handler;
        }

        public override void OnPageFinished(Android.Webkit.WebView? view, string? url)
        {
            base.OnPageFinished(view, url);

            if(_handler.VirtualView is HybridWebView hybrid)
            {
                hybrid.OnPageFinished();
            }
        }
    }
}
