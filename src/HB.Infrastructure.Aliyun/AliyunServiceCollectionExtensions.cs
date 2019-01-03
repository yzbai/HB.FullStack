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

            foreach (AliyunAccessSetting setting in aliyunOptions.Accesses)
            {
                DefaultProfile profile = DefaultProfile.GetProfile(setting.RegionId, setting.AccessKeyId, setting.AccessKeySecret);

                if (!string.IsNullOrWhiteSpace(setting.Endpoint))
                {
                    DefaultProfile.AddEndpoint(setting.ProductName + setting.RegionId, setting.RegionId, setting.ProductName, setting.Endpoint);
                }

                clientManager.AddClient(setting, profile);
            }

            serviceCollection.AddSingleton<IAcsClientManager>(clientManager);

            return serviceCollection;
        }
    }
}
