using Foundation;

using ObjCRuntime;

using WebKit;

namespace HB.FullStack.Client.UI.Maui.Controls
{
    public partial class HybridWebViewHandler : IWKScriptMessageHandler
    {
        private const string JavaScriptFunction = "function CallCSharp(data){window.webkit.messageHandlers.jsBridge.postMessage(data);}";
        protected override WKWebView CreatePlatformView()
        {
            WKWebView webview = base.CreatePlatformView();

            WKUserContentController controller = webview.Configuration.UserContentController;
            using NSString source = new NSString(JavaScriptFunction);
            using WKUserScript script = new WKUserScript(source, WKUserScriptInjectionTime.AtDocumentEnd, false);

            controller.AddUserScript(script);
            controller.AddScriptMessageHandler(this, "jsBridge");

            return webview;
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (VirtualView is HybridWebView hybrid)
            {
                hybrid.OnJavascriptCall(message.Body.ToString());
            }
        }

        protected override void DisconnectHandler(WKWebView platformView)
        {
            platformView.Configuration.UserContentController.RemoveAllScriptMessageHandlers();
            platformView.Configuration.UserContentController.RemoveAllUserScripts();

            base.DisconnectHandler(platformView);
        }

        public NativeHandle Handle { get; set; }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HybridWebViewHandler()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
