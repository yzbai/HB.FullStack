using HB.FullStack.Client.Maui.Controls;

using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using System;

namespace HB.FullStack.Client.Maui.TCaptcha
{
    public class TCaptchaPopup : Popup
    {
        public static string AppId { get; internal set; } = null!;

        //TODO: 添加容灾处理。https://cloud.tencent.com/document/product/1110/72310
        private const string HTML = @"
                <html>
                <head>
                    <script src=""https://ssl.captcha.qq.com/TCaptcha.js""></script>
                </head>
                <body>
                    
                </body>
                <script>
                    function showCaptcha(appid) {
                        var captcha = new TencentCaptcha(appid, function (res) {
                            CallCSharp(res);
                        });
                        captcha.show();
                    }
                </script>
                </html>";

        private readonly HybridWebView _webView;

        public TCaptchaPopup()
        {
            CanBeDismissedByTappingOutsideOfPopup = false;

            Content = new HybridWebView { Source = new HtmlWebViewSource { Html = HTML } }.Fill().Assign(out _webView);

            _webView.Source = new HtmlWebViewSource { Html = HTML };
            _webView.PageFinished += async (sender, e) => await _webView.EvaluateJavaScriptAsync($"showCaptcha(\"{AppId}\")");
            _webView.OnJavascriptCallCommand = new Command(json =>
            {
                string? rt = json?.ToString()?.Trim();

                if (rt.IsJsonString())
                {
                    Close(rt);
                }
                else
                {
                    //没有返回
                    Close("");
                }
            });

            Size = Currents.PopupSizeConstants.Medium;
        }
    }
}