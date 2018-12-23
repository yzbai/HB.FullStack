using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    internal class RedisConnection
    {
        public string ConnectionString { get; set; }
        public ConnectionMultiplexer Connection { get; set; }
        public IDatabase Database { get; set; }

        public RedisConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Connection = null;
            Database = null;
        }
    }
}
