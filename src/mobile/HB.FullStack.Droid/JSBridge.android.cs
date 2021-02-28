using System;
using Android.Webkit;
using HB.FullStack.XamarinForms.Controls;
using Java.Interop;

namespace HB.FullStack.Droid.Renders
{
    public class JSBridge : Java.Lang.Object
    {
        readonly WeakReference<HybridWebViewRenderer> _hybridWebViewRenderer;

        public JSBridge(HybridWebViewRenderer hybridRenderer)
        {
            _hybridWebViewRenderer = new WeakReference<HybridWebViewRenderer>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            if (_hybridWebViewRenderer != null && _hybridWebViewRenderer.TryGetTarget(out HybridWebViewRenderer hybridRenderer))
            {
                ((HybridWebView)hybridRenderer.Element).InvokeAction(data);
            }
        }
    }
}