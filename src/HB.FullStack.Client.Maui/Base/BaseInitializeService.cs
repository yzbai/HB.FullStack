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
                //TODO: 设置这个

                GlobalSettings.Logger.LogError(e.Exception, $"发现没有处理的UnobservedTaskException。Sender: {sender?.GetType().FullName}");

                //TODO: UI反应，比如显示Toast等等


                e.SetObserved();
            };
        }
    }
}