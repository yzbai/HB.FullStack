using System;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    internal interface IRedisInstanceManager : IDisposable
    {
        IDatabase GetDatabase(string instanceName);

        RedisInstanceSetting GetInstanceSetting(string instanceName);
    }
}