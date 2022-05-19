using Foundation;

using ObjCRuntime;

using WebKit;

namespace HB.FullStack.Client.UI.Maui.Controls
{
    //TODO: 找到调用HybridWebView.OnPageFinished方法的地方
    //在MauiWebViewNavigationDelegate中，但没有提供override方法。所以要不大改，但发现Navigated事件总会被调用，不跟Android一样，所以可以在Navigated事件中注入。
    public partial class HybridWebViewHandler : IWKScriptMessageHandler,IWKNavigationDelegate
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
