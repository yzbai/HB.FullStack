﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Mobile.Base;

using Microsoft.Extensions.Options;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.TCaptcha
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TCaptchaDialog : BaseContentPage
    {
        private const string _html = @"
                <html>
                <head>
                    <script src=""https://ssl.captcha.qq.com/TCaptcha.js""></script>
                </head>
                <body/>
                <script>
                    function showCaptcha(appid) {
                        var captcha = new TencentCaptcha(appid, function (res) {
                            invokeCSharpAction(JSON.stringify(res));
                        });
                        captcha.show();
                    }
                </script>
                </html>";

        public static string AppId { get; internal set; } = null!;

        public string? Result { get; set; }

        //持有的话，就要记得放弃. 不过由于这个Dialog一般生命周期很短，所以也无大碍
        public Func<string?, Task>? PoppedDelegate { get; set; }

        public TCaptchaDialog(Func<string?, Task>? poppedDelegate)
        {
            PoppedDelegate = poppedDelegate;

            InitializeComponent();

            webView.Source = new HtmlWebViewSource { Html = _html };
            webView.Loaded += WebView_Loaded;
        }

        private void WebView_Loaded(object sender, EventArgs e)
        {
            webView.EvaluateJavaScriptAsync($"showCaptcha(\"{AppId}\")").Fire();

            //TODO: 如果RegisterAction放在构造函数里，再次显示Dialog时，不会加载
            webView.RegisterCSharpAction(CaptchaCallback);
        }

        private void CaptchaCallback(string json)
        {
            Result = json;

            PoppedDelegate?.Invoke(json).Fire();
            PoppedDelegate = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                Shell.Current.Navigation.PopModalAsync(false).Fire();
            });
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls() => new List<IBaseContentView?> { webView };

        protected override void OnAppearing()
        {
            base.OnAppearing();

           // Application.Current.ModalPopped += TCaptchaDialog_ModalPopped;
        }

        //private static void TCaptchaDialog_ModalPopped(object sender, ModalPoppedEventArgs e)
        //{
        //    if (e.Modal is TCaptchaDialog dialog)
        //    {
        //        Application.Current.ModalPopped -= TCaptchaDialog_ModalPopped;

        //        if (dialog.PoppedDelegate != null)
        //        {
        //            dialog.PoppedDelegate(dialog.Result).Fire();
        //            dialog.PoppedDelegate = null;
        //        }
        //    }
        //}
    }
}