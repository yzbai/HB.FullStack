using System;
using Android.Webkit;
using HB.FullStack.XamarinForms.Controls;
using Xamarin.Forms.Platform.Android;

namespace HB.FullStack.XamarinForms.Droid.Renders
{
    public class JavascriptWebViewClient : FormsWebViewClient
    {
        readonly string _javascript;

        readonly WeakReference<HybridWebViewRenderer> _hybridWebViewRenderer;

        public JavascriptWebViewClient(HybridWebViewRenderer renderer, string javascript) : base(renderer)
        {
            _hybridWebViewRenderer = new WeakReference<HybridWebViewRenderer>(renderer);
            _javascript = javascript;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);

            view.EvaluateJavascript(_javascript, null);

            if (_hybridWebViewRenderer != null && _hybridWebViewRenderer.TryGetTarget(out HybridWebViewRenderer hybridRenderer))
            {
                if (hybridRenderer.Element is HybridWebView hybridWebView)
                {
                    hybridWebView.OnLoaded();
                }
            }
        }
    }
}