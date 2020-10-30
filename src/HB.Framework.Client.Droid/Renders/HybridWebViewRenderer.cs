using Android.Content;
using HB.Framework.Client.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HB.Framework.Client.Controls.HybridWebView), typeof(HB.Framework.Client.Droid.Renders.HybridWebViewRenderer))]
namespace HB.Framework.Client.Droid.Renders
{
    public class HybridWebViewRenderer : WebViewRenderer
    {
        const string _javascriptFunction = @"
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
                Control.RemoveJavascriptInterface("jsBridge");
                ((HybridWebView)Element).Cleanup();
            }
            if (e.NewElement != null)
            {
                //TODO: 调整Cache
                Control.Settings.CacheMode = Android.Webkit.CacheModes.NoCache;

                Control.SetWebViewClient(new JavascriptWebViewClient(this, $"javascript: {_javascriptFunction}"));
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");
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