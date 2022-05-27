using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Client.File;
using HB.FullStack.Client.Network;

using Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.Maui.Base
{

    public class InitializeService : IMauiInitializeService
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

    public class InitalizeScopedService : IMauiInitializeScopedService
    {
        public void Initialize(IServiceProvider services)
        {
            //查看application.current等等是否都已经准备好

            INavigationManager.Current = services.GetRequiredService<INavigationManager>();
        }
    }

    public abstract class BaseApplication : Application//置各种Current
    {
        public new static BaseApplication? Current => (BaseApplication?)Application.Current;

        public abstract void OnOfflineDataUsed();
        
    }
}