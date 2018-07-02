using HB.Compnent.Common.ImageCode;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImageCodeServiceCollectionExtensions
    {
        public static IServiceCollection AddImageCode(this IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<ImageCodeOptions>(o=> {});

            serviceCollection.AddSingleton<IImageCodeBiz, ImageCodeBiz>();

            return serviceCollection;
        }

        public static IServiceCollection AddImageCode(this IServiceCollection serviceCollection, Action<ImageCodeOptions> optionSetup)
        {
            serviceCollection.Configure(optionSetup);

            serviceCollection.AddSingleton<IImageCodeBiz, ImageCodeBiz>();

            return serviceCollection;
        }
    }
}
