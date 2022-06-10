using Android.Content;

using HB.FullStack.XamarinForms.Controls;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HB.FullStack.XamarinForms.Controls.HybridWebView), typeof(HB.FullStack.XamarinForms.Droid.Renders.HybridWebViewRenderer))]

namespace HB.FullStack.XamarinForms.Droid.Renders
{
    public class HybridWebViewRenderer : WebViewRenderer
    {
        private const string JAVASCRIPT_FUNCTION = @"
            function invokeCSharpAction(data){
                jsBridge.invokeAction(data);
            }";

        public HybridWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                //Control.RemoveJavascriptInterface("jsBridge");
                ((HybridWebView)Element).Cleanup();
            }
            if (e.NewElement != null)
            {
                //TODO: 调整Cache
                //Control.Settings.CacheMode = Android.Webkit.CacheModes.NoCache;

                Control.Settings.AllowContentAccess = true;
                Control.Settings.AllowFileAccess = true;
                Control.Settings.AllowFileAccessFromFileURLs = true;
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                Control.Settings.JavaScriptEnabled = true;
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");

                Control.SetWebViewClient(new JavascriptWebViewClient(this, $"javascript: {JAVASCRIPT_FUNCTION}"));
                //Control.LoadUrl($"file:///android_asset/Content/{((HybridWebView)Element).Uri}");

                if (Element is HybridWebView hybridWebView)
                {
                    if (!string.IsNullOrEmpty(hybridWebView.Uri))
                    {
                        Control.LoadUrl(hybridWebView.Uri);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((HybridWebView)Element).Cleanup();
            }
            base.Dispose(disposing);
        }
    }
}