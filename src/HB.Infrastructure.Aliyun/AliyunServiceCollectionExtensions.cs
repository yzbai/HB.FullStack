using Aliyun.Acs.Core.Profile;
using HB.Infrastructure.Aliyun;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AliyunServiceCollectionExtensions
    {
        public static IServiceCollection AddAliyunService(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            AliyunOptions aliyunOptions = new AliyunOptions();
            configuration.Bind(aliyunOptions);

            return serviceCollection.AddAliyunService(aliyunOptions);
        }

        public static IServiceCollection AddAliyunService(this IServiceCollection serviceCollections, Action<AliyunOptions> action)
        {
            AliyunOptions options = new AliyunOptions();
            action(options);

            return serviceCollections.AddAliyunService(options);
        }

        public static IServiceCollection AddAliyunService(this IServiceCollection serviceCollection, AliyunOptions aliyunOptions)
        {
            IAcsClientManager clientManager = new DefaultAcsClientManager();

            foreach (var item in aliyunOptions.Products)
            {
                DefaultProfile profile = DefaultProfile.GetProfile(item.RegionId, item.AccessKeyId, item.AccessKeySecret);

                if (!string.IsNullOrWhiteSpace(item.Endpoint))
                {
                    DefaultProfile.AddEndpoint(item.ProductName + item.RegionId, item.RegionId, item.ProductName, item.Endpoint);
                }
            }

            serviceCollection.AddSingleton<IAcsClientManager>(clientManager);

            return serviceCollection;
        }
    }
}
