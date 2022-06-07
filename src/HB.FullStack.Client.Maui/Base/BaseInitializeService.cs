using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.Maui.Base
{
    public class BaseInitializeService : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {
            GlobalSettings.Logger = services.GetRequiredService<ILogger<BaseInitializeService>>();

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 上报

                GlobalSettings.Logger.LogError(e.Exception, $"发现没有处理的UnobservedTaskException。Sender: {sender?.GetType().FullName}");

                Currents.ShowToast("抱歉，发生了错误");

                e.SetObserved();
            };

            AsyncAwaitBestPractices.SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex =>
            {
                //TODO:上报

                GlobalSettings.Logger.LogError(ex, "使用了SafeFireAndForget的默认异常处理");


                Currents.ShowToast("抱歉，发生了错误");
            });
        }
    }
}