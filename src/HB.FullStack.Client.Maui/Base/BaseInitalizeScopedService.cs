using System;

using HB.FullStack.Client.Navigation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.Maui.Base
{
    public class BaseInitalizeScopedService : IMauiInitializeScopedService
    {
        public void Initialize(IServiceProvider services)
        {
            //TODO: 查看application.current等等是否都已经准备好

            INavigationManager.Current = services.GetRequiredService<INavigationManager>();
        }
    }
}