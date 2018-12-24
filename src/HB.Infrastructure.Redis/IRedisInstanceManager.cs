using System;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    public interface IRedisInstanceManager : IDisposable
    {
        IDatabase GetDatabase(string instanceName, int dbIndex, bool isMaster);
        IDatabase GetReadDatabase(string instanceName, int dbIndex);
        IDatabase GetWriteDatabase(string instanceName, int dbIndex);
        RedisInstanceSetting GetInstanceSetting(string brokerName, bool isMaster);
    }
}