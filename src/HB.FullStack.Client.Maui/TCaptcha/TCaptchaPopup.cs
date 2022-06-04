using HB.FullStack.Client.Maui.Controls;

using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using HB.FullStack.Client.Maui.Controls.Popups;
using HB.FullStack.Client.Maui.Base;
using System;

namespace HB.FullStack.Client.Maui.TCaptcha
{
    public class TCaptchaPopup : Popup
    {
        public static string AppId { get; internal set; } = null!;

        private const string HTML = @"
                <html>
                <head>
                    <script src=""https://ssl.captcha.qq.com/TCaptcha.js""></script>
                </head>
                <body/>
                <script>
                    function showCaptcha(appid) {
                        var captcha = new TencentCaptcha(appid, function (res) {
                            CallCSharp(JSON.stringify(res));
                        });
                        captcha.show();
                    }
                </script>
                </html>";

        private readonly HybridWebView _webView;

        

        public TCaptchaPopup()
        {
            CanBeDismissedByTappingOutsideOfPopup = false;

            Content = new StackLayout
            {
                Children = {
                    new HybridWebView{ }.Fill().Assign(out _webView)
                }
            }.Fill();

            _webView.Source = new HtmlWebViewSource { Html = HTML };
            _webView.PageFinished += async (sender, e) => await _webView.EvaluateJavaScriptAsync($"showCaptcha(\"{AppId}\")").ConfigureAwait(false);
            _webView.OnJavascriptCallCommand = new Command(json => { Close(json?.ToString()); });


            Size = Current.PopupSizeConstants.Large;
        }
    }
}