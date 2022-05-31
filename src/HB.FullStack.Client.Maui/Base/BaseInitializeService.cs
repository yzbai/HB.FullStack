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
            GlobalSettings.Logger = services.GetRequiredService<ILogger<BaseApplication>>();

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 设置这个
                e.SetObserved();
            };

            //FileService
            //if (VersionTracking.IsFirstLaunchEver)
            //{
            //    IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            //    AddInitTask(fileManager.UnzipAssetZipAsync(InitAssetFileName));
            //}
        }
    }
}